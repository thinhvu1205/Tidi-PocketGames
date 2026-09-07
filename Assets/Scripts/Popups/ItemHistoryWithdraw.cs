using TMPro;
using UnityEngine;

public class ItemHistoryWithdraw : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TimeTMPUGUI, m_PesosTMPUGUI, m_PhoneTMPUGUI, m_StatusTMPUGUI;

    public void SetData(HistoryWithdrawInfo _aHWI)
    {
        m_TimeTMPUGUI.SetText(Database.FormatDateTime(_aHWI.Time));
        m_PesosTMPUGUI.SetText(Database.FormatNumber(_aHWI.Asset) + " P");
        m_PhoneTMPUGUI.SetText(_aHWI.Phone);
        m_StatusTMPUGUI.SetText(_aHWI.Status switch
        {
            Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_COMPLETED or Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_UNCONFIRMED => "<color=green>Completed</color>",
            Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_REFUNDED => "<color=yellow>Refunded</color>",
            Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_REJECTED => "<color=red>Rejected</color>",
            Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_UNSPECIFIED or Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_PENDING
                or Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_APPROVED or Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_PROCESSING
                or Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_FAILED or Database.WITHDRAW_STATUS.WITHDRAWAL_STATUS_CANCELLED or _ => "<color=#4FD7FB>Processing</color>",
        });
    }
}
public class HistoryWithdrawInfo
{
    public string Time, Phone, PaymentChannel;
    public long Asset;
    public Database.WITHDRAW_STATUS Status;
}
