
using UnityEngine;
using System;

namespace LostPlanet.Input
{
    public class InputManager : MonoBehaviour
    {
        public static event Action<Vector2Int> OnSwipe;
        public static event Action OnSuperPressed;
        [SerializeField] private float minSwipeDistance = 40f;
        private Vector2 _start;

        void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0)) _start = UnityEngine.Input.mousePosition;
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                var end = (Vector2)UnityEngine.Input.mousePosition;
                var delta = end - _start;
                if (delta.magnitude >= minSwipeDistance) OnSwipe?.Invoke(ToCardinal(delta));
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) OnSuperPressed?.Invoke();
            if (UnityEngine.Input.GetKeyDown(KeyCode.W) || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)) OnSwipe?.Invoke(new Vector2Int(0, 1));
            if (UnityEngine.Input.GetKeyDown(KeyCode.S) || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow)) OnSwipe?.Invoke(new Vector2Int(0, -1));
            if (UnityEngine.Input.GetKeyDown(KeyCode.A) || UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow)) OnSwipe?.Invoke(new Vector2Int(-1, 0));
            if (UnityEngine.Input.GetKeyDown(KeyCode.D) || UnityEngine.Input.GetKeyDown(KeyCode.RightArrow)) OnSwipe?.Invoke(new Vector2Int(1, 0));
        }

        Vector2Int ToCardinal(Vector2 v)
        {
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) return new Vector2Int(v.x > 0 ? 1 : -1, 0);
            else return new Vector2Int(0, v.y > 0 ? 1 : -1);
        }
    }
}
