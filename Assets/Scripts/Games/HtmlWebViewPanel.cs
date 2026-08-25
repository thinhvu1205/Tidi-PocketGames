using Gpm.WebView;
using UnityEngine;

public class HtmlWebViewPanel : MonoBehaviour
{
    private RectTransform _ParentRT; // the parent GameObject to fill
    private Canvas _DataC;             // the canvas that container lives under

    public void OpenHtml(string _html)
    {
        RectInt r = _GetScreenPixelRect(_ParentRT, _DataC);
        GpmWebViewRequest.Configuration config = new()
        {
            style = GpmWebViewStyle.POPUP,          // REQUIRED so position/size take effect
            orientation = GpmOrientation.UNSPECIFIED,
            backgroundColor = "#FFFFFF",
            isNavigationBarVisible = false,          // no chrome, so it fills the rect
            isCloseButtonVisible = false,
            isBackButtonVisible = false,
            isForwardButtonVisible = false,
            position = new GpmWebViewRequest.Position { hasValue = true, x = r.x, y = r.y },
            size = new GpmWebViewRequest.Size { hasValue = true, width = r.width, height = r.height },
        };
        GpmWebView.ShowHtmlString(_html, config, _OnWebViewCallback, null);
    }
    // Converts a RectTransform into native screen pixels with a TOP-LEFT origin.
    private static RectInt _GetScreenPixelRect(RectTransform _rt, Canvas _canvas)
    {
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : _canvas.worldCamera;
        Vector3[] corners = new Vector3[4];
        _rt.GetWorldCorners(corners);   // 0=BL, 1=TL, 2=TR, 3=BR (world space)
        Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, corners[0]); // bottom-left
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(cam, corners[2]); // top-right
        int x = Mathf.RoundToInt(bl.x);
        int width = Mathf.RoundToInt(tr.x - bl.x);
        int height = Mathf.RoundToInt(tr.y - bl.y);
        int y = Mathf.RoundToInt(Screen.height - tr.y); // flip Y: Unity=bottom-left, native=top-left
        return new RectInt(x, y, width, height);
    }
    // Call if the container moves/resizes (rotation, layout change) while the webview is open
    public void SyncToContainer()
    {
        if (!GpmWebView.IsActive()) return;
        RectInt r = _GetScreenPixelRect(_ParentRT, _DataC);
        GpmWebView.SetPosition(r.x, r.y);
        GpmWebView.SetSize(r.width, r.height);
    }
    public void Close() => GpmWebView.Close();
    private void _OnWebViewCallback(GpmWebViewCallback.CallbackType _type, string _data, GpmWebViewError _error)
    {
        if (_type == GpmWebViewCallback.CallbackType.Open && _error != null)
            Debug.LogError(") =3 WebView open failed: " + _error);
    }

    private void Awake()
    {
        _ParentRT = GetComponent<RectTransform>();
        _DataC = GetComponentInParent<Canvas>();
    }
}