using DG.Tweening;
using TMPro;
using UnityEngine;

public class PopupAccount : BasePopup
{
    [SerializeField] private Transform m_ChangePassTf, m_PayBackTf, m_VipTf, m_ReferralTf, m_NotiTf, m_DepositTf, m_WithdrawTf;
    [SerializeField] private TextMeshProUGUI m_UsernameTMPUGUI, m_UserIdTMPUGUI, m_AssetTMPUGUI;
    [SerializeField] private ItemOnOff m_UpgradeAccountIOO;

    #region Button
    public void DoClickChangePass()
    {
        UIManager.INSTANCE.OpenPopupChangePass();
        UIManager.DoClickBase(m_ChangePassTf);
    }
    public void DoClickUpgradeAccount()
    {
        UIManager.INSTANCE.OpenPopupUpgradeAccount();
        UIManager.DoClickBase(m_UpgradeAccountIOO.transform);
    }
    public void DoClickPayBack()
    {
        UIManager.INSTANCE.OpenPopupPayBack();
        UIManager.DoClickBase(m_PayBackTf);
    }
    public void DoClickVip()
    {
        UIManager.INSTANCE.OpenPopupVip();
        UIManager.DoClickBase(m_VipTf);
    }
    public void DoClickReferral()
    {
        UIManager.INSTANCE.OpenPopupReferral();
        UIManager.DoClickBase(m_ReferralTf);
    }
    public void DoClickNotification()
    {
        UIManager.INSTANCE.OpenPopupNotification();
        UIManager.DoClickBase(m_NotiTf);
    }
    public void DoClickDeposit()
    {
        UIManager.INSTANCE.OpenPopupDeposit();
        UIManager.DoClickBase(m_DepositTf);
    }
    public void DoClickWithdraw()
    {
        UIManager.INSTANCE.OpenPopupWithdraw();
        UIManager.DoClickBase(m_WithdrawTf);
    }
    public void DoClickLogOut()
    {
        PlayerPrefs.SetString(Database.USERNAME, "");
        PlayerPrefs.SetString(Database.PASSWORD, "");
        Database.DB.PlayToken = "";
        UIManager.DoClickBase();
        UIManager.INSTANCE.LoadScene(Database.MAIN_SCENE);
    }
    #endregion

    public override void HandleData(string _apiName, string _data)
    {
        base.HandleData(_apiName, _data);
        switch (_apiName)
        {
            case DataSender.PROFILE:
            case DataSender.REGISTER_QUICK_PLAY:
            case DataSender.LINK_GOOGLE_ACCOUNT:
                {
                    _UpdateDisplay();
                    break;
                }
        }
    }
    private void _UpdateDisplay()
    {
        m_UsernameTMPUGUI.SetText(Database.DB.Username);
        m_UserIdTMPUGUI.SetText("<color=#50D7FB>" + Database.DB.UserId + "</color>   <link><voffset=0.3em><sprite index=0></voffset></link>");
        m_AssetTMPUGUI.SetText(Database.DB.Asset + Database.DB.Currency);
        if (Database.DB.IsOfficial) m_UpgradeAccountIOO.TurnOff();
        else m_UpgradeAccountIOO.TurnOn();
    }

    private void Start()
    {
        m_VipTf.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
        m_NotiTf.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
        m_PayBackTf.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
        m_ReferralTf.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
        m_ChangePassTf.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
        m_UpgradeAccountIOO.transform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
    }
    private void OnEnable()
    {
        _UpdateDisplay();
    }
    protected override void Awake()
    {
        base.Awake();
        m_UserIdTMPUGUI.GetComponent<Hyperlink>().Init(null, () =>
            {
                GUIUtility.systemCopyBuffer = Database.DB.UserId;
                UIManager.DoClickBase(m_UserIdTMPUGUI.transform);
            });
    }
}
