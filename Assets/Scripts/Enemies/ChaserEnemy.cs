
using UnityEngine;

namespace LostPlanet.Enemies
{
    public class ChaserEnemy : EnemyBase
    {
        protected override void Tick()
        {
            var player = FindObjectOfType<LostPlanet.Gameplay.PlayerController>();
            if (player == null) return;
            Vector2Int delta = player.GridPos - GridPos;
            Vector2Int dir = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? new Vector2Int(delta.x > 0 ? 1 : -1, 0)
                                                                      : new Vector2Int(0, delta.y > 0 ? 1 : -1);
            TryStep(dir);
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
