using UnityEngine;
using UnityEngine.UI;
using LostPlanet.GridSystem;

/// Main Camera'ya tak. TopBar / AbilityBar atanmadýysa isimden bulur.
/// GridManager runtime oluþsa bile kendini otomatik bulur.
[RequireComponent(typeof(Camera))]
public class CameraViewportFitter : MonoBehaviour
{
    [Header("Optional explicit refs (auto if null)")]
    public RectTransform topBar;        // (null ise "TopBar" isimli nesneyi arar)
    public RectTransform bottomBar;     // (null ise "AbilityBar" isimli nesneyi arar)
    public Canvas canvas;               // (null ise FindObjectOfType<Canvas>())
    public GridManager grid;            // (null ise FindObjectOfType<GridManager>())

    [Header("Options")]
    public bool keepGridHeight = true; // 16 satýr daima görünür kalsýn

    void LateUpdate()
    {
        // Lazily resolve refs
        if (!canvas) canvas = FindObjectOfType<Canvas>();
        if (!topBar) { var go = GameObject.Find("TopBar"); if (go) topBar = go.GetComponent<RectTransform>(); }
        if (!bottomBar) { var go = GameObject.Find("AbilityBar"); if (go) bottomBar = go.GetComponent<RectTransform>(); }
        if (!grid) grid = FindObjectOfType<GridManager>();

        var cam = GetComponent<Camera>();
        if (!cam || !canvas) return;

        float topPx = 0f, botPx = 0f;
        if (topBar) topPx = RectTransformUtility.PixelAdjustRect(topBar, canvas).height;
        if (bottomBar) botPx = RectTransformUtility.PixelAdjustRect(bottomBar, canvas).height;

        float y = botPx / Screen.height;
        float h = 1f - (topPx + botPx) / Screen.height; // üst ve alt bar alanýný kýrp
        cam.rect = new Rect(0f, y, 1f, h);

        // Grid yüksekliðini koru: barlar kýrpsa da tüm 16 satýrý sýðdýr
        if (keepGridHeight && grid)
        {
            float baseOrtho = grid.height * grid.cellSize * 0.5f; // 16*1/2 = 8
            if (h > 0.001f) cam.orthographicSize = baseOrtho / h;
            cam.transform.position = new Vector3(0f, 0f, cam.transform.position.z);
        }
    }
}
