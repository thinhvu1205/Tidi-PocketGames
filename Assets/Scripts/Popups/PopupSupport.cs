using UnityEngine;

public class PopupSupport : BasePopup
{
    [SerializeField] private Transform m_ButtonMessengerTf, m_ButtonTelegramTf;

    #region Button
    public void DoClickMessenger()
    {
        if (string.IsNullOrEmpty(Database.DB.SupportMessenger)) Application.OpenURL("https://www.facebook.com/rubyclub.ph");
        else Application.OpenURL(Database.DB.SupportMessenger);
        UIManager.DoClickBase(m_ButtonMessengerTf);
    }
    public void DoClickTelegram()
    {
        if (string.IsNullOrEmpty(Database.DB.SupportTelegram)) Application.OpenURL("https://t.me/rubyclubph");
        else Application.OpenURL(Database.DB.SupportTelegram);
        UIManager.DoClickBase(m_ButtonTelegramTf);
    }
    #endregion
}
