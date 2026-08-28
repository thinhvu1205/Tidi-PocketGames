using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupDeposit : BasePopup
{
    [SerializeField] private List<ItemOnOff> m_PaymentChannelIOOs;
    [SerializeField] private List<TextMeshProUGUI> m_ChoiceTMPUGUIs;
    [SerializeField] private List<DepositAccountInfo> m_DepositAccountDAIs;
    [SerializeField] private Transform m_ButtonClaimDepositTf, m_ButtonSupportTf;
    [SerializeField] private TextMeshProUGUI m_SubTitleTMPUGUI, m_InstructionTMPUGUI;
    [SerializeField] private TMP_InputField m_TransactionIdTMPIF;
    [SerializeField] private ShowHideEffect m_DepositDetailsSHE;
    private long _CurrentChosenAmount;

    #region Button
    public void DoClickClaimDeposit()
    {
        DataSender.ClaimDeposit(m_TransactionIdTMPIF.text);
        UIManager.DoClickBase(m_ButtonClaimDepositTf);
    }
    public void DoClickClosePanelDepositDetails()
    {
        m_DepositDetailsSHE.RunCloseEffect(() => m_DepositDetailsSHE.gameObject.SetActive(false));
        UIManager.DoClickBase();
    }
    public void DoClickRuleDeposit()
    {
        UIManager.INSTANCE.OpenPopupRuleCashFlow(true);
        UIManager.DoClickBase();
    }
    public void DoClickSupport()
    {
        UIManager.INSTANCE.OpenPopupSupport();
        UIManager.DoClickBase(m_ButtonSupportTf);
    }
    #endregion

    public override void HandleData(string _apiName, string _data)
    {
        base.HandleData(_apiName, _data);
        switch (_apiName)
        {
            case DataSender.CLAIM_DEPOSIT:
                {
                    UIManager.Announce("Check mails box to claim your deposit", "To mails box", "", () => UIManager.INSTANCE.OpenPopupMail());
                    break;
                }
        }
    }

    private void OnEnable()
    {
        foreach (ItemOnOff aIOO in m_PaymentChannelIOOs) aIOO.gameObject.SetActive(false);
        bool isFirstPaymentChannelSelected = false;
        foreach (PaymentChannel aPC in Database.DB.DepositPCs)
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
                foreach (TextMeshProUGUI aTMPUGUI in m_ChoiceTMPUGUIs) aTMPUGUI.transform.parent.gameObject.SetActive(false);
                int countChoices = Mathf.Min(m_ChoiceTMPUGUIs.Count, aPC.Amounts.Count);
                for (int i = 0; i < countChoices; i++)
                {
                    TextMeshProUGUI aTMPUGUI = m_ChoiceTMPUGUIs[i];
                    long amount = aPC.Amounts[i];
                    aTMPUGUI.SetText(amount + "");
                    Button choiceBtn = aTMPUGUI.transform.parent.GetComponent<Button>();
                    choiceBtn.gameObject.SetActive(true);
                    choiceBtn.onClick.RemoveAllListeners();
                    choiceBtn.onClick.AddListener(() =>
                    {
                        _CurrentChosenAmount = amount;
                        m_DepositDetailsSHE.gameObject.SetActive(true);
                        m_SubTitleTMPUGUI.SetText("YOU ARE DEPOSITING <color=red>" + _CurrentChosenAmount + "</color> PESOS");
                        m_InstructionTMPUGUI.SetText("Please transfer money to our " + aPC.Name + " account");
                        int countDepositAccounts = Mathf.Min(aPC.DepositDAs.Count, m_DepositAccountDAIs.Count);
                        foreach (DepositAccountInfo aDAI in m_DepositAccountDAIs) aDAI.Account.SetActive(false);
                        for (int i = 0; i < countDepositAccounts; i++)
                        {
                            DepositAccount aDA = aPC.DepositDAs[i];
                            DepositAccountInfo aDAI = m_DepositAccountDAIs[i];
                            aDAI.Account.SetActive(true);
                            aDAI.NameTMPUGUI.SetText(aDA.AccountName);
                            aDAI.NumberTMPUGUI.SetText(aDA.AccountNumber);
                        }
                        UIManager.DoClickBase(choiceBtn.transform);
                    });
                }
                aIOO.TurnOn();
                UIManager.DoClickBase();
            });
            if (!isFirstPaymentChannelSelected)
            {
                isFirstPaymentChannelSelected = true;
                aBtn.onClick.Invoke();
            }
        }
    }
    protected override void Awake()
    {
        base.Awake();
        foreach (DepositAccountInfo aDAI in m_DepositAccountDAIs)
        {
            Button copyBtn = aDAI.CopyBtn;
            copyBtn.onClick.AddListener(() =>
            {
                GUIUtility.systemCopyBuffer = aDAI.NumberTMPUGUI.text;
                UIManager.DoClickBase(copyBtn.transform);
            });
        }
    }
}
[Serializable]
public struct DepositAccountInfo
{
    public GameObject Account;
    public TextMeshProUGUI NameTMPUGUI, NumberTMPUGUI;
    public Button CopyBtn;
}
