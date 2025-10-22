
using UnityEngine;

namespace LostPlanet.Interactives
{
    public class SpikeTrap : Trap
    {
        public override void Activate(){ if (disabled) return; /* optional anim */ }
    }
}
