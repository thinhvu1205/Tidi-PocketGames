using System.Collections.Generic;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupWithdraw : BasePopup
{
    [SerializeField] private List<ItemOnOff> m_PanelIOOs, m_PaymentChannelIOOs;
    [SerializeField] private List<TextMeshProUGUI> m_ChoiceTMPUGUIs;
    [SerializeField] private Transform m_PanelRewardTf, m_PanelHistoryTf, m_TextNoWithdrawTf;
    [SerializeField] private ShowHideEffect m_WithdrawDetailsSHE;
    [SerializeField] private TextMeshProUGUI m_SubTitleTMPUGUI, m_EffectiveBetTMPUGUI, m_EffectiveWageringTMPUGUI, m_DetailInputAccountTMPUGUI, m_DetailReinputAccountTMPUGUI;
    [SerializeField] private TMP_InputField m_DetailInputAccountTMPIF, m_DetailReinputAccountTMPIF;
    [SerializeField] private ScrollRect m_HistorySR;
    private List<PoolInfo> _HistoryPIs = new();
    private PoolGroup _HistoryPG;
    private Button _DefaultPaymentBtn;
    private long _CurrentChosenAmount;
    private string _CurrentChosenChannel;

    #region Button
    public void DoClickTabReward()
    {
        m_PanelRewardTf.gameObject.SetActive(true);
        m_PanelHistoryTf.gameObject.SetActive(false);
        UIManager.UnselectOnOffItems(m_PanelIOOs);
        UIManager.SelectAnOnOffItem(m_PanelIOOs, m_PanelIOOs[0]);
        _DefaultPaymentBtn.onClick?.Invoke();
        UIManager.DoClickBase();
    }
    public void DoClickTabHistory()
    {
        DataSender.GetWithdrawHistory();
        m_PanelRewardTf.gameObject.SetActive(false);
        m_PanelHistoryTf.gameObject.SetActive(true);
        UIManager.UnselectOnOffItems(m_PanelIOOs);
        UIManager.SelectAnOnOffItem(m_PanelIOOs, m_PanelIOOs[1]);
        _DefaultPaymentBtn.onClick?.Invoke();
        UIManager.DoClickBase();
    }
    public void DoClickRuleWithdraw()
    {
        UIManager.INSTANCE.OpenPopupRuleCashFlow();
        UIManager.DoClickBase();
    }
    public void DoClickConfirm()
    {
        UIManager.DoClickBase();
        string account = m_DetailInputAccountTMPIF.text, confirmedAccount = m_DetailReinputAccountTMPIF.text;
        if (string.IsNullOrEmpty(account))
        {
            UIManager.Announce("Account number must not be empty");
            return;
        }
        if (string.IsNullOrEmpty(confirmedAccount) || !account.Equals(confirmedAccount))
        {
            UIManager.Announce("Confirmed account number must be the same as account number");
            return;
        }
        DataSender.Withdraw(_CurrentChosenAmount, account, _CurrentChosenChannel);
    }
    public void DoClickClosePanelWithdrawDetails()
    {
        m_WithdrawDetailsSHE.RunCloseEffect(() => m_WithdrawDetailsSHE.gameObject.SetActive(false));
        UIManager.DoClickBase();
    }
    #endregion

    public override void HandleData(string _apiName, string _data)
    {
        base.HandleData(_apiName, _data);
        switch (_apiName)
        {
            case DataSender.WITHDRAW_HISTORY:
                {
                    JSONNode dataJN = JSON.Parse(_data);
                    JSONArray dataJA = dataJN["withdrawals"].AsArray;
                    Database.DB.WithdrawHWIs.Clear();
                    if (dataJA.Count <= 0) return;
                    foreach (JSONNode aJN in dataJA)
                    {
                        Database.DB.WithdrawHWIs.Add(new()
                        {
                            Asset = aJN["amount"].AsLong,
                            Phone = aJN["receiver"].Value,
                            Time = aJN["createdAt"].Value,
                            PaymentChannel = aJN["channel"].Value,
                            Status = (Database.WITHDRAW_STATUS)aJN["status"].AsInt
                        });
                    }
                    break;
                }
        }
    }
    private void OnEnable()
    {
        DoClickTabReward();
        foreach (ItemOnOff aIOO in m_PaymentChannelIOOs) aIOO.gameObject.SetActive(false);
        bool isFirstPaymentChannelSelected = false;
        foreach (PaymentChannel aPC in Database.DB.WithdrawPCs)
        {
            ItemOnOff aIOO = null;
            if (aPC.Name.Contains("gcash")) aIOO = m_PaymentChannelIOOs[0];
            else if (aPC.Name.Contains("maya")) aIOO = m_PaymentChannelIOOs[1];
            if (aIOO == null) continue;
            aIOO.TurnOff();
            aIOO.gameObject.SetActive(true);
            Button aBtn = aIOO.GetComponent<Button>();
            aBtn.onClick.RemoveAllListeners();
            aBtn.onClick.AddListener(() =>
            {
                UIManager.UnselectOnOffItems(m_PaymentChannelIOOs);
                UIManager.SelectAnOnOffItem(m_PaymentChannelIOOs, aIOO);
                if (m_PanelIOOs[0].IsTurnOn())
                {
                    foreach (TextMeshProUGUI aTMPUGUI in m_ChoiceTMPUGUIs) aTMPUGUI.transform.parent.gameObject.SetActive(false);
                    int countChoices = Mathf.Min(m_ChoiceTMPUGUIs.Count, aPC.Amounts.Count);
                    for (int i = 0; i < countChoices; i++)
                    {
                        TextMeshProUGUI aTMPUGUI = m_ChoiceTMPUGUIs[i];
                        long amount = aPC.Amounts[i];
                        aTMPUGUI.SetText(amount + "");
                        _CurrentChosenChannel = aPC.Name;
                        Button choiceBtn = aTMPUGUI.transform.parent.GetComponent<Button>();
                        choiceBtn.gameObject.SetActive(true);
                        choiceBtn.onClick.RemoveAllListeners();
                        choiceBtn.onClick.AddListener(() =>
                        {
                            _CurrentChosenAmount = amount;
                            m_WithdrawDetailsSHE.gameObject.SetActive(true);
                            m_DetailInputAccountTMPIF.text = "";
                            m_DetailReinputAccountTMPIF.text = "";
                            m_SubTitleTMPUGUI.SetText("WITHDRAW " + _CurrentChosenAmount + " PESOS");
                            m_DetailInputAccountTMPUGUI.SetText("Enter your " + _CurrentChosenChannel + " number");
                            m_DetailReinputAccountTMPUGUI.SetText("Confirm your " + _CurrentChosenChannel + " number");
                            UIManager.DoClickBase(choiceBtn.transform);
                        });
                    }
                    aIOO.TurnOn();
                }
                else
                {
                    _HistoryPIs.Clear();
                    foreach (HistoryWithdrawInfo aHWI in Database.DB.WithdrawHWIs)
                        if (aPC.Name.Contains(aHWI.PaymentChannel))
                            _HistoryPIs.Add(new() { Data = aHWI });
                    bool isHaveData = _HistoryPIs.Count > 0;
                    m_TextNoWithdrawTf.gameObject.SetActive(!isHaveData);
                    m_HistorySR.transform.parent.gameObject.SetActive(isHaveData);
                    if (isHaveData) _HistoryPG.SetControlInfo(_HistoryPIs);
                }
                UIManager.DoClickBase();
            });
            if (!isFirstPaymentChannelSelected)
            {
                isFirstPaymentChannelSelected = true;
                aBtn.onClick.Invoke();
            }
        }
        m_EffectiveBetTMPUGUI.SetText(Database.DB.TotalBet + "");
        m_EffectiveWageringTMPUGUI.SetText(Database.DB.RequiredBet + "");
        DoClickTabReward();
    }
    protected override void Awake()
    {
        base.Awake();
        _HistoryPG = m_HistorySR.content.GetComponent<PoolGroup>();
        _DefaultPaymentBtn = m_PaymentChannelIOOs[0].GetComponent<Button>();
        _HistoryPG.SetControlCbs((aRT, aPI) => { aRT.GetComponent<ItemHistoryWithdraw>().SetData((HistoryWithdrawInfo)aPI.Data); });
    }
}