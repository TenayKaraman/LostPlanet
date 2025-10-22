
using UnityEngine;

namespace LostPlanet.ScriptableObjects
{
    [CreateAssetMenu(menuName = "LostPlanet/AbilityPool")]
    public class AbilityPoolDefinition : ScriptableObject
    {
        public AbilityDefinition[] abilities;
        public AbilityDefinition PickRandom()
        {
            if (abilities == null || abilities.Length == 0) return null;
            int i = Random.Range(0, abilities.Length);
            return abilities[i];
        }
    }
}
