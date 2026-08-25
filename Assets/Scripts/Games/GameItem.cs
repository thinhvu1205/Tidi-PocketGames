using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameItem : MonoBehaviour
{
    [SerializeField] private Image m_IconImg;
    [SerializeField] private TextMeshProUGUI m_NameTMPUGUI;
    private string _Id = "-1";

    #region Button
    public void DoClick()
    {
        UIManager.DoClickBase();
        if (!string.IsNullOrEmpty(Database.DB.PlayToken)) DataSender.LaunchGame(_Id);
    }
    #endregion

    public void SetData(GameInfo _aGI)
    {
        _Id = _aGI.Id.ToString();
        m_NameTMPUGUI.SetText(_aGI.Name);
        if (_aGI.IconS != null) m_IconImg.sprite = _aGI.IconS;
    }
}
