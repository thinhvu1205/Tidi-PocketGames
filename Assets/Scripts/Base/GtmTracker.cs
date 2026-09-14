using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public static class GtmTracker
{
    // private static readonly string[] _FbPixelIds =
    // {
    //     "1041299715026464",
    //     "1321123543505150",
    //     "1663910502405992",
    //     "2070881370487794"
    // };
    private static string _SessionId;
    private static bool _IsSessionStarted;
    private const string _GTM_CONTAINER_ID = "GTM-5TLWDH72", _GA_MEASUREMENT_ID = "G-T37KEC9X8Q", _EVENT_COMPLETE_REGISTRATION = "CompleteRegistration",
        _GTM_CLIENT_ID = "GTM_CLIENT_ID", _GTM_SESSION_COUNT = "GTM_SESSION_COUNT", _WEB_PAGE_URL = "https://h5.rubyclubph.com/", _WEB_PAGE_TITLE = "Ruby Club";

    public static void SendCompleteRegistration()
    {
        if (NetworkManager.INSTANCE == null) return;
        NetworkManager.INSTANCE.StartCoroutine(_SendCompleteRegistration());
    }

    private static IEnumerator _SendCompleteRegistration()
    {
        string clientId = _GetOrCreateClientId(), sessionId = _GetSessionId();
        int sessionCount = PlayerPrefs.GetInt(_GTM_SESSION_COUNT, 1);
        string lang = CultureInfo.CurrentCulture.Name.ToLowerInvariant();
        if (string.IsNullOrEmpty(lang)) lang = "en-us";
        string screen = Screen.width + "x" + Screen.height;
        long pageId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        List<string> query = new()
        {
            "v=2",
            "tid=" + _GA_MEASUREMENT_ID,
            "cid=" + UnityWebRequest.EscapeURL(clientId),
            "sid=" + sessionId,
            "sct=" + sessionCount,
            "seg=1",
            "_s=1",
            "_p=" + pageId,
            "ul=" + UnityWebRequest.EscapeURL(lang),
            "sr=" + screen,
            "dl=" + UnityWebRequest.EscapeURL(_WEB_PAGE_URL),
            "dt=" + UnityWebRequest.EscapeURL(_WEB_PAGE_TITLE),
            "en=" + _EVENT_COMPLETE_REGISTRATION,
            "ep.platform=apk"
        };
        if (!_IsSessionStarted)
        {
            query.Add("_ss=1");
            if (sessionCount <= 1) query.Add("_fv=1");
            _IsSessionStarted = true;
        }
        // call to gg tag
        yield return _Get("https://www.google-analytics.com/g/collect?" + string.Join("&", query));

        // call to fb ads
        // foreach (string pixelId in _FbPixelIds)
        // {
        //     yield return _Get("https://www.facebook.com/tr?id=" + pixelId
        //         + "&ev=" + _EVENT_COMPLETE_REGISTRATION
        //         + "&noscript=1&dl=" + UnityWebRequest.EscapeURL(_WEB_PAGE_URL));
        // }
        Debug.Log("|   ) )=3 GTM " + _EVENT_COMPLETE_REGISTRATION + " sent | " + _GTM_CONTAINER_ID + " | " + _GA_MEASUREMENT_ID);
    }
    private static IEnumerator _Get(string _url)
    {
        using UnityWebRequest aUWR = UnityWebRequest.Get(_url);
        aUWR.timeout = 15;
        aUWR.SetRequestHeader("Accept", "*/*");
        yield return aUWR.SendWebRequest();
        if (aUWR.result != UnityWebRequest.Result.Success)
            Debug.LogWarning("|   ) )=3 GTM track failed | " + aUWR.responseCode + " | " + aUWR.error + " | " + _url);
    }
    private static string _GetOrCreateClientId()
    {
        string clientId = PlayerPrefs.GetString(_GTM_CLIENT_ID, "");
        if (!string.IsNullOrEmpty(clientId)) return clientId;
        clientId = Guid.NewGuid().ToString();
        PlayerPrefs.SetString(_GTM_CLIENT_ID, clientId);
        PlayerPrefs.Save();
        return clientId;
    }
    private static string _GetSessionId()
    {
        if (!string.IsNullOrEmpty(_SessionId)) return _SessionId;
        _SessionId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        int sessionCount = PlayerPrefs.GetInt(_GTM_SESSION_COUNT, 0) + 1;
        PlayerPrefs.SetInt(_GTM_SESSION_COUNT, sessionCount);
        PlayerPrefs.Save();
        return _SessionId;
    }
}
