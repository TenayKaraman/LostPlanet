using UnityEngine;

namespace LostPlanet.UI
{
    [DisallowMultipleComponent]
    public class UIButtons : MonoBehaviour
    {
        // Retry panelindeki "Yeniden Dene" butonu
        public void Retry()
        {
            var gm = LostPlanet.Core.GameManager.Instance;
            if (gm != null) gm.RestartLevel();
        }

        // Pause panelindeki "Devam" butonu
        public void Resume()
        {
            var gm = LostPlanet.Core.GameManager.Instance;
            if (gm != null) gm.ResumeGame();
        }

        // "Ana Menü" butonu
        public void MainMenu()
        {
            var ui = FindObjectOfType<LostPlanet.Core.UIManager>();
            if (ui != null) ui.ShowMainMenu();
        }

        // Level complete'te "Sonraki" örneği
        public void NextLevel()
        {
            var gm = LostPlanet.Core.GameManager.Instance;
            if (gm != null) gm.StartLevel(gm.CurrentLevelId + 1);
        }
        // UIButtons.cs içine ek
        public void Pause()
        {
            var gm = LostPlanet.Core.GameManager.Instance;
            if (gm != null) gm.PauseGame();
        }

        public void RestartFromPause()
        {
            var gm = LostPlanet.Core.GameManager.Instance;
            if (gm != null) gm.RestartLevel(); // tasarım gereği −1 can harcar
        }

    }
}
