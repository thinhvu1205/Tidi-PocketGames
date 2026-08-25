using System;
using UnityEngine;

public class GamesTab : MonoBehaviour
{
    [SerializeField] private int _Id;
    private ItemOnOff _DataIOO;
    private Action _OnClickCb;

    #region Button
    public void DoClick()
    {
        _OnClickCb?.Invoke();
        _DataIOO.TurnOn();
        UIManager.DoClickBase(transform);
    }
    #endregion

    public void SetOnClickCb(Action _onClickCb) => _OnClickCb = _onClickCb;
    public int GetId() => _Id;
    public void UnSelect() => _DataIOO.TurnOff();

    private void Awake() => _DataIOO = GetComponent<ItemOnOff>();
}
