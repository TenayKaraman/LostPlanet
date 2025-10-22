
using UnityEngine;

namespace LostPlanet.Interactives
{
    public class Trap : MonoBehaviour
    {
        protected bool disabled = false;
        protected bool frozen = false;

        public virtual void Activate() { if (disabled) return; }
        public virtual void SetDisabled(bool on) { disabled = on; }
        public virtual void SetFrozen(bool on) { frozen = on; }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (disabled) return;
            if (other.GetComponent<LostPlanet.Gameplay.PlayerController>() != null)
                LostPlanet.Core.GameManager.Instance.OnPlayerDeath();
        }
    }
}
