using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LostPlanet.UI
{
    public class UIFader : MonoBehaviour
    {
        Canvas _ownCanvas;
        CanvasGroup _group;

        void Awake()
        {
            // Eðer sahnede bir Canvas yoksa oluþtur
            var canvas = FindObjectOfType<Canvas>();
            if (!canvas)
            {
                var go = new GameObject("Canvas");
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
            }

            // Kendi alt-canvas'ýmýz (üstte olsun)
            var faderCanvasGO = new GameObject("ScreenFader");
            faderCanvasGO.transform.SetParent(canvas.transform, false);
            _ownCanvas = faderCanvasGO.AddComponent<Canvas>();
            _ownCanvas.overrideSorting = true;
            _ownCanvas.sortingOrder = 999; // her þeyin üstünde

            var scaler = faderCanvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            faderCanvasGO.AddComponent<GraphicRaycaster>();

            // Tam ekran siyah Image + CanvasGroup
            var imgGO = new GameObject("Black");
            imgGO.transform.SetParent(faderCanvasGO.transform, false);
            var rt = imgGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var img = imgGO.AddComponent<Image>();
            img.color = Color.black; // siyah perde

            _group = imgGO.AddComponent<CanvasGroup>();
            _group.alpha = 0f; // baþlangýçta görünmez
            _group.blocksRaycasts = false; // inputu engelleme
        }

        public IEnumerator FadeTo(float target, float duration)
        {
            float start = _group.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
                _group.alpha = Mathf.Lerp(start, target, k);
                yield return null;
            }
            _group.alpha = target;
        }
    }
}
