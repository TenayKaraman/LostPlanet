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
        public int refillMinutes = 30;        // 1 can / 30 dk
        [SerializeField] long nextRefillUtcTicks; // Inspector�da g�rmek istersen

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
            InitFromSave();            // ilk y�kleme
            StopAllCoroutines();
            StartCoroutine(CoRefillTick()); // arkaplanda dakika sayac�
        }

        /// <summary> Kaydedilmi� veriden can ve refill saatini y�kler; ge�mi� zaman� telafi eder; UI�y� g�nceller. </summary>
        public void InitFromSave()
        {
            // 1) �lk kez �al���yorsa can� max yap
            if (save != null && save.HasKey("life"))
            {
                current = save.LoadInt("life", max); // daha �nce ne kaydedildiyse onu al
            }
            else
            {
                current = max;                       // �LK KURULUM: full can
                SaveAll();
            }

            // 2) Refill zaman�n� oku veya ba�lat
            if (save != null && save.HasKey("nextRefillUtc"))
            {
                var ticksStr = save.LoadString("nextRefillUtc", "0");
                long ticks; if (!long.TryParse(ticksStr, out ticks)) ticks = 0;
                nextRefillUtc = ticks > 0 ? new DateTime(ticks, DateTimeKind.Utc) : DateTime.UtcNow;
            }
            else
            {
                nextRefillUtc = DateTime.UtcNow;
                SaveAll();
            }

            // 3) Ge�mi� s�reye g�re telafi + UI
            RefillByElapsedTime();
            GameManager.Instance?.UIManager?.UpdateLifeUI(current, max);
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
                RefillByElapsedTime(forceUI: false);
                yield return wait;
            }
        }

        /// <summary> �u ana kadar ge�en s�reye g�re refill uygular. </summary>
        public void RefillByElapsedTime(bool forceUI = false)
        {
            var now = DateTime.UtcNow;
            bool changed = false;

            if (current < max)
            {
                // Elapsed refill�leri tek seferde uygula
                while (now >= nextRefillUtc)
                {
                    current++;
                    changed = true;
                    if (current >= max) break;
                    nextRefillUtc = nextRefillUtc.AddMinutes(refillMinutes);
                }

                if (current >= max)
                {
                    // Full oldu, sayac� durdur
                    nextRefillUtc = now;
                }
            }
            else
            {
                // Zaten full; sayac� beklet
                nextRefillUtc = now;
            }

            if (changed || forceUI)
            {
                (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeUI(current, max);
                SaveAll();
            }
        }

        public bool CanConsume(int n = 1) => current >= n;

        /// <summary> n adet can harcar; gerekirse refill sayac�n� ba�lat�r; UI�y� g�nceller. </summary>
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
            return true;
        }

        public void AddLife(int n = 1)
        {
            current = Mathf.Min(max, current + n);
            (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeUI(current, max);

            if (current >= max)
                nextRefillUtc = DateTime.UtcNow; // tamamsa sayac� beklet

            SaveAll();
        }

        public void SetMax(int newMax, bool refill = true)
        {
            max = Mathf.Max(1, newMax);
            if (refill) current = max;
            (ui ?? GameManager.Instance?.UIManager)?.UpdateLifeUI(current, max);
            SaveAll();
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus) RefillByElapsedTime(forceUI: true);
        }

        void OnApplicationPause(bool pause)
        {
            if (!pause) RefillByElapsedTime(forceUI: true);
        }
    }
}
