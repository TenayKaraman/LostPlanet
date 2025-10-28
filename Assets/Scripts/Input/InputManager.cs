// Minimal & robust input bridge for LostPlanet
using UnityEngine;

namespace LostPlanet.Input
{
    [DefaultExecutionOrder(-100)]
    public class InputManager : MonoBehaviour
    {
        // Global events the gameplay listens to:
        public static System.Action<Vector2Int> OnSwipe;       // Up/Down/Left/Right
        public static System.Action OnSuperPressed;            // NOS / Super ability trigger

        [Header("General")]
        [Tooltip("Editor/PC testinde klavye okları/WASD ve Space/E ile tetiklemeyi açar.")]
        public bool enableKeyboardFallback = true;

        [Header("Swipe Settings")]
        [Tooltip("Swipe algılamak için minimum piksel mesafesi.")]
        public float swipeMinPixels = 40f;

        // Internal
        private Vector2 _pointerDownPos;
        private bool _pointerDown;

        private static InputManager _instance;

        private void Awake()
        {
            // Singleton-like guard (not strict singleton to allow domain reloads)
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // --- Touch / Mouse swipe detection ---
            HandlePointerSwipe();

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (enableKeyboardFallback)
            {
                // Keyboard arrows / WASD -> swipe
                if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W))
                    FireSwipe(Vector2Int.up);
                else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S))
                    FireSwipe(Vector2Int.down);
                else if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
                    FireSwipe(Vector2Int.left);
                else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D))
                    FireSwipe(Vector2Int.right);

                // Space / E -> Super
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.E))
                    FireSuper();
            }
#endif
        }

        private void HandlePointerSwipe()
        {
            // Touch first
            if (UnityEngine.Input.touchCount > 0)
            {
                var t = UnityEngine.Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    _pointerDown = true;
                    _pointerDownPos = t.position;
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    if (_pointerDown)
                    {
                        var delta = (Vector2)t.position - _pointerDownPos;
                        TryEmitSwipe(delta);
                    }
                    _pointerDown = false;
                }
                return; // if we had touch, skip mouse
            }

            // Mouse as fallback (click-drag-release)
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _pointerDown = true;
                _pointerDownPos = UnityEngine.Input.mousePosition;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                if (_pointerDown)
                {
                    var delta = (Vector2)UnityEngine.Input.mousePosition - _pointerDownPos;
                    TryEmitSwipe(delta);
                }
                _pointerDown = false;
            }
        }

        private void TryEmitSwipe(Vector2 delta)
        {
            if (delta.magnitude < swipeMinPixels) return;

            // Cardinalize
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                FireSwipe(delta.x > 0 ? Vector2Int.right : Vector2Int.left);
            else
                FireSwipe(delta.y > 0 ? Vector2Int.up : Vector2Int.down);
        }

        // -------- Public helpers for UI Buttons --------
        public void TriggerSwipeUp() => FireSwipe(Vector2Int.up);
        public void TriggerSwipeDown() => FireSwipe(Vector2Int.down);
        public void TriggerSwipeLeft() => FireSwipe(Vector2Int.left);
        public void TriggerSwipeRight() => FireSwipe(Vector2Int.right);
        public void TriggerSuper() => FireSuper();

        // -------- Emitters --------
        private static void FireSwipe(Vector2Int dir)
        {
            OnSwipe?.Invoke(dir);
        }

        private static void FireSuper()
        {
            OnSuperPressed?.Invoke();
        }
    }
}
