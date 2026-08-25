using UnityEngine;
using TMPro;
using System;

public class PopupAnnouncement : BasePopup
{
    [SerializeField] private Transform m_Button1Tf, m_Button2Tf, m_ButtonCloseTf;
    [SerializeField] private TextMeshProUGUI m_ContentTMPUGUI, m_Button1TMPUGUI, m_Button2TMPUGUI;
    private Action _OnClickButton1Cb, _OnClickButton2Cb;

    #region Button
    public void DoClickButton1()
    {
        _OnClickButton1Cb?.Invoke();
        DoClickClose(false);
    }
    public void DoClickButton2()
    {
        _OnClickButton2Cb?.Invoke();
        DoClickClose(false);
    }
    #endregion

    public void SetData(string _content, string _button1Label = "", string _button2Label = "",
        Action _onClickButton1Cb = null, Action _onClickButton2Cb = null, Action _onClickCloseCb = null)
    {
        _OnClickButton1Cb = _onClickButton1Cb;
        _OnClickButton2Cb = _onClickButton2Cb;
        SetOnCloseCb(_onClickCloseCb);
        m_Button1Tf.gameObject.SetActive(!_button1Label.Equals(""));
        m_Button2Tf.gameObject.SetActive(!_button2Label.Equals(""));
        m_ContentTMPUGUI.SetText(_content);
        m_Button1TMPUGUI.SetText(_button1Label);
        m_Button2TMPUGUI.SetText(_button2Label);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (UIManager.INSTANCE.PoolPAs.Contains(this)) UIManager.INSTANCE.PoolPAs.Remove(this);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (!UIManager.INSTANCE.PoolPAs.Contains(this)) UIManager.INSTANCE.PoolPAs.Add(this);
    }
}
