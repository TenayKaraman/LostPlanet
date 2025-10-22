
using UnityEngine;

namespace LostPlanet.ScriptableObjects
{
    [CreateAssetMenu(menuName = "LostPlanet/AbilityDefinition")]
    public class AbilityDefinition : ScriptableObject
    {
        public string abilityId;   // "Shield","EMP","Phase","Bomb","Freeze"
        public float duration = 3f;
        public float power = 1f;   // Bomb yarıçapı vs.
    }
}
