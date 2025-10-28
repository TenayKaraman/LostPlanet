using UnityEngine;
using UInput = UnityEngine.Input;

namespace LostPlanet.GridSystem
{
    public class GridManager : MonoBehaviour
    {
        public int width = 9;    // PORTRAIT: 9 sütun
        public int height = 16;   // PORTRAIT: 16 satır
        public float cellSize = 1f;

        public CellData[,] cells;

        [Header("World Origin (Grid (0,0) dünya konumu)")]
        public Vector2 origin;  // (0,0) hücresinin world konumu

        public void RecalculateOrigin()
        {
            // Grid’i dünya merkezine ortala (0,0 dünya ≈ grid orta hücre)
            origin = new Vector2(-(width - 1) * 0.5f * cellSize,
                                 -(height - 1) * 0.5f * cellSize);
        }

        void Awake()
        {
            cells = new CellData[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    cells[x, y] = new CellData();

            RecalculateOrigin();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Editörde width/height değişince merkezde kalsın
            RecalculateOrigin();
        }
#endif

        public void BuildFromScene() { }

        public Vector2Int WorldToGrid(Vector3 p)
        {
            int x = Mathf.RoundToInt((p.x - origin.x) / cellSize);
            int y = Mathf.RoundToInt((p.y - origin.y) / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorld(Vector2Int c)
        {
            return new Vector3(origin.x + c.x * cellSize,
                               origin.y + c.y * cellSize, 0f);
        }

        public bool IsInside(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;
        public bool IsWall(Vector2Int c) => !IsInside(c) || cells[c.x, c.y].hasWall;

        public bool IsCellBlocked(Vector2Int c)
        {
            if (!IsInside(c)) return true;
            if (cells[c.x, c.y].hasWall) return true;
            if (cells[c.x, c.y].occupant is Interactives.DoorController door && !door.IsOpen) return true;
            return false;
        }

        public void SetOccupant(Vector2Int c, MonoBehaviour entity)
        {
            if (!IsInside(c)) return;
            cells[c.x, c.y].occupant = entity;
        }
    }

    public class CellData
    {
        public bool hasWall = false;
        public MonoBehaviour occupant = null;
    }
}
