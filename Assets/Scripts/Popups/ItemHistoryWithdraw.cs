using TMPro;
using UnityEngine;
using WITHDRAW_STATUS = Database.WITHDRAW_STATUS;
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
            WITHDRAW_STATUS.COMPLETED or WITHDRAW_STATUS.UNCONFIRMED => "<color=green>Completed</color>",
            WITHDRAW_STATUS.REFUNDED => "<color=yellow>Refunded</color>",
            WITHDRAW_STATUS.REJECTED => "<color=red>Rejected</color>",
            WITHDRAW_STATUS.UNSPECIFIED or WITHDRAW_STATUS.PENDING or WITHDRAW_STATUS.APPROVED or WITHDRAW_STATUS.PROCESSING
                or WITHDRAW_STATUS.FAILED or WITHDRAW_STATUS.CANCELLED or _ => "<color=#4FD7FB>Processing</color>",
        });
    }
}
public class HistoryWithdrawInfo
{
    public string Time, Phone, PaymentChannel;
    public double Asset;
    public WITHDRAW_STATUS Status;
}
