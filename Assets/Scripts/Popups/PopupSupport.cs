using UnityEngine;

public class PopupSupport : BasePopup
{
    [SerializeField] private Transform m_ButtonMessengerTf, m_ButtonTelegramTf;

    #region Button
    public void DoClickMessenger()
    {
        Application.OpenURL(Database.DB.SupportMessenger);
        UIManager.DoClickBase(m_ButtonMessengerTf);
    }
    public void DoClickTelegram()
    {
        Application.OpenURL(Database.DB.SupportTelegram);
        UIManager.DoClickBase(m_ButtonTelegramTf);
    }
    #endregion
}
