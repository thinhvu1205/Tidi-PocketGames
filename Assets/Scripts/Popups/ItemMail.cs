using System;
using TMPro;
using UnityEngine;

public class ItemMail : MonoBehaviour
{
    [SerializeField] private GameObject m_BgUnread, m_Received;
    [SerializeField] private TextMeshProUGUI m_TitleTMPUGUI, m_ContentTMPUGUI, m_DateTimeTMPUGUI;
    private MailInfo _DataMI;
    private ItemOnOff _DataIOO;
    private Action<MailInfo> _OnCliCkOpenCb;
    private Action _OnSelectionCb;

    #region Button
    public void DoClickOpenMail()
    {
        _OnCliCkOpenCb?.Invoke(_DataMI);
        UIManager.DoClickBase();
    }
    public void DoClickSelectionBox()
    {
        _DataMI.IsSelected = !_DataMI.IsSelected;
        if (_DataMI.IsSelected) _DataIOO.TurnOn();
        else _DataIOO.TurnOff();
        _OnSelectionCb?.Invoke();
        UIManager.DoClickBase();
    }
    #endregion

    public void SetData(MailInfo _dataMI, Action<MailInfo> _onClickOpenCb, Action _onSelectionCb)
    {
        _DataMI = _dataMI;
        _OnCliCkOpenCb ??= _onClickOpenCb;
        _OnSelectionCb ??= _onSelectionCb;
        string splitContent = _dataMI.Content.Split('\n')[0];
        m_TitleTMPUGUI.SetText(_dataMI.Title[..Mathf.Min(_dataMI.Title.Length, 40)] + "...");
        m_ContentTMPUGUI.SetText(splitContent[..Mathf.Min(splitContent.Length, 40)] + "...");
        m_DateTimeTMPUGUI.SetText(Database.FormatDateTime(_dataMI.DateTime));
        m_BgUnread.SetActive(!_dataMI.IsRead);
        m_Received.SetActive(_dataMI.IsClaimed);
        if (_dataMI.IsSelected) _DataIOO.TurnOn();
        else _DataIOO.TurnOff();
    }

    private void Awake() => _DataIOO = GetComponent<ItemOnOff>();
}
public class MailInfo
{
    public string Id, Title, Content, DateTime;
    public long Amount;
    public bool IsClaimed, IsRead, IsSelected;
}
