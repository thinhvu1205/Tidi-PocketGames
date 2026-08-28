using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager INSTANCE;
    [HideInInspector] public List<PopupAnnouncement> PoolPAs = new();
    [SerializeField] GameObject m_Loading;
    [SerializeField] private Transform _ParentPopupsTf;
    private float _TimeShowLoading = 0;

    public static void DoClickBase(Transform _targetTf = null)
    {
        if (_targetTf != null) _targetTf.DOScale(.95f, 0.1f).SetLoops(2, LoopType.Yoyo);
        SoundManager.INSTANCE.SoundClick();
    }
    public static void UnselectOnOffItems(List<ItemOnOff> _dataIOOs) { foreach (ItemOnOff aIOO in _dataIOOs) aIOO.TurnOff(); }
    public static void SelectAnOnOffItem(List<ItemOnOff> _dataIOOs, ItemOnOff _aIOO)
    {
        UnselectOnOffItems(_dataIOOs);
        _aIOO.TurnOn();
    }
    public static Transform GetParentPopup() => INSTANCE._ParentPopupsTf;
    public static void Announce(string _content, string _button1Label = "", string _button2Label = "",
        Action _onClickButton1Cb = null, Action _onClickButton2Cb = null, Action _onClickCloseCb = null)
    {
        if (string.IsNullOrEmpty(_content)) return;
        PopupAnnouncement foundPA = null;
        foreach (PopupAnnouncement aPA in INSTANCE.PoolPAs)
        {
            if (aPA.gameObject.activeSelf) continue;
            foundPA = aPA;
            break;
        }
        if (foundPA == null)
        {
            foundPA = BundleHandler.Instantiate(BundleHandler.LoadGameObject(Database.PU_ANNOUNCEMENT), GetParentPopup()).GetComponent<PopupAnnouncement>();
            INSTANCE.PoolPAs.Add(foundPA);
        }
        foundPA.SetData(_content, _button1Label, _button2Label, _onClickButton1Cb, _onClickButton2Cb, _onClickCloseCb);
        foundPA.gameObject.SetActive(true);
        foundPA.transform.SetAsLastSibling();
    }
    public void LoadScene(string _sceneName)
    {
        HideLoading();
        foreach (Transform aTf in _ParentPopupsTf) Destroy(aTf.gameObject);
        SceneManager.LoadScene(_sceneName);
    }
    public void ShowLoading(float _timeOut = 10)
    {
        if (m_Loading.activeSelf) return;
        _TimeShowLoading = _timeOut;
        m_Loading.SetActive(true);
    }
    public void HideLoading()
    {
        _TimeShowLoading = 0;
        m_Loading.SetActive(false);
    }
    public void OpenPopupMail(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_MAIL, _onCloseCb);
    public void OpenPopupDeposit(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_DEPOSIT, _onCloseCb);
    public void OpenPopupWithdraw(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_WITHDRAW, _onCloseCb);
    public void OpenPopupUpgradeAccount(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_UPGRADE_ACCOUNT, _onCloseCb);
    public void OpenPopupVip(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_VIP, _onCloseCb);
    public void OpenPopupSupport(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_SUPPORT, _onCloseCb);
    public void OpenPopupAccount(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_ACCOUNT, _onCloseCb);
    public void OpenPopupBanner(Action _onCloseCb = null) => _InstantiatePopup(Database.PU_BANNER, _onCloseCb);
    public void OpenPopupRuleCashFlow(bool _isDeposit, Action _onCloseCb = null)
        => ((PopupRuleCashFlow)_InstantiatePopup(Database.PU_RULE_CASH_FLOW, _onCloseCb)).Show(_isDeposit);
    public void OpenPopupPayBack(Action _onCloseCb = null) => Announce("Coming soon");
    public void OpenPopupChangePass(Action _onCloseCb = null) => Announce("Coming soon");
    public void OpenPopupReferral(Action _onCloseCb = null) => Announce("Coming soon");
    public void OpenPopupNotification(Action _onCloseCb = null) => Announce("Coming soon");
    public void OpenPopupPromotion(Action _onCloseCb = null) => Announce("Coming soon");
    private BasePopup _InstantiatePopup(string _path, Action _onCloseCb = null)
    {
        BasePopup aBP = BundleHandler.Instantiate(BundleHandler.LoadGameObject(_path), GetParentPopup()).GetComponent<BasePopup>();
        aBP.SetOnCloseCb(_onCloseCb);
        return aBP;
    }

    // public void OnApplicationQuit()
    // {
    //     Debug.LogWarning("-=-=OnApplicationQuit ");
    // }
    // public void OnApplicationPause(bool _isPause)
    // {
    //     if (_isPause)
    //     {
    //         Debug.Log("-=-=OnApplicationPause ");
    //         NetworkManager.getInstance().UserLogout = true;
    //     }
    //     else
    //     {
    //         StartCoroutine(delayTurnOffUserLogout());
    //     }

    //     IEnumerator delayTurnOffUserLogout()
    //     {
    //         yield return new WaitForSeconds(.5f);
    //         if (INSTANCE.LobbyLP.gameObject.activeSelf)
    //             NetworkManager.getInstance().UserLogout = false;
    //     }
    // }

    private void Update()
    {
        if (_TimeShowLoading > 0)
        {
            _TimeShowLoading -= Time.deltaTime;
            if (_TimeShowLoading <= 0) HideLoading();
        }
    }
    private void Awake()
    {
        if (INSTANCE == null) INSTANCE = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Input.multiTouchEnabled = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
