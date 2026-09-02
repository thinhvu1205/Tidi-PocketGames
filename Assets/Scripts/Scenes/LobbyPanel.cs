using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gpm.WebView;
using SimpleJSON;
using TMPro;
using UnityEngine;

public class LobbyPanel : GameListener
{
    [SerializeField] private List<GamesTab> m_TabGTs;
    [SerializeField] private GameObject m_PanelLoggedin, m_PanelUnloggedin, m_ContentTop, m_ContentMiddle;
    [SerializeField] private Transform m_BannersDataTf, m_PanelCashFlowsTf;
    [SerializeField] private List<ItemOnOff> m_FooterIOOs;
    [SerializeField] private PopupLogin m_LoginPanelPL;
    [SerializeField] private TextMeshProUGUI m_AssetTMPUGUI, m_UsernameTMPUGUI, m_CountUnreadMailsTMPUGUI;
    [SerializeField] private PagesSlider m_BannersPS;
    [SerializeField] private PoolGroup m_GameIconsPG;
    private List<PoolInfo> _GamesDataPIs = new();
    private WaitForSeconds _BannersAutoSwipeDelayWFS = new(3f), _UnreadMailsNotiWFS = new(5f);
    private const string _WEBVIEW_CLOSE_SCHEME = "gpmclose";
    private const string _WEBVIEW_CLOSE_BUTTON_JS = "(function(){" + "function addCloseBtn(){" + "if(document.getElementById('gpm-unity-close'))return;" + "var parent=document.body||document.documentElement;" + "if(!parent)return;" + "var b=document.createElement('button');" + "b.id='gpm-unity-close';" + "b.type='button';" + "b.textContent='\\u003C';" + "b.style.cssText='position:fixed;top:max(12px,env(safe-area-inset-top));left:max(12px,env(safe-area-inset-left));z-index:2147483647;display:flex;align-items:center;justify-content:center;box-sizing:border-box;width:40px;height:40px;border:0;border-radius:20px;background:rgba(0,0,0,.55);color:#fff;font-size:26px;line-height:1;text-align:center;padding:0;margin:0;-webkit-tap-highlight-color:transparent;';" + "b.onclick=function(e){e.preventDefault();e.stopPropagation();location.href='" + _WEBVIEW_CLOSE_SCHEME + "://close';};" + "parent.appendChild(b);" + "}" + "if(document.body)addCloseBtn();" + "else document.addEventListener('DOMContentLoaded',addCloseBtn);" + "})();";

    #region Button
    public void DoClickOpenLoginPanel()
    {
        m_LoginPanelPL.gameObject.SetActive(true);
        m_LoginPanelPL.DoClickTabLogin();
    }
    public void DoClickOpenSigninPanel()
    {
        m_LoginPanelPL.gameObject.SetActive(true);
        m_LoginPanelPL.DoClickTabSignin();
    }
    public void DoClickHome()
    {
        PlayerPrefs.SetString(Database.USERNAME, "");
        PlayerPrefs.SetString(Database.PASSWORD, "");
        Database.DB.PlayToken = "";
        UIManager.DoClickBase();
        UIManager.SelectAnOnOffItem(m_FooterIOOs, m_FooterIOOs[0]);
        UIManager.INSTANCE.LoadScene(Database.MAIN_SCENE);
    }
    public void DoClickPromotion()
    {
        UIManager.INSTANCE.OpenPopupPromotion();
        UIManager.DoClickBase(m_FooterIOOs[1].transform);
        UIManager.SelectAnOnOffItem(m_FooterIOOs, m_FooterIOOs[1]);
    }
    public void DoClickCICO()
    {
        UIManager.DoClickBase();
        if (string.IsNullOrEmpty(Database.DB.PlayToken))
        {
            UIManager.Announce("PLease login first!");
            return;
        }
        if (m_FooterIOOs[2].IsTurnOn())
            m_PanelCashFlowsTf.DOScale(Vector3.zero, .2f).SetEase(Ease.InBack).OnComplete(() => UIManager.UnselectOnOffItems(m_FooterIOOs));
        else
        {
            m_PanelCashFlowsTf.localScale = Vector3.zero;
            UIManager.SelectAnOnOffItem(m_FooterIOOs, m_FooterIOOs[2]);
            m_PanelCashFlowsTf.DOScale(Vector3.one, .2f).SetEase(Ease.OutBack);
        }
    }
    public void DoClickMail()
    {
        UIManager.DoClickBase(m_FooterIOOs[3].transform);
        if (string.IsNullOrEmpty(Database.DB.PlayToken))
        {
            UIManager.Announce("PLease login first!");
            return;
        }
        UIManager.SelectAnOnOffItem(m_FooterIOOs, m_FooterIOOs[3]);
        UIManager.INSTANCE.OpenPopupMail(() => UIManager.UnselectOnOffItems(m_FooterIOOs));
    }
    public void DoClickAccount()
    {
        if (string.IsNullOrEmpty(Database.DB.PlayToken))
        {
            UIManager.DoClickBase(m_FooterIOOs[4].transform);
            UIManager.Announce("PLease login first!");
            return;
        }
        foreach (Transform aTf in UIManager.GetParentPopup())
        {
            if (!aTf.TryGetComponent<PopupAccount>(out var aPA)) continue;
            aPA.DoClickClose(true);
            UIManager.UnselectOnOffItems(m_FooterIOOs);
            return;
        }
        UIManager.DoClickBase(m_FooterIOOs[4].transform);
        UIManager.SelectAnOnOffItem(m_FooterIOOs, m_FooterIOOs[4]);
        m_ContentTop.SetActive(false);
        m_ContentMiddle.SetActive(false);
        UIManager.INSTANCE.OpenPopupAccount(() =>
            {
                m_ContentTop.SetActive(true);
                m_ContentMiddle.SetActive(true);
                UIManager.UnselectOnOffItems(m_FooterIOOs);
            });
    }
    public void DoClickDeposit()
    {
        UIManager.INSTANCE.OpenPopupDeposit(() => UIManager.UnselectOnOffItems(m_FooterIOOs));
        UIManager.DoClickBase();
    }
    public void DoClickWithdraw()
    {
        UIManager.INSTANCE.OpenPopupWithdraw(() => UIManager.UnselectOnOffItems(m_FooterIOOs));
        UIManager.DoClickBase();
    }
    public void DoClickPayBack()
    {
        UIManager.INSTANCE.OpenPopupPayBack();
        UIManager.DoClickBase();
    }
    public void DoClickSupport()
    {
        UIManager.INSTANCE.OpenPopupSupport();
        UIManager.DoClickBase();
    }
    #endregion

