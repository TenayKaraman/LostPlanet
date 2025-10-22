using UnityEngine;
using System.Collections;
using LostPlanet.GridSystem;
using LostPlanet.Core;
using LostPlanet.Items;

namespace LostPlanet.Gameplay
{
    public class PlayerController : GridEntity
    {
        public float stepDuration = 0.15f;
        public Vector2Int MoveDir = Vector2Int.zero;
        public bool IsSliding { get; private set; } = false;

        public System.Action OnMovedStep;
        public System.Action OnSlideStart;
        public System.Action OnSlideStop;

        private Coroutine _slideCo;

        void OnEnable() { LostPlanet.Input.InputManager.OnSwipe += HandleSwipe; }
        void OnDisable() { LostPlanet.Input.InputManager.OnSwipe -= HandleSwipe; }

        void Start()
        {
            if (Grid == null) Grid = FindObjectOfType<GridManager>();
            GridPos = Grid.WorldToGrid(transform.position);
            Grid.SetOccupant(GridPos, this);
        }

        void HandleSwipe(Vector2Int dir)
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
            if (!IsSliding) { MoveDir = dir; _slideCo = StartCoroutine(SlideLoop()); OnSlideStart?.Invoke(); }
            else { MoveDir = dir; }
        }

        IEnumerator SlideLoop()
        {
            IsSliding = true;
            while (MoveDir != Vector2Int.zero)
            {
                Vector2Int next = GridPos + MoveDir;
                var outcome = Probe(next);
                if (!outcome.canMove) { StopSliding(); break; }

                if (outcome.crystal) outcome.crystal.Collect();
                if (outcome.block && outcome.pushTarget.HasValue) outcome.block.Push(MoveDir, outcome.pushTarget.Value, Grid);
                if (outcome.trap) { if (!GetComponent<AbilityInventory>()?.IsPhaseActive() ?? true) { outcome.trap.Activate(); Die(); yield break; } }
                if (outcome.enemy)
                {
                    var inv = GetComponent<AbilityInventory>();
                    if (inv != null && (inv.HasActiveShield() || inv.IsPhaseActive())) { if (!inv.IsPhaseActive()) inv.ConsumeShield(); outcome.enemy.Die(); }
                    else { Die(); yield break; }
                }

                StepTo(next);
                OnMovedStep?.Invoke();

                if (outcome.portal) { outcome.portal.TryComplete(); yield break; }
                yield return new WaitForSeconds(stepDuration);
            }
            IsSliding = false;
        }

        void StepTo(Vector2Int c)
        {
            Grid.SetOccupant(GridPos, null);
            GridPos = c;
            Grid.SetOccupant(GridPos, this);
            transform.position = Grid.GridToWorld(GridPos);
        }

        void StopSliding()
        {
            MoveDir = Vector2Int.zero;
            IsSliding = false;
            OnSlideStop?.Invoke();
        }

        void Die()
        {
            StopAllCoroutines();
            if (GameManager.Instance != null) GameManager.Instance.OnPlayerDeath();
        }

        private ProbeResult Probe(Vector2Int c)
        {
            var res = new ProbeResult();
            if (!Grid.IsInside(c) || Grid.IsWall(c)) { res.canMove = false; return res; }
            var occ = Grid.cells[c.x, c.y].occupant;
            var inv = GetComponent<AbilityInventory>();
            bool phase = inv != null && inv.IsPhaseActive();

            if (occ == null) { res.canMove = true; return res; }
            if (occ is Interactives.DoorController door && !door.IsOpen && !phase) { res.canMove = false; return res; }
            if (occ is Interactives.Block blk)
            {
                if (phase) { res.canMove = true; return res; }
                Vector2Int t = c + MoveDir;
                if (Grid.IsInside(t) && !Grid.IsCellBlocked(t) && Grid.cells[t.x, t.y].occupant == null) { res.canMove = true; res.block = blk; res.pushTarget = t; return res; }
                res.canMove = false; return res;
            }
            if (occ is Items.Crystal cry) { res.canMove = true; res.crystal = cry; return res; }
            if (occ is Interactives.Trap tr) { res.canMove = true; res.trap = tr; return res; }
            if (occ is Enemies.EnemyBase en) { res.canMove = true; res.enemy = en; return res; }
            if (occ is Items.PortalController po) { res.canMove = true; res.portal = po; return res; } // <- düzeltildi
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
            public Items.PortalController portal; // <- düzeltildi
        }
    }
}
