using UnityEngine;

namespace LostPlanet.Core
{
    public static class FreezeService
    {
        static bool _frozen;

        public static void FreezeWorld()
        {
            if (_frozen) return;
            _frozen = true;

            // DOTween kullanýyorsan bu sembolü tanýmlayýp pause edebilirsin.
            // #define DOTWEEN_EXISTS
#if DOTWEEN_EXISTS
            DG.Tweening.DOTween.PauseAll();
#endif
            Physics2D.simulationMode = SimulationMode2D.Script; // fizik durur
            Time.timeScale = 0f;                                 // zaman durur
        }

        public static void UnfreezeWorld()
        {
            if (!_frozen) return;
            _frozen = false;

#if DOTWEEN_EXISTS
            DG.Tweening.DOTween.PlayAll();
#endif
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
            Time.timeScale = 1f;
        }
    }
}
