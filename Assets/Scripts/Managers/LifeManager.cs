using UnityEngine;
using System;
using System.Collections;
using LostPlanet.Core;

namespace LostPlanet.Managers
{
    public class LifeManager : MonoBehaviour
    {
        [Header("Lives")]
        public int current = 5;
        public int max = 5;

        [Header("Refill")]
        public int refillMinutes = 30;            // 1 can / 30 dk
        [SerializeField] long nextRefillUtcTicks; // Inspector'da g�rmek istersen

        DateTime nextRefillUtc;

        [Header("Refs")]
        public SaveManager save;
        public UIManager ui;

        void Awake()
        {
            if (!save) save = FindObjectOfType<SaveManager>();
            if (!ui) ui = FindObjectOfType<UIManager>();
        }

        void Start()
        {
            InitFromSave();             // ilk y�kleme
            StopAllCoroutines();
            StartCoroutine(CoRefillTick()); // arkaplanda saniyelik tick
            UpdateRefillTimerUI();      // ekrana ilk anda yans�s�n
        }

        /// <summary>
        /// Kaydedilmi� veriden can ve refill saatini y�kler; ge�mi� zaman� telafi eder; UI�yi g�nceller.
        /// </summary>
        public void InitFromSave()
        {
            if (!save) save = FindObjectOfType<LostPlanet.Core.SaveManager>();
            if (!ui) ui = FindObjectOfType<LostPlanet.Core.UIManager>();

            // �lk kurulum: e�er hi� kay�t yoksa max�tan ba�la
            int loaded = save != null ? save.LoadInt("life", -1) : -1;
            if (loaded < 0)
            {
                current = max;
                // ilk kez ba�larken refill sayac� ileriye kurulsun
                nextRefillUtc = DateTime.UtcNow.AddMinutes(refillMinutes);
                SaveAll();
            }
            else
            {
                current = Mathf.Clamp(loaded, 0, max);

                // kay�tl� refill zaman�
                string ticksStr = save.LoadString("nextRefillUtc", DateTime.UtcNow.Ticks.ToString());
                long ticks;
                if (!long.TryParse(ticksStr, out ticks)) ticks = DateTime.UtcNow.Ticks;
                nextRefillUtc = new DateTime(ticks, DateTimeKind.Utc);

                // ge�en s�reye g�re doldur
                RefillByElapsedTime(true);
            }

            // UI�yi an�nda g�ncelle
            ui?.UpdateLifeUI(current, max);
            UpdateRefillTimerUI();
        }

        void SaveAll()
        {
            nextRefillUtcTicks = nextRefillUtc.Ticks;
            save?.SaveInt("life", current);
            save?.SaveString("nextRefillUtc", nextRefillUtcTicks.ToString());
        }

        IEnumerator CoRefillTick()
        {
            var wait = new WaitForSeconds(1f);
            while (true)
            {
                RefillByElapsedTime(forceUI: false); // can dolmu�sa gerekti�inde art�r
                UpdateRefillTimerUI();               // her saniye geri say�m� yazd�r
                yield return wait;
            }
        }

        /// <summary>�u ana kadar ge�en s�reye g�re refill uygular.</summary>
        public void RefillByElapsedTime(bool forceUI = false)
        {
            var now = DateTime.UtcNow;
            bool changed = false;

            while (now >= nextRefillUtc && current < max)
            {
                current++;
                nextRefillUtc = nextRefillUtc.AddMinutes(refillMinutes);
                changed = true;
            }

            if (current >= max)
                nextRefillUtc = now;

            if (changed || forceUI)
            {
                SaveAll();
                ui?.UpdateLifeUI(current, max);
            }

            // Geri say�m metnini de g�ncelle
            UpdateRefillTimerUI();
        }

        public bool CanConsume(int n = 1) => current >= n;

        /// <summary>n adet can harcar; gerekirse refill sayac�n� ba�lat�r; UI�yi g�nceller.</summary>
        public bool Consume(int n = 1)
        {
            if (!CanConsume(n)) return false;

            current = Mathf.Max(0, current - n);

            // Refill ihtiyac� varsa zaman� kur
            if (current < max)
            {
                if (DateTime.UtcNow >= nextRefillUtc)
                    nextRefillUtc = DateTime.UtcNow.AddMinutes(refillMinutes);
            }

            (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeUI(current, max);
            SaveAll();
            UpdateRefillTimerUI();
            return true;
        }

        public void AddLife(int n = 1)
        {
            current = Mathf.Min(max, current + n);
            (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeUI(current, max);

            if (current >= max)
                nextRefillUtc = DateTime.UtcNow; // tamamsa saya� beklesin

            SaveAll();
            UpdateRefillTimerUI();
        }

        public void SetMax(int newMax, bool refill = true)
        {
            max = Mathf.Max(1, newMax);
            if (refill) current = max;
            (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeUI(current, max);
            SaveAll();
            UpdateRefillTimerUI();
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                RefillByElapsedTime(forceUI: true);
                // UpdateRefillTimerUI() zaten RefillByElapsedTime i�inde �a�r�l�yor
            }
        }

        void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                RefillByElapsedTime(forceUI: true);
            }
        }

        // -------------------- Helpers --------------------

        void UpdateRefillTimerUI()
        {
            // Canlar doluysa geri say�m� gizle
            if (current >= max)
            {
                (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeRefillTimer(string.Empty);
                return;
            }

            var now = DateTime.UtcNow;
            var remaining = nextRefillUtc - now;

            if (remaining.TotalSeconds <= 0)
            {
                (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeRefillTimer("00:00");
                return;
            }

            int minutes = Mathf.FloorToInt((float)remaining.TotalMinutes);
            int seconds = Mathf.FloorToInt((float)remaining.Seconds);
            (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeRefillTimer($"{minutes:00}:{seconds:00}");
        }
    }
}
