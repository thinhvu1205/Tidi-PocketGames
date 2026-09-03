using TMPro;
using UnityEngine;

public class PopupUpgradeAccount : BasePopup
{
    [SerializeField]
    private GameObject m_ReEnterPassword, m_ButtonConfirm, m_ButtonGGLinking, m_ButtonSupport247;
    [SerializeField] private TMP_InputField m_UsernameTMPIF, m_PasswordTMPIF, m_ReEnterPasswordTMPIF;

    #region Button
    public void DoClickConfirm()
    {
        string username = m_UsernameTMPIF.text, password = m_PasswordTMPIF.text, reEnterPassword = m_ReEnterPasswordTMPIF.text;
        if (username.Equals("") || password.Equals("")) UIManager.Announce("Username and Password must not be empty!");
        else
        {
            if (!password.Equals(reEnterPassword)) UIManager.Announce("Password must be re-entered correctly!");
            else DataSender.RegisterQuickPlay(username, password);
        }
        UIManager.DoClickBase(m_ButtonConfirm.transform);
    }
    public void DoClickUnhidePassword()
    {
        if (m_PasswordTMPIF.contentType == TMP_InputField.ContentType.Password) m_PasswordTMPIF.contentType = TMP_InputField.ContentType.Standard;
        else m_PasswordTMPIF.contentType = TMP_InputField.ContentType.Password;
        m_PasswordTMPIF.ForceLabelUpdate();
        UIManager.DoClickBase();
    }
    public void DoClickUnhideReEnterPassword()
    {
        if (m_ReEnterPasswordTMPIF.contentType == TMP_InputField.ContentType.Password) m_ReEnterPasswordTMPIF.contentType = TMP_InputField.ContentType.Standard;
        else m_ReEnterPasswordTMPIF.contentType = TMP_InputField.ContentType.Password;
        m_ReEnterPasswordTMPIF.ForceLabelUpdate();
        UIManager.DoClickBase();
    }
    public void DoClickGoogleLinking()
    {
        NetworkManager.INSTANCE.LogInOrLinkGoogle(false);
        UIManager.DoClickBase(m_ButtonGGLinking.transform);
    }
    public void DoClickSupport247()
    {
        UIManager.INSTANCE.OpenPopupSupport();
        UIManager.DoClickBase(m_ButtonSupport247.transform);
    }
    #endregion

    public override void HandleData(string _apiName, string _data)
    {
        base.HandleData(_apiName, _data);
        switch (_apiName)
        {
            case DataSender.REGISTER_QUICK_PLAY:
                {
                    PlayerPrefs.SetString(Database.USERNAME, m_UsernameTMPIF.text);
                    PlayerPrefs.SetString(Database.PASSWORD, m_PasswordTMPIF.text);
                    Destroy(gameObject);
                    break;
                }
            case DataSender.LINK_GOOGLE_ACCOUNT:
                {
                    UIManager.Announce("Account linked successfully");
                    Destroy(gameObject);
                    break;
                }
        }
    }
}