    public override void HandleData(string _apiName, string _data)
    {
        base.HandleData(_apiName, _data);
        switch (_apiName)
        {
            case DataSender.LIST_GAME:
                {
                    JSONArray dataJA = JSON.Parse(_data)["games"].AsArray;
                    Database.DB.GamesInfoD.Clear();
                    Dictionary<int, string> tagNamesD = new();
                    foreach (JSONNode aJN in dataJA)
                    {
                        JSONArray itemJA = aJN["tags"].AsArray;
                        List<GameTag> itemGTs = new();
                        foreach (JSONNode itemJN in itemJA)
                        {
                            GameTag aGT = new()
                            {
                                Id = itemJN["id"].AsInt,
                                Name = itemJN["name"].Value,
                                Slug = itemJN["slug"].Value,
                                SortOrder = itemJN["sortOrder"].AsInt
                            };
                            itemGTs.Add(aGT);
                            tagNamesD.TryAdd(aGT.Id, aGT.Name);
                        }
                        GameInfo aGI = new()
                        {
                            Id = aJN["gameId"].AsInt,
                            Name = aJN["gameName"].Value,
                            ImageUrl = aJN["imageUrl"].Value,
                            DataGTs = itemGTs
                        };
                        Database.DB.GamesInfoD[aGI.Id] = aGI;
                        _ = Database.DB.DownloadAndCacheGameIcons(aGI, default,
                            () =>
                            {
                                try { if (m_GameIconsPG.CheckAPoolInfoIsShown(_GamesDataPIs.Find(x => x.Data == aGI))) m_GameIconsPG.RefreshUI(false); }
                                catch { }
                            });
                    }
                    foreach (GamesTab aGT in m_TabGTs)
                    {
                        int tabId = aGT.GetId();
                        tagNamesD.TryGetValue(tabId, out string tabName);
                        if (string.IsNullOrEmpty(tabName)) tabName = "All Games";
                        aGT.SetTabName(tabName.ToUpper());
                    }
                    m_TabGTs[0].DoClick();
                    break;
                }
            case DataSender.PROFILE:
                {
                    m_PanelLoggedin.SetActive(true);
                    m_PanelUnloggedin.SetActive(false);
                    m_AssetTMPUGUI.SetText(Database.DB.Asset + " " + Database.DB.Currency + "  <link><voffset=0.35em><sprite index=0></voffset></link>");
                    m_UsernameTMPUGUI.SetText("Hi, " + Database.DB.Username);
                    break;
                }
            case DataSender.LIST_MAIL:
                {
                    JSONArray dataJA = JSON.Parse(_data)["mails"].AsArray;
                    int countUnreadMails = 0;
                    foreach (JSONNode aJN in dataJA)
                    {
                        if (countUnreadMails > 9) break;
                        if (!aJN["isRead"].AsBool) countUnreadMails++;
                    }
                    m_CountUnreadMailsTMPUGUI.transform.parent.gameObject.SetActive(countUnreadMails > 0);
                    m_CountUnreadMailsTMPUGUI.SetText(countUnreadMails + (countUnreadMails > 9 ? "+" : ""));
                    break;
                }
            case DataSender.LAUNCH_GAME:
                {
                    _ShowLaunchGameWebView(_data);
                    break;
                }
        }
    }

