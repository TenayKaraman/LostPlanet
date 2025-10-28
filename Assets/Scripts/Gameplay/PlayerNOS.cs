using UnityEngine;
using LostPlanet.Core;

namespace LostPlanet.Gameplay
{
    public class PlayerNOS : MonoBehaviour
    {
        public float cleanSlideThreshold = 10f;
        public float cleanSlideTime = 0f;
        public bool isReady = false;
        public bool isDashing = false;

        private PlayerController _pc;
        private GridSystem.GridManager _grid;
        bool _sliding = false;

        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _grid = FindObjectOfType<GridSystem.GridManager>();
        }

        void OnEnable()
        {
            if (_pc)
            {
                _pc.OnSlideStart += OnSlideStart;
                _pc.OnSlideStop += OnSlideStop;
            }
            LostPlanet.Input.InputManager.OnSuperPressed += TryActivate;

        }

        void OnDisable()
        {
            if (_pc)
            {
                _pc.OnSlideStart -= OnSlideStart;
                _pc.OnSlideStop -= OnSlideStop;
            }
            LostPlanet.Input.InputManager.OnSuperPressed -= TryActivate;
        }

        void OnSlideStart() { _sliding = true; }

        void OnSlideStop()
        {
            _sliding = false;
            if (!isReady) cleanSlideTime = 0f;
            UpdateBar();
        }

        void Update()
        {
            var gm = LostPlanet.Core.GameManager.Instance;
            if (gm != null && gm.State != LostPlanet.Core.GameState.Playing) return; // <-- EKLENDÝ

            if (_sliding && !isReady)
            {
                cleanSlideTime += Time.deltaTime;
                UpdateBar();
                if (cleanSlideTime >= cleanSlideThreshold)
                {
                    isReady = true;
                    GameManager.Instance?.UIManager?.SetNOSInteractable(true);
                    GameManager.Instance?.UIManager?.PulseNOS();
                }
            }
        }

        void UpdateBar()
        {
            float v = Mathf.Clamp01(cleanSlideTime / Mathf.Max(0.01f, cleanSlideThreshold));
            GameManager.Instance?.UIManager?.UpdateNOSBar(v);
        }

        // UI butonundan da çaðrýlabilsin
        public void UseNOS() => TryActivate();

        void TryActivate()
        {
            if (!isReady || isDashing) return;
            StartCoroutine(CoDash());
        }

        System.Collections.IEnumerator CoDash()
        {
            isDashing = true;
            isReady = false;
            cleanSlideTime = 0f;

            GameManager.Instance?.UIManager?.SetNOSInteractable(false);
            GameManager.Instance?.UIManager?.UpdateNOSBar(0f);

            Vector2Int dir = _pc.MoveDir == Vector2Int.zero ? Vector2Int.right : _pc.MoveDir;

            while (true)
            {
                var next = _pc.GridPos + dir;
                if (!_grid.IsInside(next) || _grid.IsWall(next)) break;

                var occ = _grid.cells[next.x, next.y].occupant;

                if (occ is Enemies.EnemyBase e)
                {
                    if (e.isBoss) e.TakeDashDamage(1);
                    else e.Die();
                }
                else if (occ is Interactives.Block || occ is Interactives.DoorController || occ is Interactives.Trap)
                {
                    break; // sert engelde dur
                }

                _pc.SendMessage("StepTo", next, SendMessageOptions.DontRequireReceiver);
                yield return new WaitForSeconds(_pc.stepDuration * 0.2f);
            }

            isDashing = false;
        }
    }
}
