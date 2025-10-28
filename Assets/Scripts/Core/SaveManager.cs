using UnityEngine;

namespace LostPlanet.Core
{
    /// <summary>
    /// Basit PlayerPrefs sarmalayýcýsý.
    /// Mevcut API korunmuþtur: Save/Load Int/String/Float, Save(), Has/Delete, MarkCompleted.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SaveManager : MonoBehaviour
    {
        public void Init() { /* Gerekirse ileride migrate/upgrade koyarýz */ }

        // ---- Write ----
        public void SaveString(string key, string value) => PlayerPrefs.SetString(key, value);
        public void SaveInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public void SaveFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        // ---- Read ----
        public string LoadString(string key, string def = "") => PlayerPrefs.GetString(key, def);
        public int LoadInt(string key, int def = 0) => PlayerPrefs.GetInt(key, def);
        public float LoadFloat(string key, float def = 0f) => PlayerPrefs.GetFloat(key, def);

        // ---- Utilities ----
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public void DeleteKey(string key) { PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
        public void DeleteAll() { PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); }
        public void Save() => PlayerPrefs.Save();

        /// <summary>Seviye ilerlemesini günceller (en yükseði saklar).</summary>
        public void MarkCompleted(int levelId)
        {
            int existing = LoadInt("level_progress", 0);
            if (levelId > existing) SaveInt("level_progress", levelId);
            Save();
        }
    }
}
