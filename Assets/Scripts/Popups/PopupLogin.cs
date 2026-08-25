using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupLogin : BasePopup
{
    [SerializeField] private GameObject m_ReEnterPassword, m_ButtonLogin, m_ButtonSignIn, m_ButtonGGLogin, m_ButtonFreePlay, m_ButtonSupport247;
    [SerializeField] private List<ItemOnOff> m_TabIOOs;
    [SerializeField] private Hyperlink m_ForgotPasswordH;
    [SerializeField] private TMP_InputField m_UsernameTMPIF, m_PasswordTMPIF, m_ReEnterPasswordTMPIF;

    #region Button
    public void DoClickTabLogin()
    {
        UIManager.SelectAnOnOffItem(m_TabIOOs, m_TabIOOs[0]);
        m_ButtonLogin.SetActive(true);
        m_ButtonSignIn.SetActive(false);
        m_ReEnterPassword.SetActive(false);
        m_ForgotPasswordH.gameObject.SetActive(false);
        UIManager.DoClickBase(m_TabIOOs[0].transform);
    }
    public void DoClickTabSignin()
    {
        UIManager.SelectAnOnOffItem(m_TabIOOs, m_TabIOOs[1]);
        m_ButtonLogin.SetActive(false);
        m_ButtonSignIn.SetActive(true);
        m_ReEnterPassword.SetActive(true);
        m_ForgotPasswordH.gameObject.SetActive(false);
        UIManager.DoClickBase(m_TabIOOs[1].transform);
    }
    public void DoClickLogIn()
    {
        string username = m_UsernameTMPIF.text, password = m_PasswordTMPIF.text;
        if (username.Equals("") || password.Equals("")) UIManager.Announce("Username and Password must not be empty!");
        else DataSender.Login(username, password);
        UIManager.DoClickBase(m_ButtonSignIn.transform);
    }
    public void DoClickSignIn()
    {
        string username = m_UsernameTMPIF.text, password = m_PasswordTMPIF.text, reEnterPassword = m_ReEnterPasswordTMPIF.text;
        if (username.Equals("") || password.Equals("")) UIManager.Announce("Username and Password must not be empty!");
        else
        {
            if (!password.Equals(reEnterPassword)) UIManager.Announce("Password must be re-entered correctly!");
            else DataSender.Register(username, password);
        }
        UIManager.DoClickBase(m_ButtonLogin.transform);
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
    public void DoClickGoogleLogin()
    {
        NetworkManager.INSTANCE.LogInGoogle();
        UIManager.DoClickBase(m_ButtonGGLogin.transform);
    }
    public void DoClickFreePlay()
    {
        DataSender.QuickPlay();
        UIManager.DoClickBase(m_ButtonFreePlay.transform);
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
            case DataSender.QUICK_PLAY:
            case DataSender.REGISTER:
            case DataSender.LOGIN:
            case DataSender.GOOGLE_LOGIN:
                {
                    PlayerPrefs.SetString(Database.USERNAME, Database.DB.IsOfficial ? "" : m_UsernameTMPIF.text);
                    PlayerPrefs.SetString(Database.PASSWORD, Database.DB.IsOfficial ? "" : m_PasswordTMPIF.text);
                    _EffectSHE.RunCloseEffect(() => gameObject.SetActive(false));
                    break;
                }
        }
    }
    public void TryAutoLogin()
    {
        m_UsernameTMPIF.text = PlayerPrefs.GetString(Database.USERNAME, "");
        m_PasswordTMPIF.text = PlayerPrefs.GetString(Database.PASSWORD, "");
        if (!m_UsernameTMPIF.text.Equals("") && !m_PasswordTMPIF.text.Equals("")) DoClickLogIn();
    }

    private void OnEnable()
    {
        m_UsernameTMPIF.text = PlayerPrefs.GetString(Database.USERNAME, "");
        m_PasswordTMPIF.text = "";
        m_ReEnterPasswordTMPIF.text = "";
    }
    protected override void Awake()
    {
        base.Awake();
        m_ForgotPasswordH.Init(Camera.main, () =>
            {

            });
    }
}
