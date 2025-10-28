using UnityEngine;
using System.Collections;
using LostPlanet.GridSystem;
using LostPlanet.Core;
using LostPlanet.Items;
using UInput = UnityEngine.Input;

#if DOTWEEN_EXISTS
using DG.Tweening;
#endif

namespace LostPlanet.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))] // <- RB2D'yi garanti et
    public class PlayerController : GridEntity
    {
        [Header("Movement")]
        [Tooltip("Bir hücre ilerlemek için geçen süre (sn)")]
        public float stepDuration = 0.15f;

        [Tooltip("Editor/PC testinde klavye ile tetikle")]
        public bool enableKeyboardFallback = true;

        public Vector2Int MoveDir = Vector2Int.zero;
        public bool IsSliding { get; private set; } = false;

        private bool _isDead = false;
        private Coroutine _slideCo;

        public System.Action OnMovedStep;
        public System.Action OnSlideStart;
        public System.Action OnSlideStop;

        void OnEnable()
        {
            LostPlanet.Input.InputManager.OnSwipe += HandleSwipe;
        }

        void OnDisable()
        {
            LostPlanet.Input.InputManager.OnSwipe -= HandleSwipe;
        }

        void Start()
        {
            // Grid garanti
            if (Grid == null) Grid = FindObjectOfType<GridManager>();

            // Fizik bileþenlerini garanti altýna al
            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.isKinematic = true; // transform ile hareket edeceðiz

            if (!TryGetComponent<Collider2D>(out var _)) gameObject.AddComponent<BoxCollider2D>();

            // Hücreye oturt
            GridPos = Grid.WorldToGrid(transform.position);
            Grid.SetOccupant(GridPos, this);

            // Sprite görünür olsun (prefab küçükse vs.)
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                var c = sr.color; c.a = 1f; sr.color = c;
                if (sr.sortingOrder < 5) sr.sortingOrder = 5;
            }
        }

        void Update()
        {
            // Klavye fallback (Editor/PC test)
            if (!enableKeyboardFallback) return;
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            if (IsSliding) return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (UInput.GetKeyDown(KeyCode.UpArrow) || UInput.GetKeyDown(KeyCode.W)) HandleSwipe(Vector2Int.up);
            else if (UInput.GetKeyDown(KeyCode.DownArrow) || UInput.GetKeyDown(KeyCode.S)) HandleSwipe(Vector2Int.down);
            else if (UInput.GetKeyDown(KeyCode.LeftArrow) || UInput.GetKeyDown(KeyCode.A)) HandleSwipe(Vector2Int.left);
            else if (UInput.GetKeyDown(KeyCode.RightArrow) || UInput.GetKeyDown(KeyCode.D)) HandleSwipe(Vector2Int.right);
#endif
        }

        void HandleSwipe(Vector2Int dir)
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
            if (_isDead) return;

            if (!IsSliding)
            {
                MoveDir = dir;
                _slideCo = StartCoroutine(SlideLoop());
                OnSlideStart?.Invoke();
            }
            else
            {
                // Kayarken yeni yönde akmaya devam et
                MoveDir = dir;
            }
        }

        IEnumerator SlideLoop()
        {
            IsSliding = true;

            while (MoveDir != Vector2Int.zero)
            {
                Vector2Int next = GridPos + MoveDir;
                var outcome = Probe(next);
                if (!outcome.canMove)
                {
                    StopSliding();
                    break;
                }

                // Etkileþimler (giriþ hücresinde)
                if (outcome.crystal) outcome.crystal.Collect();

                if (outcome.block && outcome.pushTarget.HasValue)
                {
                    outcome.block.Push(MoveDir, outcome.pushTarget.Value, Grid);
                }

                if (outcome.trap)
                {
                    var inv = GetComponent<AbilityInventory>();
                    bool phase = inv != null && inv.IsPhaseActive();
                    if (!phase)
                    {
                        outcome.trap.Activate();
                        Die();
                        yield break;
                    }
                }

                if (outcome.enemy)
                {
                    var inv = GetComponent<AbilityInventory>();
                    bool phase = inv != null && inv.IsPhaseActive();
                    bool shield = inv != null && inv.HasActiveShield();

                    if (phase || shield)
                    {
                        if (!phase && shield) inv.ConsumeShield();
                        outcome.enemy.Die();
                    }
                    else
                    {
                        Die();
                        yield break;
                    }
                }

                // Adýmý uygula
                yield return StepTo(next);

                OnMovedStep?.Invoke();

                // Portala girdiysek level akýþýný tetikle ve çýk
                if (outcome.portal)
                {
                    outcome.portal.TryComplete();
                    yield break;
                }

                // Bir sonraki adým süresi
                yield return new WaitForSeconds(stepDuration);
            }

            IsSliding = false;
        }

        IEnumerator StepTo(Vector2Int c)
        {
            Grid.SetOccupant(GridPos, null);
            GridPos = c;
            Grid.SetOccupant(GridPos, this);

            Vector3 target = Grid.GridToWorld(GridPos);

#if DOTWEEN_EXISTS
            // Küçük bir tween ile kaydýr
            var t = transform;
            t.DOKill();
            var tween = t.DOMove(target, stepDuration * 0.9f).SetEase(Ease.Linear);
            yield return tween.WaitForCompletion();
#else
            transform.position = target;
            yield return null;
#endif
        }

        void StopSliding()
        {
            MoveDir = Vector2Int.zero;
            IsSliding = false;
            OnSlideStop?.Invoke();
        }

        void Die()
        {
            if (_isDead) return;
            _isDead = true;

            MoveDir = Vector2Int.zero;
            IsSliding = false;
            OnSlideStop?.Invoke();
            StopAllCoroutines();

            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDeath();
        }

        private ProbeResult Probe(Vector2Int c)
        {
            var res = new ProbeResult();

            if (!Grid.IsInside(c) || Grid.IsWall(c))
            {
                res.canMove = false;
                return res;
            }

            var occ = Grid.cells[c.x, c.y].occupant;
            var inv = GetComponent<AbilityInventory>();
            bool phase = inv != null && inv.IsPhaseActive();

            if (occ == null)
            {
                res.canMove = true;
                return res;
            }

            if (occ is Interactives.DoorController door && !door.IsOpen && !phase)
            {
                res.canMove = false;
                return res;
            }

            if (occ is Interactives.Block blk)
            {
                if (phase) { res.canMove = true; return res; }

                Vector2Int t = c + MoveDir;
                if (Grid.IsInside(t) && !Grid.IsCellBlocked(t) && Grid.cells[t.x, t.y].occupant == null)
                {
                    res.canMove = true;
                    res.block = blk;
                    res.pushTarget = t;
                    return res;
                }
                res.canMove = false;
                return res;
            }

            if (occ is Items.Crystal cry) { res.canMove = true; res.crystal = cry; return res; }
            if (occ is Interactives.Trap tr) { res.canMove = true; res.trap = tr; return res; }
            if (occ is Enemies.EnemyBase en) { res.canMove = true; res.enemy = en; return res; }
            if (occ is Items.PortalController po) { res.canMove = true; res.portal = po; return res; }

            res.canMove = true;
            return res;
        }

        struct ProbeResult
        {
            public bool canMove;
            public Items.Crystal crystal;
            public Interactives.Block block;
            public System.Nullable<Vector2Int> pushTarget;
            public Interactives.Trap trap;
            public Enemies.EnemyBase enemy;
            public Items.PortalController portal;
        }
    }
}
