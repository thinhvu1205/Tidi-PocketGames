using System;
using UnityEngine;

[RequireComponent(typeof(ShowHideEffect))]
public class BasePopup : GameListener
{
    protected Transform _ContentTf;
    protected Action _OnCloseCb;
    protected ShowHideEffect _EffectSHE;

    public virtual void DoClickClose(bool _isDestroy = true)
    {
        _EffectSHE.RunCloseEffect(() =>
            {
                if (_isDestroy) Destroy(gameObject);
                else gameObject.SetActive(false);
            });
        UIManager.DoClickBase();
    }
    public void SetOnCloseCb(Action _onCloseCb) => _OnCloseCb = _onCloseCb;

    protected virtual void OnDisable() => _OnCloseCb?.Invoke();
    protected override void Awake()
    {
        base.Awake();
        _ContentTf = transform.GetChild(0);
        _EffectSHE = GetComponent<ShowHideEffect>();
    }
}
