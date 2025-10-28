using UnityEngine;
#if DOTWEEN_EXISTS
using DG.Tweening;
#endif
using UInput = UnityEngine.Input;

namespace LostPlanet.Gameplay
{
    public class BobAndSpin : MonoBehaviour
    {
        [Header("Bob (Yoyo)")]
        public bool bob = true;
        public float bobAmplitude = 0.2f;
        public float bobDuration = 1.6f;

        [Header("Spin")]
        public bool spin = true;
        public float spinSpeed = 60f; // deg/s

        [Header("Pulse (scale)")]
        public bool pulse = false;
        public float pulseScale = 1.08f;
        public float pulseDuration = 0.6f;

        Vector3 _startLocalPos;
#if DOTWEEN_EXISTS
        Tween _bobT, _pulseT;
#endif

        void OnEnable()
        {
            _startLocalPos = transform.localPosition;
#if DOTWEEN_EXISTS
            if (bob)
                _bobT = transform.DOLocalMoveY(_startLocalPos.y + bobAmplitude, bobDuration)
                    .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            if (pulse)
                _pulseT = transform.DOScale(Vector3.one * pulseScale, pulseDuration)
                    .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
#endif
        }

        void Update()
        {
            if (spin)
                transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }

        void OnDisable()
        {
#if DOTWEEN_EXISTS
            _bobT?.Kill();
            _pulseT?.Kill();
            transform.DOKill();
#endif
            transform.localPosition = _startLocalPos;
            transform.localScale = Vector3.one;
        }
    }
}
