
using UnityEngine;
using LostPlanet.Items;
using UInput = UnityEngine.Input;

namespace LostPlanet.Managers
{
    public class CrystalManager : MonoBehaviour
    {
        public int requiredTotal = 3;
        public int collectedTotal = 0;

        public LostPlanet.Items.PortalController portal;
        public LostPlanet.Core.UIManager ui;

        void Awake(){ if (!ui) ui = FindObjectOfType<LostPlanet.Core.UIManager>(); }

        public void InitLevelRequirements(int required = -1)
        {
            collectedTotal = 0;
            if (required >= 0) requiredTotal = required;
            UpdateUI();
            if (portal) portal.isActive = false;
        }

        public void CollectOne()
        {
            collectedTotal++;
            UpdateUI();
            if (collectedTotal >= requiredTotal) portal?.Activate();
        }

        void UpdateUI(){ ui?.UpdateCrystalUI("Crystal", collectedTotal, requiredTotal); }
    }
}
