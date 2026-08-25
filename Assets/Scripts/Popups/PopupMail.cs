using System.Collections.Generic;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupMail : BasePopup
{
    [SerializeField] private ShowHideEffect m_PanelMailDetailSHE;
    [SerializeField] private ItemOnOff m_ButtonSelectIOO;
    [SerializeField] private Button m_ClaimRewardsBtn;
    [SerializeField] private PoolGroup m_MailsPG;
    [SerializeField] private TextMeshProUGUI m_MailDetailTitleTMPUGUI, m_MailDetailContentTMPUGUI;
    private List<PoolInfo> _MailPIs = new();
    private string _ReadMailId;
    private bool _IsWaitingMailsData;

    #region Button
    public void DoClickSelectAll()
    {
        bool isSelected = m_ButtonSelectIOO.IsTurnOn();
        if (isSelected) m_ButtonSelectIOO.TurnOff();
        else m_ButtonSelectIOO.TurnOn();
        foreach (PoolInfo aPI in _MailPIs) ((MailInfo)aPI.Data).IsSelected = !isSelected;
        m_MailsPG.RefreshUI();
        UIManager.DoClickBase();
    }
    public void DoClickDelete()
    {
        UIManager.DoClickBase();
        List<string> deletedIds = new();
        foreach (PoolInfo aPI in _MailPIs)
        {
            MailInfo aMI = (MailInfo)aPI.Data;
            if (!aMI.IsClaimed && aMI.Amount > 0)
            {
                UIManager.Announce("Can not delete, there are unclaimed mails");
                return;
            }
            if (aMI.IsSelected) deletedIds.Add(aMI.Id);
        }
        if (deletedIds.Count <= 0) UIManager.Announce("Please select at least 1 mail to delete!");
        else UIManager.Announce("You really want to delete?", "Yes", "Cancel", () => { DataSender.DeleteMail(deletedIds); });
    }
    public void DoClickClosePanelMailDetail()
    {
        _ReadMailId = "";
        m_ClaimRewardsBtn.interactable = true;
        m_PanelMailDetailSHE.RunCloseEffect(() => m_PanelMailDetailSHE.gameObject.SetActive(false));
        m_MailsPG.RefreshUI();
        UIManager.DoClickBase();
    }
    public void DoClickClaimMail()
    {
        DataSender.ClaimMailRewards(_ReadMailId);
        m_ClaimRewardsBtn.interactable = false;
        UIManager.DoClickBase();
    }
    #endregion

    public override void HandleData(string _apiName, string _data)
    {
        base.HandleData(_apiName, _data);
        switch (_apiName)
        {
            case DataSender.LIST_MAIL:
                {
                    if (!_IsWaitingMailsData) return;
                    _IsWaitingMailsData = false;
                    _MailPIs.Clear();
                    JSONArray dataJA = JSON.Parse(_data)["mails"].AsArray;
                    foreach (JSONNode aJN in dataJA)
                    {
                        MailInfo aMI = new()
                        {
                            Id = aJN["id"].Value,
                            Title = aJN["title"].Value,
                            Content = aJN["content"].Value,
                            Amount = aJN["amount"].AsLong,
                            IsClaimed = aJN["isClaimed"].AsBool,
                            IsRead = aJN["isRead"].AsBool,
                            DateTime = aJN["createdAt"].Value,
                            IsSelected = false,
                        };
                        _MailPIs.Add(new() { Data = aMI });
                    }
                    m_ButtonSelectIOO.TurnOff();
                    m_MailsPG.SetControlInfo(_MailPIs);
                    break;
                }
            case DataSender.CLAIM_MAIL:
                {
                    UIManager.Announce("Congratulations! You have received <color=yellow>" + JSON.Parse(_data)["amount"].AsLong + "</color>");
                    DataSender.GetProfile();
                    _UpdateMailsData();
                    break;
                }
            case DataSender.DELETE_MAIL:
                {
                    UIManager.Announce("Delete mails successfully");
                    DataSender.GetProfile();
                    _UpdateMailsData();
                    break;
                }
        }
    }
    private void _UpdateMailsData()
    {
        _IsWaitingMailsData = true;
        DataSender.GetListMail();
    }

    private void OnEnable()
    {
        _UpdateMailsData();
    }
    protected override void Awake()
    {
        base.Awake();
        m_MailsPG.SetControlCbs((aRT, aPI) => aRT.GetComponent<ItemMail>().SetData((MailInfo)aPI.Data, onClickOpenMail, onClickSelection));

        void onClickOpenMail(MailInfo _aMI)
        {
            _ReadMailId = _aMI.Id;
            m_PanelMailDetailSHE.gameObject.SetActive(true);
            m_MailDetailTitleTMPUGUI.SetText((_aMI.Amount > 0 ? "<voffset=0.3em><sprite index=0></voffset>" : "") + _aMI.Title);
            m_MailDetailContentTMPUGUI.SetText(_aMI.Content);
            if (!_aMI.IsRead) DataSender.SetMailAsRead(_aMI.Id);
            m_ClaimRewardsBtn.gameObject.SetActive(!_aMI.IsClaimed);
        }
        void onClickSelection()
        {
            bool isSelectingAll = true;
            foreach (PoolInfo aPI in _MailPIs)
            {
                if (((MailInfo)aPI.Data).IsSelected) continue;
                isSelectingAll = false;
                break;
            }
            if (isSelectingAll) m_ButtonSelectIOO.TurnOn();
            else m_ButtonSelectIOO.TurnOff();
        }
    }
}
