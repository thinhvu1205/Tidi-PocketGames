using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ShowHideEffect : MonoBehaviour
{
    private enum TYPE { NONE, SCALE_UP, DROP_FROM_ABOVE }
    [SerializeField] private TYPE m_ShowHideEffect = TYPE.NONE;
    [SerializeField] private Transform m_ContentTf;
    [SerializeField] private CanvasGroup m_ContentCG;
    private const float _SHOW_HIDE_DURATION = .2f, _ALPHA_CHANGING_DURATION = .85f * _SHOW_HIDE_DURATION;
    private Vector3 _StartPosV3;
    private float _ElapsedAlphaDuration, _InterpolateAlphaDuration;

    public void RunCloseEffect(Action _OnCloseCb)
    {
        _ElapsedAlphaDuration = _InterpolateAlphaDuration = 0;
        switch (m_ShowHideEffect)
        {
            case TYPE.DROP_FROM_ABOVE:
                {
                    m_ContentTf.DOLocalMoveY(Screen.height, _SHOW_HIDE_DURATION).SetEase(Ease.InBack)
                        .OnUpdate(_ChangingAlphaOnClose)
                        .OnComplete(() =>
                        {
                            _OnCloseCb?.Invoke();
                            m_ContentCG.alpha = 0;
                        });

                    break;
                }
            case TYPE.SCALE_UP:
                {
                    m_ContentTf.DOScale(0, _SHOW_HIDE_DURATION).SetEase(Ease.InBack)
                        .OnUpdate(_ChangingAlphaOnClose)
                        .OnComplete(() =>
                        {
                            _OnCloseCb?.Invoke();
                            m_ContentCG.alpha = 0;
                        });
                    break;
                }
            case TYPE.NONE:
            default: { _OnCloseCb?.Invoke(); break; }
        }
    }
    private void _ChangingAlpha(float _alpha1, float _alpha2)
    {
        if (_InterpolateAlphaDuration >= 1) return;
        _ElapsedAlphaDuration += Time.deltaTime;
        _InterpolateAlphaDuration = _ElapsedAlphaDuration / _ALPHA_CHANGING_DURATION;
        m_ContentCG.alpha = Mathf.Lerp(_alpha1, _alpha2, _InterpolateAlphaDuration);
    }
    private void _ChangingAlphaOnOpen() => _ChangingAlpha(0, 1);
    private void _ChangingAlphaOnClose() => _ChangingAlpha(1, 0);

    private void OnEnable()
    {
        m_ContentTf.localPosition = _StartPosV3;
        _ElapsedAlphaDuration = _InterpolateAlphaDuration = 0;
        switch (m_ShowHideEffect)
        {
            case TYPE.DROP_FROM_ABOVE:
                {
                    m_ContentCG.alpha = 0;
                    m_ContentTf.DOLocalMoveY(m_ContentTf.localPosition.y - Screen.height, _SHOW_HIDE_DURATION).SetEase(Ease.OutQuad)
                        .OnUpdate(_ChangingAlphaOnOpen)
                        .OnComplete(() => m_ContentCG.alpha = 1);
                    break;
                }
            case TYPE.SCALE_UP:
                {
                    m_ContentTf.localScale = Vector3.zero;
                    m_ContentCG.alpha = 0;
                    m_ContentTf.DOScale(1f, _SHOW_HIDE_DURATION).SetEase(Ease.OutBack)
                        .OnUpdate(_ChangingAlphaOnOpen)
                        .OnComplete(() => m_ContentCG.alpha = 1);
                    break;
                }
            case TYPE.NONE:
            default: { break; }
        }
    }
    private void Awake()
    {
        _StartPosV3 = m_ShowHideEffect switch
        {
            TYPE.DROP_FROM_ABOVE => new Vector3(0, m_ContentTf.localPosition.y + Screen.height, 0),
            TYPE.NONE or TYPE.SCALE_UP or _ => Vector3.zero
        };
    }
}
