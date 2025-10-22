
using UnityEngine;

namespace LostPlanet.UI
{
    [CreateAssetMenu(menuName = "LostPlanet/UI/AbilityIconRegistry")]
    public class AbilityIconRegistry : ScriptableObject
    {
        [System.Serializable]
        public struct Entry{ public string abilityId; public Sprite sprite; }

        public Sprite placeholder;
        public Entry[] entries;

        public Sprite GetSprite(string id)
        {
            if (entries == null) return placeholder;
            for (int i = 0; i < entries.Length; i++)
                if (entries[i].abilityId == id) return entries[i].sprite ? entries[i].sprite : placeholder;
            return placeholder;
        }
    }
}
