using UnityEngine;

namespace LostPlanet.Items
{
    public class PortalController : MonoBehaviour
    {
        [Header("State")]
        public bool isActive = false;                 // manuel açık/kapalı
        public bool autoActivateByCrystals = true;    // kristaller tamamlanınca otomatik aç

        [Header("Refs")]
        public Collider2D trigger;

        // cache
        LostPlanet.Managers.CrystalManager _cm;
        LostPlanet.Core.GameManager _gm;
        LostPlanet.UI.UIFader _fader;

        void Awake()
        {
            if (!trigger) trigger = GetComponent<Collider2D>();
            if (trigger) trigger.isTrigger = true;

            _gm = LostPlanet.Core.GameManager.Instance;
            if (_gm == null) _gm = FindObjectOfType<LostPlanet.Core.GameManager>();

            _cm = _gm != null ? _gm.CrystalManager : FindObjectOfType<LostPlanet.Managers.CrystalManager>();
            _fader = FindObjectOfType<LostPlanet.UI.UIFader>();
        }

        void Update()
        {
            // Kristaller yeterli ise otomatik aktive et
            if (!isActive && autoActivateByCrystals && _cm != null &&
                _cm.collectedTotal >= _cm.requiredTotal)
            {
                Activate();
            }
        }

        public void Activate() { isActive = true; }

        bool IsReady()
        {
            if (isActive) return true;
            if (_cm != null) return _cm.collectedTotal >= _cm.requiredTotal;
            return false;
        }

        // PlayerController burayı çağırıyor
        public void TryComplete()
        {
            if (!IsReady()) return;
            if (_gm == null) return;

            // Seviye tamamlanma sekansı: dünya don, kısa fade, LevelComplete göster
            _gm.StartCoroutine(CoCompleteSequence());
        }

        System.Collections.IEnumerator CoCompleteSequence()
        {
            // 1) Dünya donsun (Update/Coroutines WaitForSeconds durur)
            _gm.SetState(LostPlanet.Core.GameState.LevelComplete);
            Time.timeScale = 0f;

            // 2) Kısa fade ile sahneyi karart (varsa)
            if (_fader != null)
                yield return _fader.FadeTo(0.6f, 0.35f); // %60 karart, 0.35 sn
            else
                yield return null;

            // 3) LevelComplete ekranı
            _gm.OnLevelComplete();
            // Not: OnLevelComplete içinde tekrar State set ediliyor; sorun değil.
            // Donuk kalmasını istediğimiz için burada timeScale'i açmıyoruz.
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsReady()) return;
            var pc = other.GetComponent<LostPlanet.Gameplay.PlayerController>();
            if (pc != null) TryComplete();
        }
    }
}
