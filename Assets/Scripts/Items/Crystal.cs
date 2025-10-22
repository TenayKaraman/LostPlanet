using UnityEngine;

namespace LostPlanet.Items
{
    // Tek tip kristal: sadece portalı açmaya yarar
    public class Crystal : MonoBehaviour
    {
        // DIŞARIDAN çağrılabilen güvenli toplama metodu
        public void Collect()
        {
            var grid = FindObjectOfType<LostPlanet.GridSystem.GridManager>();
            if (grid != null)
            {
                var cell = grid.WorldToGrid(transform.position);
                if (grid.IsInside(cell) && grid.cells[cell.x, cell.y].occupant == (MonoBehaviour)this)
                    grid.SetOccupant(cell, null);
            }

            var cm = FindObjectOfType<LostPlanet.Managers.CrystalManager>();
            cm?.CollectOne();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<LostPlanet.Gameplay.PlayerController>();
            if (player == null) return;
            Collect(); // tetiklemeden geldiğinde de aynı yolu kullan
        }
    }
}
