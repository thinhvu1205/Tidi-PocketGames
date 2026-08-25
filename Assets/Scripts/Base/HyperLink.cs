using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Hyperlink : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI _ThisTMPUGUI;
    private Action _OnClickCb;
    private Camera _MainC;

    public void Init(Camera _mainC, Action _onClickCb)
    {   // if using Camera Overlay then use null here, otherwise use Camera.main
        _MainC = _mainC;
        _OnClickCb = _onClickCb;
    }
    public void OnPointerClick(PointerEventData _dataPED)
    {
        if (TMP_TextUtilities.FindIntersectingLink(_ThisTMPUGUI, _dataPED.position, _MainC) != -1)
            _OnClickCb?.Invoke();
    }

    private void Awake() => _ThisTMPUGUI = GetComponent<TextMeshProUGUI>();
}