using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupVip : BasePopup
{
    [SerializeField] private List<TextMeshProUGUI> m_LevelTMPUGUIs, m_DepositTMPUGUIs, m_BetTMPUGUIs;
    [SerializeField] private ShowHideEffect m_PanelRuleSHE;
    [SerializeField] private Button m_UpgradeVipBtn, m_ButtonRuleBtn;
    [SerializeField] private TextMeshProUGUI m_SubTitle1TMPUGUI, m_SubTitle2TMPUGUI;

    #region Button
    public void DoCLickOpenPanelRule()
    {
        m_PanelRuleSHE.gameObject.SetActive(true);
        UIManager.DoClickBase(m_ButtonRuleBtn.transform);
    }
    public void DoClickUpgradeVip()
    {
        UIManager.INSTANCE.OpenPopupDeposit();
        UIManager.DoClickBase(m_UpgradeVipBtn.transform);
    }
    public void DoClickClosePanelRule()
    {
        m_PanelRuleSHE.RunCloseEffect(() => m_PanelRuleSHE.gameObject.SetActive(false));
        UIManager.DoClickBase();
    }
    #endregion

    private void OnEnable()
    {
        m_SubTitle1TMPUGUI.SetText("Kabuuang halaga ng deposito: <color=#63CDEF>" + Database.DB.TotalDeposit + "</color>");
        m_SubTitle1TMPUGUI.SetText("Kabuuang halaga ng taya: <color=#63CDEF>" + Database.DB.TotalBet + "</color>");
        int countVip = Mathf.Min(Database.DB.VipVIs.Count, m_LevelTMPUGUIs.Count);
        for (int i = 0; i < countVip; i++)
        {
            VipInfo aVI = Database.DB.VipVIs[i];
            m_LevelTMPUGUIs[i].SetText(aVI.Level == 1 ? "0-1" : aVI.Level.ToString());
            m_DepositTMPUGUIs[i].SetText(Database.FormatAndShortenNumber(aVI.TotalDeposit, 0, 1000000, false));
            m_BetTMPUGUIs[i].SetText(Database.FormatAndShortenNumber(aVI.TotalBet, 0, 1000000, false));
        }
    }
}
