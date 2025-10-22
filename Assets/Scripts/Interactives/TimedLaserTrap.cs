
using UnityEngine;
using System.Collections;

namespace LostPlanet.Interactives
{
    public class TimedLaserTrap : Trap
    {
        public float onTime = 1.5f;
        public float offTime = 1.0f;
        public Collider2D beamCollider;
        public GameObject beamVisual;

        void Start(){ StartCoroutine(Loop()); }

        IEnumerator Loop()
        {
            while (true)
            {
                if (!disabled && !frozen) SetBeam(true);
                else SetBeam(false);
                yield return new WaitForSeconds(onTime);
                SetBeam(false);
                yield return new WaitForSeconds(offTime);
            }
        }

        void SetBeam(bool on)
        {
            if (beamCollider) beamCollider.enabled = on;
            if (beamVisual) beamVisual.SetActive(on);
        }

        public override void SetDisabled(bool on){ base.SetDisabled(on); if (on) SetBeam(false); }
        public override void SetFrozen(bool on){ base.SetFrozen(on); if (on) SetBeam(false); }
    }
}
