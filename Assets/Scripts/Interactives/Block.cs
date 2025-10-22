
using UnityEngine;
using LostPlanet.GridSystem;

namespace LostPlanet.Interactives
{
    public class Block : GridEntity
    {
        public void Push(Vector2Int dir, Vector2Int target, GridManager grid)
        {
            grid.SetOccupant(GridPos, null);
            GridPos = target;
            grid.SetOccupant(GridPos, this);
            transform.position = grid.GridToWorld(GridPos);
        }
    }
}
