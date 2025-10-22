using UnityEngine;
using UnityEngine.UI;
// E�er TextMeshPro kullan�yorsan bu sat�r� a�:
using TMPro;

namespace LostPlanet.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("Ability Slot UI (assign in scene)")]
        public Image[] slotIcons = new Image[3];
        public Image[] slotCooldown = new Image[3]; // Type=Filled, Radial360

        [Header("Optional Icon Registry")]
        public LostPlanet.UI.AbilityIconRegistry iconRegistry;

        [Header("Optional Buttons (assign in scene)")]
        public Button[] slotButtons = new Button[3]; // atanmasa da �al���r

        [Header("TopBar References")]
        public Image crystalIcon;
        public TMP_Text crystalText;      // �r: "0 / 3"
        public TMP_Text levelNameText;    // �r: "Level 1"
        public Image heartIcon;
        public TMP_Text lifeText;         // �r: "x 3"
        public Slider nosSlider;          // 0..1

        [Header("Colors")]
        public Color filledColor = Color.white;
        public Color emptyColor = new Color(1f, 1f, 1f, 0.25f);

        void Awake()
        {
            Init(); // slotlar� ve topbar'� g�venli ba�lat
        }

        // ---------- Lifecycle / Screens (placeholder) ----------
        public void Init()
        {
            // Ability slotlar� bo� g�r�n�mle ba�lat
            for (int i = 0; i < 3; i++)
            {
                ClearAbilitySlot(i);
                SetSlotCooldown01(i, 0f);
            }

            // TopBar ba�lang�� de�erleri
            if (levelNameText) levelNameText.text = "";
            if (crystalText) crystalText.text = "0 / 0";
            if (lifeText) lifeText.text = "x 0";
            if (nosSlider)
            {
                nosSlider.minValue = 0f;
                nosSlider.maxValue = 1f;
                nosSlider.value = 0f;
                nosSlider.interactable = false; // sadece g�sterge
            }
        }

        public void BindLevelHUD() { }
        public void ShowMainMenu() { }
        public void ShowPause() { }
        public void HidePause() { }
        public void ShowLevelComplete() { }
        public void ShowRetry() { }
        public void ShowNoLifeOptions() { }

        // ---------- TopBar API ----------
        public void SetLevelName(string name)
        {
            if (levelNameText) levelNameText.text = name;
        }

        // id param� ileride �oklu kristal t�r� olursa i�ine yarar; �imdilik kullanm�yoruz
        public void UpdateCrystalUI(string id, int collected, int required)
        {
            if (crystalText) crystalText.text = $"{collected} / {required}";
        }

        public void UpdateLifeUI(int current, int max)
        {
            // �stenilen bi�im: Kalp ikonu + yan�nda say�
            if (lifeText) lifeText.text = $"x {current}";
        }

        public void UpdateNOSBar(float v01)
        {
            if (nosSlider) nosSlider.value = Mathf.Clamp01(v01);
        }

        // NOS haz�r olunca (�rn. %100) etkile�im a�mak istersen burada tetikle
        public void SetNOSInteractable(bool on)
        {
            // Slider sadece g�sterge; buton kullan�yorsan orada a�/kapat.
            // Yine de istersen slider'� g�rsel olarak vurgulayabilirsin.
            // (Bo� b�rak�lmas� sorun de�il.)
        }

        public void PulseNOS()
        {
            // DOTween ile k�sa bir vurgulama (opsiyonel)
            // if (nosSlider) nosSlider.transform.DOPunchScale(Vector3.one * 0.08f, 0.2f, 6, 0.8f);
        }

        // ---------- Ability Slots ----------
        // abilityIdOrNull null/empty ise slotu temizler; doluysa ikon ve etkile�im a��l�r
        public void SetAbilitySlot(int index, string abilityIdOrNull)
        {
            if (index < 0 || index >= 3) return;

            if (string.IsNullOrEmpty(abilityIdOrNull))
            {
                ClearAbilitySlot(index);
                return;
            }

            var icon = slotIcons != null && index < slotIcons.Length ? slotIcons[index] : null;
            if (!icon) return;

            Sprite s = iconRegistry ? iconRegistry.GetSprite(abilityIdOrNull) : null;
            if (!s && iconRegistry) s = iconRegistry.placeholder;

            icon.sprite = s;
            icon.color = filledColor;

            if (slotButtons != null && index < slotButtons.Length && slotButtons[index])
                slotButtons[index].interactable = true;
        }

        // AbilityInventory taraf�ndan bo�altma/temizleme i�in �a�r�l�r
        public void ClearAbilitySlot(int index)
        {
            if (index < 0 || index >= 3) return;

            var icon = slotIcons != null && index < slotIcons.Length ? slotIcons[index] : null;
            if (icon)
            {
                icon.sprite = iconRegistry ? iconRegistry.placeholder : null;
                icon.color = emptyColor;
            }

            if (slotButtons != null && index < slotButtons.Length && slotButtons[index])
                slotButtons[index].interactable = false;

            if (slotCooldown != null && index < slotCooldown.Length && slotCooldown[index])
            {
                slotCooldown[index].fillAmount = 0f;
                slotCooldown[index].enabled = false;
            }
        }

        // 0..1 aras� cooldown overlay
        public void SetSlotCooldown01(int index, float v01)
        {
            if (index < 0 || index >= 3) return;
            var img = slotCooldown != null && index < slotCooldown.Length ? slotCooldown[index] : null;
            if (!img) return;

            img.fillAmount = Mathf.Clamp01(v01);
            img.enabled = v01 > 0f && v01 < 1f;
        }
    }
}
