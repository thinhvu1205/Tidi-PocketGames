using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using JO = SimpleJSON.JSONObject;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager INSTANCE { get; private set; }
    [HideInInspector] public bool UserLogout;
    private List<GameListener> _DataGLs = new();
    private Queue<Action> jobsResend = new();

    public static NetworkManager getInstance() { return INSTANCE; }
    public void AddListener(GameListener _aGL) => _DataGLs.Add(_aGL);
    public void RemoveListener(GameListener _aGL) => _DataGLs.Remove(_aGL);
    public void SendGET(string _apiName, string _tail = "", JO _dataJO = null)
    {
        StartCoroutine(sendGetRequest());
        IEnumerator sendGetRequest()
        {
            string url = Database.BASE_URL + _apiName + _tail, data = "";
            if (_dataJO != null)
            {
                data = _dataJO.ToString();
                url += "?";
                foreach (string aKey in _dataJO.Keys)
                {
                    if (!url.EndsWith("?")) url += "&";
                    url += aKey + "=" + _dataJO[aKey].ToString();
                }
            }
            using UnityWebRequest aUWR = new(url, UnityWebRequest.kHttpVerbGET);
            Debug.Log("|     )  )=3 send GET " + url + " | " + data + " | " + Database.DB.PlayToken);
            aUWR.downloadHandler = new DownloadHandlerBuffer();
            aUWR.timeout = 20;
            aUWR.SetRequestHeader("Content-Type", "application/json");
            aUWR.SetRequestHeader("Accept", "application/json");
            aUWR.SetRequestHeader("Authorization", "Bearer " + Database.DB.PlayToken);
            yield return aUWR.SendWebRequest();

            if (aUWR.result != UnityWebRequest.Result.Success) _HandleError(_apiName, aUWR);
            else if (aUWR.downloadedBytes > 0) _HandleReceivedData(_apiName, aUWR.downloadHandler.text);
        }
    }
    public void SendPOST(string _apiName, string _tail = "", JO _dataJO = null)
    {
        StartCoroutine(sendPostRequest());
        IEnumerator sendPostRequest()
        {
            string url = Database.BASE_URL + _apiName, data = _dataJO.ToString();
            using UnityWebRequest aUWR = new(url, UnityWebRequest.kHttpVerbPOST);
            Debug.Log("|     )  )=3 send POST " + url + " | " + data + " | " + Database.DB.PlayToken);
            aUWR.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));
            aUWR.downloadHandler = new DownloadHandlerBuffer();
            aUWR.timeout = 20;
            aUWR.SetRequestHeader("Content-Type", "application/json");
            aUWR.SetRequestHeader("Accept", "application/json");
            aUWR.SetRequestHeader("Authorization", "Bearer " + Database.DB.PlayToken);
            yield return aUWR.SendWebRequest();

            if (aUWR.result != UnityWebRequest.Result.Success) _HandleError(_apiName, aUWR);
            else if (aUWR.downloadedBytes > 0) _HandleReceivedData(_apiName, aUWR.downloadHandler.text);
        }
    }
    private void _HandleReceivedData(string _apiName, string _data)
    {
        Debug.Log("|     )  )=3 " + _apiName + " | " + _data);
        switch (_apiName)
        {
            case DataSender.LOGIN:
            case DataSender.REGISTER:
            case DataSender.QUICK_PLAY:
            case DataSender.GOOGLE_LOGIN:
            case DataSender.LINK_GOOGLE_ACCOUNT:
                {
                    Database.DB.PlayToken = JSON.Parse(_data)["token"].Value;
                    Database.DB.LoginType = Database.LOGIN_TYPE.NORMAL;
                    DataSender.GetSettings();
                    DataSender.GetProfile();
                    UIManager.INSTANCE.HideLoading();
                    break;
                }
            case DataSender.REGISTER_QUICK_PLAY:
                {
                    UIManager.Announce("Register successfully");
                    DataSender.GetSettings();
                    DataSender.GetProfile();
                    UIManager.INSTANCE.HideLoading();
                    break;
                }
            case DataSender.SETTINGS:
                {
                    JSONNode dataJN = JSON.Parse(_data);
                    JO depositJO = dataJN["depositChannels"].AsObject, withdrawJO = dataJN["withdrawChannels"].AsObject,
                        socialLinksJO = dataJN["socialLinks"].AsObject, supportLinksJO = dataJN["supportLinks"].AsObject;
                    // vip
                    Database.DB.VipVIs.Clear();
                    JSONArray vipJA = dataJN["vips"].AsArray;
                    foreach (JSONNode aJN in vipJA)
                    {
                        Database.DB.VipVIs.Add(new()
                        {
                            Level = aJN["level"].AsInt,
                            TotalDeposit = aJN["totalDeposit"].AsLong,
                            TotalBet = aJN["totalBet"].AsLong
                        });
                    }
                    // deposit
                    Database.DB.DepositPCs.Clear();
                    foreach (string key in depositJO.Keys)
                    {
                        PaymentChannel aPC = new() { Name = key };
                        JSONArray accountJA = depositJO[key]["accounts"].AsArray, amountJA = depositJO[key]["amounts"].AsArray;
                        foreach (JSONNode aJN in accountJA) aPC.DepositDAs.Add(new() { AccountName = aJN["name"].Value, AccountNumber = aJN["number"].Value });
                        foreach (JSONNode aJN in amountJA) aPC.Amounts.Add(aJN.AsLong);
                        Database.DB.DepositPCs.Add(aPC);
                    }
                    // withdraw
                    Database.DB.WithdrawPCs.Clear();
                    foreach (string key in withdrawJO.Keys)
                    {
                        PaymentChannel aPC = new() { Name = key };
                        JSONArray amountJA = withdrawJO[key]["amounts"].AsArray;
                        foreach (JSONNode aJN in amountJA) aPC.Amounts.Add(aJN.AsLong);
                        Database.DB.WithdrawPCs.Add(aPC);
                    }

                    // social and support
                    Database.DB.SocialMessenger = socialLinksJO["facebook"].Value;
                    Database.DB.SocialTelegram = socialLinksJO["telegram"].Value;
                    Database.DB.SupportMessenger = supportLinksJO["facebook"].Value;
                    Database.DB.SupportTelegram = supportLinksJO["telegram"].Value;
                    break;
                }
            case DataSender.PROFILE:
                {
                    JO userDataJO = JSON.Parse(_data)["user"].AsObject;
                    Database.DB.UserId = userDataJO["sid"].Value;
                    Database.DB.Username = userDataJO["name"].Value;
                    Database.DB.Asset = userDataJO["balance"].AsLong;
                    Database.DB.Currency = userDataJO["currency"].Value;
                    Database.DB.TotalDeposit = userDataJO["totalDeposit"].AsLong;
                    Database.DB.TotalBet = userDataJO["totalBet"].AsLong;
                    Database.DB.RequiredBet = userDataJO["requiredBet"].AsLong;
                    Database.DB.VipLevel = userDataJO["vipLevel"].AsInt;
                    Database.DB.IsOfficial = userDataJO["official"].AsBool;
                    break;
                }
        }
        for (int i = _DataGLs.Count - 1; i >= 0; i--) _DataGLs[i].HandleData(_apiName, _data);
    }
    private void _HandleError(string _apiName, UnityWebRequest _aUWR)
    {
        string bodyError = _aUWR.downloadHandler != null ? _aUWR.downloadHandler.text : "";
        Debug.LogError("|     )  )=3 " + _apiName + " | result: " + _aUWR.result + " | code: " + _aUWR.responseCode + " | error: " + _aUWR.error + " | details: " + bodyError);
        switch (_apiName)
        {
            case DataSender.QUICK_PLAY:
            case DataSender.REGISTER:
            case DataSender.LOGIN:
            case DataSender.GOOGLE_LOGIN:
            case DataSender.REGISTER_QUICK_PLAY:
            case DataSender.LINK_GOOGLE_ACCOUNT:
                {
                    UIManager.Announce(JSON.Parse(bodyError)["message"].Value);
                    UIManager.INSTANCE.HideLoading();
                    break;
                }
            case DataSender.CLAIM_MAIL:
            case DataSender.DELETE_MAIL:
                {
                    JSONNode errorJN = JSON.Parse(bodyError);
                    UIManager.Announce(errorJN["message"].Value);
                    break;
                }
            case DataSender.CLAIM_DEPOSIT:
                {
                    UIManager.Announce("Please recheck the transaction Id and try again later");
                    break;
                }
            case DataSender.WITHDRAW:
                {
                    UIManager.Announce("Withdrawal request has been accepted");
                    DataSender.GetProfile();
                    break;
                }
        }
        for (int i = _DataGLs.Count - 1; i >= 0; i--) _DataGLs[i].HandleError(_apiName, bodyError);
    }
    public void LogInGoogle()
    {
        GoogleSignIn.Configuration = new()
        {
            WebClientId = Database.BASE_URL,
            RequestIdToken = true,
            UseGameSignIn = false,
            ForceTokenRefresh = true
        };
        GoogleSignIn.DefaultInstance.SignOut();
        GoogleSignIn.DefaultInstance.Disconnect();
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(aGSIU =>
        {
            if (aGSIU.IsFaulted)
            {
                foreach (Exception aE in aGSIU.Exception.InnerExceptions)
                {
                    if (aE is GoogleSignIn.SignInException aSIE) Debug.LogError("|     )  )=3 Google Sign-In error: Status: " + aSIE.Status + " | Message: " + aSIE.Message);
                    else Debug.LogError("|     )  )=3 Google Sign-In unknown error: " + aE.GetType().Name + " | Message: " + aE.Message);
                }
            }
            else if (aGSIU.IsCanceled) Debug.LogError("|     )  )=3 Google Sign-In was canceled");
            else
            {
                string googleToken = aGSIU.Result.IdToken;
                Debug.Log("|     )  )=3 Google Sign-In success: " + aGSIU.Result.DisplayName + " | token: " + googleToken);
                DataSender.LoginWithGoogle(googleToken);
            }

        }, TaskScheduler.FromCurrentSynchronizationContext());
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
    }
}