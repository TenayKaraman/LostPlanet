using UnityEngine;
using UnityEngine.UI;
using UInput = UnityEngine.Input;

using TMPro;
#if DOTWEEN_EXISTS
using DG.Tweening;
#endif

namespace LostPlanet.Core
{
    /// <summary>
    /// Oyun içi HUD ve panellerin tek elden kontrolü.
    /// Inspector üzerinden referanslarý atayýn.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        [Header("Ability Slot UI")]
        public Image[] slotIcons = new Image[3];        // ikon görseli
        public Image[] slotCooldown = new Image[3];     // Type=Filled, FillMethod=Radial360
        public LostPlanet.UI.AbilityIconRegistry iconRegistry;

        [Header("HUD Roots / Panels")]
        public GameObject hudRoot;
        public GameObject panelMainMenu;
        public GameObject panelPause;
        public GameObject panelRetry;
        public GameObject panelLevelComplete;

        [Header("TopBar (TMP)")]
        public TMP_Text levelNameText;
        public TMP_Text crystalCounterText;
        public TMP_Text lifeText;
        public TMP_Text lifeRefillTimerText; // TopBar'da göstermek istersen

        [Header("NOS (optional)")]
        public Slider nosSlider;
        public Button nosButton;

        [Header("Ability Buttons (optional)")]
        public Button[] slotButtons = new Button[3];

        [Header("Retry Panel (TMP, optional)")]
        public TMP_Text retryLifeText;                 // Panel_Retry içindeki can metni (opsiyonel)
        public TMP_Text retryLifeRefillTimerText;     // Panel_Retry içindeki sayaç metni (mm:ss)

        [Header("Level Complete (TMP, optional)")]
        public TMP_Text lcCrystalText;   // "Kristal: x/y"
        public TMP_Text lcTimeText;      // "Süre: mm:ss"

        // ---------- Boot / HUD ----------
        /// <summary>HUD varsayýlaný aç, panelleri kapat, buton etkileþimlerini ayarla.</summary>
        public void Init()
        {
            ShowHUD(true);
            HideAllOverlays();
            SetNOSInteractable(false);     // Baþlangýçta kilitli; PlayerNOS hazýr olduðunda açar.
            SetSlotsInteractable(true);
        }

        public void BindLevelHUD()
        {
            ShowHUD(true);
            HideAllOverlays();
        }

        public void ShowMainMenu()
        {
            ShowHUD(false);
            HideAllOverlays();
            if (panelMainMenu) panelMainMenu.SetActive(true);
        }

        public void ShowPause()
        {
            if (panelPause) panelPause.SetActive(true);
            SetSlotsInteractable(false);
            SetNOSInteractable(false);
        }

        public void HidePause()
        {
            if (panelPause) panelPause.SetActive(false);
            SetSlotsInteractable(true);
            SetNOSInteractable(true);
        }

        public void ShowLevelComplete()
        {
            ShowHUD(false);
            HideAllOverlays();
            if (panelLevelComplete) panelLevelComplete.SetActive(true);
        }

        public void ShowRetry()
        {
            ShowHUD(false);
            HideAllOverlays();
            if (panelRetry) panelRetry.SetActive(true);

            // Panel açýlýr açýlmaz: kaydý güncelle ve sayaçlarý tazele
            var lm = LostPlanet.Core.GameManager.Instance?.LifeManager;
            lm?.RefillByElapsedTime(true);
        }

        public void ShowNoLifeOptions()
        {
            // Ayrý bir ekran yoksa Retry panelini kullan
            ShowRetry();
        }

        void HideAllOverlays()
        {
            if (panelMainMenu) panelMainMenu.SetActive(false);
            if (panelPause) panelPause.SetActive(false);
            if (panelRetry) panelRetry.SetActive(false);
            if (panelLevelComplete) panelLevelComplete.SetActive(false);
        }

        void ShowHUD(bool on)
        {
            if (hudRoot) hudRoot.SetActive(on);
        }

