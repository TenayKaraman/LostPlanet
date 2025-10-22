
using UnityEngine;

namespace LostPlanet.GridSystem
{
    public class GridEntity : MonoBehaviour
    {
        public Vector2Int GridPos;
        public GridManager Grid;

        public virtual void SnapToGrid()
        {
            if (Grid == null) return;
            transform.position = Grid.GridToWorld(GridPos);
        }
    }
}
