
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LostPlanet.Dev
{
    public static class SceneValidator
    {
        [MenuItem("Tools/LostPlanet/Validate Scene")]
        public static void Validate()
        {
            int issues = 0;

            // Zorunlu tekiller
            var gm = Object.FindObjectOfType<LostPlanet.Core.GameManager>();
            if (!gm) { Debug.LogError("[Validate] GameManager bulunamadı."); issues++; }

            var ui = Object.FindObjectOfType<LostPlanet.Core.UIManager>();
            if (!ui) { Debug.LogWarning("[Validate] UIManager sahnede yok (bootstrap ile gelebilir)."); }

            var grid = Object.FindObjectOfType<LostPlanet.GridSystem.GridManager>();
            if (!grid) { Debug.LogError("[Validate] GridManager bulunamadı."); issues++; }

            // Player var mı?
            var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (!player) Debug.LogWarning("[Validate] Player bulunamadı (GameManager safe-spawn eder).");

            // NOS UI kontrolü (opsiyonel)
            if (ui && ui.nosButton == null) Debug.LogWarning("[Validate] UI nosButton atanmamış olabilir.");

            // Crystal/Portal ilişkisi
            var cm = Object.FindObjectOfType<LostPlanet.Managers.CrystalManager>();
            if (cm && cm.portal == null) Debug.LogWarning("[Validate] CrystalManager.portal atanmamış.");

            EditorUtility.DisplayDialog("LostPlanet Scene Validator",
                issues == 0 ? "Her şey yolunda görünüyor ✅" : $"Tamamlandı. Hata sayısı: {issues}",
                "OK");
        }
    }
}
#endif
