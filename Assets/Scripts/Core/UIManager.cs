using UnityEngine;
using UnityEngine.UI;
// Eðer TextMeshPro kullanýyorsan bu satýrý aç:
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
        public Button[] slotButtons = new Button[3]; // atanmasa da çalýþýr

        [Header("TopBar References")]
        public Image crystalIcon;
        public TMP_Text crystalText;      // ör: "0 / 3"
        public TMP_Text levelNameText;    // ör: "Level 1"
        public Image heartIcon;
        public TMP_Text lifeText;         // ör: "x 3"
        public Slider nosSlider;          // 0..1

        [Header("Colors")]
        public Color filledColor = Color.white;
        public Color emptyColor = new Color(1f, 1f, 1f, 0.25f);

        void Awake()
        {
            Init(); // slotlarý ve topbar'ý güvenli baþlat
        }

        // ---------- Lifecycle / Screens (placeholder) ----------
        public void Init()
        {
            // Ability slotlarý boþ görünümle baþlat
            for (int i = 0; i < 3; i++)
            {
                ClearAbilitySlot(i);
                SetSlotCooldown01(i, 0f);
            }

            // TopBar baþlangýç deðerleri
            if (levelNameText) levelNameText.text = "";
            if (crystalText) crystalText.text = "0 / 0";
            if (lifeText) lifeText.text = "x 0";
            if (nosSlider)
            {
                nosSlider.minValue = 0f;
                nosSlider.maxValue = 1f;
                nosSlider.value = 0f;
                nosSlider.interactable = false; // sadece gösterge
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

        // id paramý ileride çoklu kristal türü olursa iþine yarar; þimdilik kullanmýyoruz
        public void UpdateCrystalUI(string id, int collected, int required)
        {
            if (crystalText) crystalText.text = $"{collected} / {required}";
        }

        public void UpdateLifeUI(int current, int max)
        {
            // Ýstenilen biçim: Kalp ikonu + yanýnda sayý
            if (lifeText) lifeText.text = $"x {current}";
        }

        public void UpdateNOSBar(float v01)
        {
            if (nosSlider) nosSlider.value = Mathf.Clamp01(v01);
        }

        // NOS hazýr olunca (örn. %100) etkileþim açmak istersen burada tetikle
        public void SetNOSInteractable(bool on)
        {
            // Slider sadece gösterge; buton kullanýyorsan orada aç/kapat.
            // Yine de istersen slider'ý görsel olarak vurgulayabilirsin.
            // (Boþ býrakýlmasý sorun deðil.)
        }

        public void PulseNOS()
        {
            // DOTween ile kýsa bir vurgulama (opsiyonel)
            // if (nosSlider) nosSlider.transform.DOPunchScale(Vector3.one * 0.08f, 0.2f, 6, 0.8f);
        }

        // ---------- Ability Slots ----------
        // abilityIdOrNull null/empty ise slotu temizler; doluysa ikon ve etkileþim açýlýr
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

        // AbilityInventory tarafýndan boþaltma/temizleme için çaðrýlýr
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

        // 0..1 arasý cooldown overlay
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
