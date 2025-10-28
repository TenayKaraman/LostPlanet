using UnityEngine;
#if DOTWEEN_EXISTS
using DG.Tweening;
#endif

namespace LostPlanet.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("Path")]
        public Transform pointA;
        public Transform pointB;
        public float duration = 2f;
        public bool startAtA = true;
        public bool yoyo = true;

        [Header("Contact")]
        public string playerTag = "Player";
        public VoidEvent onPlayerHit;  // inspector’dan LifeManager/GM baðla

#if DOTWEEN_EXISTS
        Tween _moveT;
#endif
        Vector3 _backupA, _backupB;

        void OnEnable()
        {
            if (!pointA || !pointB)
            {
                // Otomatik 2 nokta üret (yerinde + x)
                _backupA = transform.position;
                _backupB = transform.position + Vector3.right * 2f;
            }

#if DOTWEEN_EXISTS
            Vector3 a = pointA ? pointA.position : _backupA;
            Vector3 b = pointB ? pointB.position : _backupB;

            transform.position = startAtA ? a : b;
            _moveT = transform.DOMove(startAtA ? b : a, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(yoyo ? -1 : 0, LoopType.Yoyo);
#endif
        }

        void OnDisable()
        {
#if DOTWEEN_EXISTS
            _moveT?.Kill();
            transform.DOKill();
#endif
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
                onPlayerHit?.Invoke();
        }
    }
}