        // ---------- TopBar Helpers ----------
        public void SetLevelName(string txt)
        {
            if (levelNameText) levelNameText.text = txt;
        }

        public void UpdateCrystalUI(string id, int collected, int required)
        {
            if (!crystalCounterText) return;
            crystalCounterText.text = $"{collected}/{required}";
        }

        public void UpdateLifeUI(int current, int max)
        {
            if (lifeText) lifeText.text = $"{current}/{max}";
            if (retryLifeText) retryLifeText.text = $"{current}/{max}";
        }

        /// <summary>mm:ss stringi ver; hem Retry hem TopBar sayaçlarýný yönetir.</summary>
        public void UpdateLifeRefillTimer(string s)
        {
            bool show = !string.IsNullOrEmpty(s);

            // Retry panelindeki sayaç
            if (retryLifeRefillTimerText)
            {
                retryLifeRefillTimerText.text = show ? s : "";
                retryLifeRefillTimerText.enabled = show;
            }

            // TopBar (opsiyonel)
            if (lifeRefillTimerText)
            {
                lifeRefillTimerText.text = show ? s : "";
                lifeRefillTimerText.enabled = show;
            }
        }

        // ---------- NOS ----------
        public void UpdateNOSBar(float v01)
        {
            if (nosSlider) nosSlider.normalizedValue = Mathf.Clamp01(v01);
        }

        public void SetNOSInteractable(bool on)
        {
            if (nosButton) nosButton.interactable = on;
        }

        public void PulseNOS()
        {
#if DOTWEEN_EXISTS
            if (nosButton)
            {
                var t = nosButton.transform;
                t.DOKill();
                t.DOPunchScale(Vector3.one * 0.08f, 0.25f, 6, 0.8f);
            }
#endif
        }

        // ---------- Ability Slots ----------
        public void SetAbilitySlot(int index, string abilityIdOrNull)
        {
            if (index < 0 || index >= 3) return;
            var icon = slotIcons[index];
            if (!icon) return;

            if (string.IsNullOrEmpty(abilityIdOrNull))
            {
                if (iconRegistry && iconRegistry.placeholder) icon.sprite = iconRegistry.placeholder;
                icon.color = new Color(1, 1, 1, 0.25f);
            }
            else
            {
                var s = iconRegistry ? iconRegistry.GetSprite(abilityIdOrNull) : null;
                if (s) icon.sprite = s;
                icon.color = Color.white;
            }
        }

        public void ClearAbilitySlot(int index)
        {
            if (index < 0 || index >= 3) return;
            var icon = slotIcons[index];
            if (!icon) return;

            if (iconRegistry && iconRegistry.placeholder) icon.sprite = iconRegistry.placeholder;
            icon.color = new Color(1, 1, 1, 0.25f);
        }

        public void SetSlotCooldown01(int index, float v01)
        {
            if (index < 0 || index >= 3) return;
            var img = slotCooldown[index];
            if (!img) return;

            img.fillAmount = Mathf.Clamp01(v01);
            img.enabled = v01 > 0f && v01 < 1f;
        }

        public void SetSlotsInteractable(bool on)
        {
            if (slotButtons == null) return;
            for (int i = 0; i < slotButtons.Length; i++)
                if (slotButtons[i]) slotButtons[i].interactable = on;
        }

        // ---------- Level Complete ----------
        public void UpdateLevelComplete(int collected, int required, float seconds)
        {
            if (lcCrystalText) lcCrystalText.text = $"Kristal: {collected}/{required}";
            if (lcTimeText) lcTimeText.text = $"Süre: {FormatTime(seconds)}";
        }

        string FormatTime(float sec)
        {
            if (sec < 0) sec = 0;
            int m = Mathf.FloorToInt(sec / 60f);
            int s = Mathf.FloorToInt(sec % 60f);
            return $"{m:00}:{s:00}";
        }
    }
}
