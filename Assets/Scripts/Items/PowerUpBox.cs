
using UnityEngine;
using LostPlanet.ScriptableObjects;
using LostPlanet.Gameplay;

namespace LostPlanet.Items
{
    public class PowerUpBox : MonoBehaviour
    {
        public bool hasKnownCard = false;
        public AbilityDefinition knownCard;
        public AbilityPoolDefinition pool;

        void OnTriggerEnter2D(Collider2D other)
        {
            var inv = other.GetComponent<AbilityInventory>();
            if (!inv) return;

            // 1) Bilinen kart varsa onu ver, yoksa pool’dan rastgele
            AbilityDefinition picked = hasKnownCard && knownCard ? knownCard
                                       : pool ? pool.PickRandom() : null;

            if (inv.AddCard(picked))
            {
                Debug.Log($"[PowerUpBox] Gave: {picked?.abilityId}");
                Destroy(gameObject);
            }
        }

    }
}
