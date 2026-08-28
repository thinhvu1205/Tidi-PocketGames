using UnityEngine;

public class PopupRuleCashFlow : BasePopup
{
    [SerializeField] private GameObject m_RuleDeposit, m_RuleWithdraw;

    public void Show(bool _isDepositRule)
    {
        m_RuleDeposit.SetActive(_isDepositRule);
        m_RuleWithdraw.SetActive(!_isDepositRule);
    }
}
