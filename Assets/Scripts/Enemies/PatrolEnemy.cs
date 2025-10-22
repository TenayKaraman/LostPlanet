
using UnityEngine;

namespace LostPlanet.Enemies
{
    public class PatrolEnemy : EnemyBase
    {
        public Vector2Int[] waypoints;
        int _i;
        protected override void Tick()
        {
            if (waypoints == null || waypoints.Length == 0) return;
            var target = waypoints[_i];
            Vector2Int dir = new Vector2Int(Mathf.Clamp(target.x - GridPos.x, -1, 1), Mathf.Clamp(target.y - GridPos.y, -1, 1));
            TryStep(dir);
            if (GridPos == target) _i = (_i + 1) % waypoints.Length;
        }

        void TryStep(Vector2Int dir)
        {
            var next = GridPos + dir;
            if (!Grid.IsInside(next) || Grid.IsWall(next)) return;
            var occ = Grid.cells[next.x, next.y].occupant;
            if (occ == null)
            {
                Grid.SetOccupant(GridPos, null);
                GridPos = next;
                Grid.SetOccupant(GridPos, this);
                transform.position = Grid.GridToWorld(GridPos);
            }
            else if (occ.GetComponent<LostPlanet.Gameplay.PlayerController>() != null)
            {
                LostPlanet.Core.GameManager.Instance.OnPlayerDeath();
            }
        }
    }
}
