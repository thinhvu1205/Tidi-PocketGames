using System;
using System.Collections.Generic;
using UnityEngine;
using JO = SimpleJSON.JSONObject;
using WITHDRAW_STATUS = Database.WITHDRAW_STATUS;

public class DataSender
{
    #region API-Keys
    public const string QUICK_PLAY = "api/v1/auth/quick-play", REGISTER = "api/v1/auth/register", REGISTER_QUICK_PLAY = "api/v1/auth/register-quick-play",
        LOGIN = "api/v1/auth/login", SETTINGS = "api/v1/settings", PROFILE = "api/v1/user/me", LIST_GAME = "api/v1/games", LAUNCH_GAME = "api/v1/auth/launch",
        LIST_MAIL = "api/v1/mail/my", READ_MAIL = "api/v1/mail/read", CLAIM_MAIL = "api/v1/mail/claim", DELETE_MAIL = "api/v1/mail/delete",
        GOOGLE_LOGIN = "api/v1/auth/google/login", LINK_GOOGLE_ACCOUNT = "api/v1/auth/link-google", CLAIM_DEPOSIT = "api/v1/deposit/claim",
        WITHDRAW_HISTORY = "api/v1/withdraw/mine", WITHDRAW = "api/v1/withdraw", ATTRIBUTES = "api/v1/tracking/attributes", INSTALL = "/api/v1/tracking/install",
        TRACKING_EVENT = "/api/v1/tracking/events";
    #endregion

    public static void QuickPlay() { UIManager.INSTANCE.ShowLoading(); _SendPOST(QUICK_PLAY, "", new() { ["id"] = SystemInfo.deviceUniqueIdentifier }); }
    public static void Register(string _acc, string _pass) { UIManager.INSTANCE.ShowLoading(); _SendPOST(REGISTER, "", new() { ["userName"] = _acc, ["password"] = _pass }); }
    public static void RegisterQuickPlay(string _acc, string _pass) { UIManager.INSTANCE.ShowLoading(); _SendPOST(REGISTER_QUICK_PLAY, "", new() { ["userName"] = _acc, ["password"] = _pass }); }
    public static void Login(string _acc, string _pass) { UIManager.INSTANCE.ShowLoading(); _SendPOST(LOGIN, "", new() { ["userName"] = _acc, ["password"] = _pass }); }
    public static void LoginWithGoogle(string _token) { UIManager.INSTANCE.ShowLoading(); _SendGET(GOOGLE_LOGIN, "", new() { ["token"] = _token }); }
    public static void LinkGoogleAccount(string _token) { UIManager.INSTANCE.ShowLoading(); _SendPOST(LINK_GOOGLE_ACCOUNT, "", new() { ["token"] = _token }); }
    public static void GetSettings() => _SendGET(SETTINGS);
    public static void GetProfile() => _SendGET(PROFILE);
    public static void GetListGames() => _SendGET(LIST_GAME);
    public static void LaunchGame(string _gameId) => _SendPOST(LAUNCH_GAME, "", new() { ["gameId"] = _gameId });
    public static void GetListMail(int _pageId = 1, int _pageSize = 100) => _SendGET(LIST_MAIL, "", new() { ["page"] = _pageId, ["pageSize"] = _pageSize });
    public static void SetMailAsRead(string _mailId) => _SendGET(READ_MAIL, "/" + _mailId);
    public static void ClaimMailRewards(string _mailId) => _SendGET(CLAIM_MAIL, "/" + _mailId);
    public static void DeleteMail(List<string> _mailIds) => _SendPOST(DELETE_MAIL, "", new() { ["ids"] = _mailIds });
    public static void ClaimDeposit(string _transactionId) => _SendPOST(CLAIM_DEPOSIT, "", new() { ["tx"] = _transactionId });
    public static void GetWithdrawHistory()
        => _SendGET(WITHDRAW_HISTORY, "", new() { ["page"] = 1, ["pageSize"] = 100, ["status"] = (int)WITHDRAW_STATUS.UNSPECIFIED });
    public static void Withdraw(long _amount, string _account, string _channel)
        => _SendPOST(WITHDRAW, "", new() { ["amount"] = _amount, ["receiver"] = _account, ["channel"] = _channel });
    public static void SendLoginAttributes()
    {
        _SendPOST(ATTRIBUTES, "", new()
        {
            ["utm_source"] = "",
            ["utm_medium"] = "",
            ["utm_campaign"] = "",
            ["utm_content"] = "",
            ["utm_term"] = "",
            ["affiliate_id"] = "",
            ["sub_id"] = "",
            ["referrer"] = "",
            ["landing_page"] = "",
            ["platform"] = Database.PLATFORM_ID,
            ["fbclid"] = "",
            ["fbp"] = "",
            ["publisher_code"] = Database.PUBLISHER
        });
    }
    public static void SendInstall()
    {
        if (PlayerPrefs.GetInt(Database.STORED_IS_INSTALL_SENT, 0) == 1) return; // already sent
        _SendPOST(INSTALL, "", new()
        {
            ["platform"] = Database.PLATFORM,
            ["publisher_code"] = Database.PUBLISHER,
            ["event_id"] = SystemInfo.deviceUniqueIdentifier,
            ["event_time"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["fbclid"] = "",
            ["fbp"] = "",
            ["ttclid"] = "",
            ["ttp"] = "",
            ["app_version"] = Application.version
        });
    }
    public static void SendInitiateCheckout(float _checkoutValue, string _paymentChannel)
    {
        _SendPOST(TRACKING_EVENT, "", new()
        {
            ["event_name"] = "InitiateCheckout",
            ["event_id"] = Guid.NewGuid().ToString("N"),
            ["event_time"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["event_source_url"] = "",
            ["platform"] = Database.PLATFORM,
            ["value"] = _checkoutValue,
            ["currency"] = Database.DB.Currency,
            ["custom_data_json"] = "{\"payment_method\":\"" + _paymentChannel + "\"}",
            ["fbp"] = "",
            ["ttclid"] = "",
            ["ttp"] = ""
        });
    }

    private static void _SendGET(string _apiName, string _tail = "", JO _dataJO = null) => NetworkManager.INSTANCE.SendGET(_apiName, _tail, _dataJO);
    private static void _SendPOST(string _apiName, string _tail = "", JO _dataJO = null) => NetworkManager.INSTANCE.SendPOST(_apiName, _tail, _dataJO);
}
