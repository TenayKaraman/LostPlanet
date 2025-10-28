using UnityEngine;

namespace LostPlanet.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class AbilityBox : MonoBehaviour
    {
        [Header("Config")]
        public string abilityId = "dash";        // inspector’dan set
        public string playerTag = "Player";

        [Header("FX")]
        public GameObject pickupVfxPrefab;
        public AudioClip pickupSfx;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;

        [Header("Events")]
        public StringEvent onAbilityPicked; // abilityId parametresi ile

        bool _taken;

        void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
            var bs = GetComponent<BobAndSpin>() ?? gameObject.AddComponent<BobAndSpin>();
            bs.pulse = true; bs.bob = true; bs.spin = false; // kutularda hafif bob+pulse hoþ durur
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_taken) return;
            if (!other.CompareTag(playerTag)) return;
            Pickup();
        }

        void Pickup()
        {
            _taken = true;

            if (pickupVfxPrefab)
                Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);
            if (pickupSfx)
                AudioSource.PlayClipAtPoint(pickupSfx, transform.position, sfxVolume);

            onAbilityPicked?.Invoke(abilityId);  // UIManager/GameManager’da baðla (slot atama, kayýt vs.)

            ToggleRender(false);
            Destroy(gameObject, 0.05f);
        }

        void ToggleRender(bool on)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = on;
            var col2D = GetComponent<Collider2D>();
            if (col2D) col2D.enabled = on;
        }
    }
}
