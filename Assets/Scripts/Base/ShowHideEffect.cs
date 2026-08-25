using System;
using DG.Tweening;
using UnityEngine;

public class ShowHideEffect : MonoBehaviour
{
    private enum TYPE { NONE, SCALE_UP, DROP_FROM_ABOVE }
    [SerializeField] private TYPE m_ShowHideEffect = TYPE.NONE;
    private const float _SHOW_HIDE_DURATION = .2f;
    private Transform _ContentTf;
    private Vector3 _StartPosV3;

    public void RunCloseEffect(Action _OnCloseCb)
    {
        switch (m_ShowHideEffect)
        {
            case TYPE.DROP_FROM_ABOVE:
                {
                    _ContentTf.DOLocalMoveY(Screen.height, _SHOW_HIDE_DURATION).SetEase(Ease.InBack).OnComplete(() => _OnCloseCb?.Invoke());
                    break;
                }
            case TYPE.SCALE_UP:
                {
                    _ContentTf.DOScale(0, .2f).SetEase(Ease.InBack).OnComplete(() => _OnCloseCb?.Invoke());
                    break;
                }
            case TYPE.NONE:
            default: { _OnCloseCb?.Invoke(); break; }
        }
    }

    private void OnEnable()
    {
        switch (m_ShowHideEffect)
        {
            case TYPE.DROP_FROM_ABOVE:
                {
                    _ContentTf.localPosition = _StartPosV3;
                    _ContentTf.DOLocalMoveY(_ContentTf.localPosition.y - Screen.height, _SHOW_HIDE_DURATION).SetEase(Ease.OutQuad);
                    break;
                }
            case TYPE.SCALE_UP:
                {
                    _ContentTf.localScale = Vector3.zero;
                    _ContentTf.DOScale(1f, .2f).SetEase(Ease.OutBack);
                    break;
                }
            case TYPE.NONE:
            default: { break; }
        }
    }
    private void Awake()
    {
        _ContentTf = transform.GetChild(0);
        _StartPosV3 = m_ShowHideEffect switch
        {
            TYPE.DROP_FROM_ABOVE => new Vector3(0, _ContentTf.localPosition.y + Screen.height, 0),
            TYPE.NONE or TYPE.SCALE_UP or _ => Vector3.zero
        };
    }
}