    private void _ShowLaunchGameWebView(string html)
    {
        GpmWebView.ShowHtmlString(html,
            new GpmWebViewRequest.Configuration()
            {
                style = GpmWebViewStyle.FULLSCREEN,
                orientation = GpmOrientation.PORTRAIT,
                isClearCookie = true,
                isClearCache = true,
                isNavigationBarVisible = false,
                isCloseButtonVisible = false,
                margins = new GpmWebViewRequest.Margins
                {
                    hasValue = true,
                    left = 0,
                    top = 0,
                    right = 0,
                    bottom = 0
                },
                supportMultipleWindows = true,
                addJavascript = _WEBVIEW_CLOSE_BUTTON_JS,
#if UNITY_IOS
                contentMode = GpmWebViewContentMode.MOBILE,
                isMaskViewVisible = true,
#endif
            },
            _OnLaunchGameWebViewCallback,
            new List<string>() { _WEBVIEW_CLOSE_SCHEME });
    }
    private void _OnLaunchGameWebViewCallback(GpmWebViewCallback.CallbackType callbackType, string data, GpmWebViewError error)
    {
        switch (callbackType)
        {
            case GpmWebViewCallback.CallbackType.PageLoad:
                GpmWebView.ExecuteJavaScript(_WEBVIEW_CLOSE_BUTTON_JS);
                break;
            case GpmWebViewCallback.CallbackType.Scheme:
                if (string.IsNullOrEmpty(data) == false && data.StartsWith(_WEBVIEW_CLOSE_SCHEME))
                    GpmWebView.Close();
                break;
        }
    }

    private void Start()
    {
        m_LoginPanelPL.TryAutoLogin();
        foreach (ItemOnOff aIOO in m_FooterIOOs) aIOO.transform.DOScale(Vector3.one, .7f).SetEase(Ease.OutBack);
        StartCoroutine(initBanners());
        StartCoroutine(sendUpdateMailsNoti());
        m_CountUnreadMailsTMPUGUI.transform.parent.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .3f).SetLoops(-1, LoopType.Yoyo);

        IEnumerator initBanners()
        {
            List<Transform> bannerDataTfs = new();
            foreach (Transform aTf in m_BannersDataTf) bannerDataTfs.Add(aTf);
            yield return m_BannersPS.Init(bannerDataTfs, new(true, true, true, true));
            while (true)
            {
                yield return _BannersAutoSwipeDelayWFS;
                m_BannersPS.DoClickRight();
            }
        }
        IEnumerator sendUpdateMailsNoti()
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(Database.DB.PlayToken)) DataSender.GetListMail();
                yield return _UnreadMailsNotiWFS;
            }
        }
    }
    private void OnEnable()
    {
        m_CountUnreadMailsTMPUGUI.transform.parent.gameObject.SetActive(false);
        UIManager.INSTANCE.OpenPopupBanner();
        NetworkManager.getInstance().UserLogout = false;
        DataSender.GetListGames();
        m_PanelLoggedin.SetActive(false);
        m_PanelUnloggedin.SetActive(true);
        SoundManager.INSTANCE.PlayMusic();
    }
    protected override void Awake()
    {
        base.Awake();
        m_AssetTMPUGUI.GetComponent<Hyperlink>().Init(Camera.main, () =>
            {
                DataSender.GetProfile();
                UIManager.DoClickBase(m_AssetTMPUGUI.transform);
            });
        m_GameIconsPG.SetControlCbs((aRT, aPI) => { aRT.GetComponent<GameItem>().SetData((GameInfo)aPI.Data); });
        foreach (GamesTab tabGT in m_TabGTs)
        {
            tabGT.SetOnClickCb(() =>
            {
                foreach (GamesTab aGT in m_TabGTs) aGT.UnSelect();
                _GamesDataPIs.Clear();
                if (tabGT.GetId() == 0)
                    foreach (KeyValuePair<int, GameInfo> aKVP in Database.DB.GamesInfoD)
                    {
                        aKVP.Value.IsRunShowingEffect = true;
                        _GamesDataPIs.Add(new() { Data = aKVP.Value });
                    }
                else
                {
                    foreach (KeyValuePair<int, GameInfo> aKVP in Database.DB.GamesInfoD)
                    {
                        foreach (GameTag tagGT in aKVP.Value.DataGTs)
                        {
                            if (tagGT.Id != tabGT.GetId()) continue;
                            aKVP.Value.IsRunShowingEffect = true;
                            _GamesDataPIs.Add(new() { Data = aKVP.Value });
                            break;
                        }
                    }
                }
                m_GameIconsPG.SetControlInfo(_GamesDataPIs);
                m_GameIconsPG.ScrollToItem(0);
            });
        }
    }
}
