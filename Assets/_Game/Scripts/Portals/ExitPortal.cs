using UnityEngine;
#if DOTWEEN_EXISTS
using DG.Tweening;
#endif
using LostPlanet.Core; // UIManager i�in

namespace LostPlanet.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class ExitPortal : MonoBehaviour
    {
        public string playerTag = "Player";
        public VoidEvent onEnter;   // inspector�dan GameManager.LevelComplete tarz� ba�layabilirsin

        [Header("Visual (optional)")]
        public Transform ring;
        public float ringSpin = 180f;

        void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (GetComponent<BobAndSpin>() == null)
            {
                var bs = gameObject.AddComponent<BobAndSpin>();
                bs.bob = true; bs.spin = false; bs.pulse = true;
            }
        }

        void Update()
        {
            if (ring) ring.Rotate(0, 0, ringSpin * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            // 1) Inspector event
            onEnter?.Invoke();

            // 2) Fallback: UI LevelComplete a� (GameManager entegrasyonun yoksa bile �al���r)
            var ui = FindObjectOfType<UIManager>();
            ui?.ShowLevelComplete();
        }
    }
}
