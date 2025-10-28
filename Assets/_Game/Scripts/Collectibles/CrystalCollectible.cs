using UnityEngine;
using UnityEngine.Events;

namespace LostPlanet.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class CrystalCollectible : MonoBehaviour
    {
        [Header("Who can collect")]
        public string playerTag = "Player";

        [Header("FX (optional)")]
        public GameObject pickupVfxPrefab;
        public AudioClip pickupSfx;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;

        [Header("Events")]
        public VoidEvent onCollected;
        public IntEvent onAddCrystal;   // kaç adet eklendi (genelde 1)

        bool _taken;

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            c.isTrigger = true;
            if (GetComponent<BobAndSpin>() == null)
            {
                gameObject.AddComponent<BobAndSpin>();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_taken) return;
            if (!other.CompareTag(playerTag)) return;
            Collect();
        }

        void OnTriggerEnter(Collider other)
        {
            if (_taken) return;
            if (!other.CompareTag(playerTag)) return;
            Collect();
        }

        void Collect()
        {
            _taken = true;

            // FX
            if (pickupVfxPrefab)
                Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);

            if (pickupSfx)
                AudioSource.PlayClipAtPoint(pickupSfx, transform.position, sfxVolume);

            // Events → inspector’dan GameManager/SaveManager bağlayabilirsin.
            onCollected?.Invoke();
            onAddCrystal?.Invoke(1);

            // Render/collider kapat, sonra yok et
            ToggleRender(false);
            Destroy(gameObject, 0.05f);
        }

        void ToggleRender(bool on)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = on;
            var col2D = GetComponent<Collider2D>();
            if (col2D) col2D.enabled = on;
            var col3D = GetComponent<Collider>();
            if (col3D) col3D.enabled = on;
        }
    }
}
