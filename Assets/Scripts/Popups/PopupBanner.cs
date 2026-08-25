using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupBanner : BasePopup
{
    [SerializeField] private Transform m_BannersDataTf;
    [SerializeField] private Image m_BannerImg;
    private List<Sprite> _BannersDataSs = new();
    private int _CurrentId;

    #region Button
    public void DoCLickBanner()
    {
        UIManager.DoClickBase();
    }
    #endregion

    protected override void OnDisable()
    {
        _CurrentId++;
        if (_CurrentId >= _BannersDataSs.Count) _CurrentId = 0;
        base.OnDisable();
    }
    private void OnEnable()
    {
        m_BannerImg.sprite = _BannersDataSs[_CurrentId];
        _ContentTf.localScale = Vector3.zero;
        _ContentTf.DOScale(1f, .2f).SetEase(Ease.OutBack);
    }
    protected override void Awake()
    {
        base.Awake();
        foreach (Transform aTf in m_BannersDataTf) _BannersDataSs.Add(aTf.GetComponent<Image>().sprite);
    }
}
