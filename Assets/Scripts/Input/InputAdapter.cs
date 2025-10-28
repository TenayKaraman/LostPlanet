// LostPlanet.Input ile UnityEngine.Input isim çakýþmasýný çözen köprü.
// Bu dosya sayesinde "using LostPlanet.Input;" olan yerlerdeki
// Input.GetKeyDown() gibi çaðrýlar UnityEngine.Input'a yönlenir.

using UnityEngine;

namespace LostPlanet.Input
{
    public static class Input  // <--- tür adý "Input"
    {
        // Klavye
        public static bool GetKeyDown(KeyCode key) => UnityEngine.Input.GetKeyDown(key);
        public static bool GetKey(KeyCode key) => UnityEngine.Input.GetKey(key);
        public static bool GetKeyUp(KeyCode key) => UnityEngine.Input.GetKeyUp(key);

        // Mouse
        public static bool GetMouseButtonDown(int button) => UnityEngine.Input.GetMouseButtonDown(button);
        public static bool GetMouseButtonUp(int button) => UnityEngine.Input.GetMouseButtonUp(button);
        public static Vector3 mousePosition => UnityEngine.Input.mousePosition;

        // Touch
        public static int touchCount => UnityEngine.Input.touchCount;
        public static Touch GetTouch(int index) => UnityEngine.Input.GetTouch(index);
    }
}
