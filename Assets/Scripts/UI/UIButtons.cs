using UnityEngine;
using UInput = UnityEngine.Input;

namespace LostPlanet.UI
{
    [DisallowMultipleComponent]
    public class UIButtons : MonoBehaviour
    {

        [SerializeField] GameObject levelCompletePanel;
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
            if (levelCompletePanel) levelCompletePanel.SetActive(false);
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
        public void RestartFromComplete()
        {
            var gm = LostPlanet.Core.GameManager.Instance;
            if (gm != null) gm.RestartLevel();
            if (levelCompletePanel) levelCompletePanel.SetActive(false);
        }

        public void MainMenuFromComplete()
        {
            var ui = FindObjectOfType<LostPlanet.Core.UIManager>();
            if (ui != null) ui.ShowMainMenu();
            if (levelCompletePanel) levelCompletePanel.SetActive(false);
        }
    }
}
