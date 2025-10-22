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
            // 1) Dünya kesin dursun: state, zaman ve fizik
            _gm.SetState(LostPlanet.Core.GameState.LevelComplete);
            Time.timeScale = 0f;
            Physics2D.simulationMode = SimulationMode2D.Script; // 2D fizik stop

            // DOTween kullanıyorsan (UpdateType=Normal) hepsini durdur (opsiyonel):
            // #define DOTWEEN_EXISTS
            // #if DOTWEEN_EXISTS
            // DG.Tweening.DOTween.PauseAll();
            // #endif

            // 2) Kısa fade (varsa)
            if (_fader != null)
                yield return _fader.FadeTo(0.6f, 0.35f);
            else
                yield return null;

            // 3) LevelComplete ekranı
            _gm.OnLevelComplete();
            // Not: Donuk kalması isteniyor → burada hiçbir şeyi "açmıyoruz".
        }


        void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsReady()) return;
            var pc = other.GetComponent<LostPlanet.Gameplay.PlayerController>();
            if (pc != null) TryComplete();
        }
    }
}
