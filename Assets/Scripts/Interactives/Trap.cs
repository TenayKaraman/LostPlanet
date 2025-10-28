using UnityEngine;

namespace LostPlanet.Interactives
{
    using System.Collections.Generic;

    public class Trap : MonoBehaviour
    {
        // Global kayıt: EMP/Freeze gibi işlemler için hızlı erişim
        public static readonly List<Trap> All = new List<Trap>();

        protected bool disabled = false;
        protected bool frozen = false;

        protected virtual void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        protected virtual void OnDisable() { All.Remove(this); }

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
