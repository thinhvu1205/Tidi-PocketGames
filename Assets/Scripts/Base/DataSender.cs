using System.Collections.Generic;
using UnityEngine;
using JO = SimpleJSON.JSONObject;
using WITHDRAW_STATUS = Database.WITHDRAW_STATUS;

public class DataSender
{
    #region APIs
    public const string QUICK_PLAY = "api/v1/auth/quick-play";
    public const string REGISTER = "api/v1/auth/register";
    public const string REGISTER_QUICK_PLAY = "api/v1/auth/register-quick-play";
    public const string LOGIN = "api/v1/auth/login";
    public const string SETTINGS = "api/v1/settings";
    public const string PROFILE = "api/v1/user/me";
    public const string LIST_GAME = "api/v1/games";
    public const string LAUNCH_GAME = "api/v1/auth/launch";
    public const string LIST_MAIL = "api/v1/mail/my";
    public const string READ_MAIL = "api/v1/mail/read";
    public const string CLAIM_MAIL = "api/v1/mail/claim";
    public const string DELETE_MAIL = "api/v1/mail/delete";
    public const string GOOGLE_LOGIN = "api/v1/auth/google/login";
    public const string LINK_GOOGLE_ACCOUNT = "api/v1/auth/link-google";
    public const string CLAIM_DEPOSIT = "api/v1/deposit/claim";
    public const string WITHDRAW_HISTORY = "api/v1/withdraw/mine";
    public const string WITHDRAW = "api/v1/withdraw";
    #endregion

    public static void QuickPlay()
    {
        UIManager.INSTANCE.ShowLoading();
        _SendPOST(QUICK_PLAY, "", new JO() { ["id"] = SystemInfo.deviceUniqueIdentifier });
    }
    public static void Register(string _acc, string _pass)
    {
        UIManager.INSTANCE.ShowLoading();
        _SendPOST(REGISTER, "", new JO() { ["userName"] = _acc, ["password"] = _pass });
    }
    public static void RegisterQuickPlay(string _acc, string _pass)
    {
        UIManager.INSTANCE.ShowLoading();
        _SendPOST(REGISTER_QUICK_PLAY, "", new JO() { ["userName"] = _acc, ["password"] = _pass });
    }
    public static void Login(string _acc, string _pass)
    {
        UIManager.INSTANCE.ShowLoading();
        _SendPOST(LOGIN, "", new JO() { ["userName"] = _acc, ["password"] = _pass });
    }
    public static void LoginWithGoogle(string _token)
    {
        UIManager.INSTANCE.ShowLoading();
        _SendGET(GOOGLE_LOGIN, "", new JO() { ["token"] = _token });
    }
    public static void LinkGoogleAccount(string _token)
    {
        UIManager.INSTANCE.ShowLoading();
        _SendPOST(LINK_GOOGLE_ACCOUNT, "", new JO() { ["token"] = _token });
    }
    public static void GetSettings() => _SendGET(SETTINGS);
    public static void GetProfile() => _SendGET(PROFILE);
    public static void GetListGames() => _SendGET(LIST_GAME);
    public static void LaunchGame(string _gameId) => _SendPOST(LAUNCH_GAME, "", new JO() { ["gameId"] = _gameId });
    public static void GetListMail(int _pageId = 1, int _pageSize = 100) => _SendGET(LIST_MAIL, "", new JO() { ["page"] = _pageId, ["pageSize"] = _pageSize });
    public static void SetMailAsRead(string _mailId) => _SendGET(READ_MAIL, "/" + _mailId);
    public static void ClaimMailRewards(string _mailId) => _SendGET(CLAIM_MAIL, "/" + _mailId);
    public static void DeleteMail(List<string> _mailIds) => _SendPOST(DELETE_MAIL, "", new JO() { ["ids"] = _mailIds });
    public static void ClaimDeposit(string _transactionId) => _SendPOST(CLAIM_DEPOSIT, "", new JO() { ["tx"] = _transactionId });
    public static void GetWithdrawHistory()
        => _SendGET(WITHDRAW_HISTORY, "", new JO() { ["page"] = 1, ["pageSize"] = 100, ["status"] = (int)WITHDRAW_STATUS.WITHDRAWAL_STATUS_UNSPECIFIED });
    public static void Withdraw(long _amount, string _account, string _channel)
        => _SendPOST(WITHDRAW, "", new JO() { ["amount"] = _amount, ["receiver"] = _account, ["channel"] = _channel });

    private static void _SendGET(string _apiName, string _tail = "", JO _dataJO = null) => NetworkManager.INSTANCE.SendGET(_apiName, _tail, _dataJO);
    private static void _SendPOST(string _apiName, string _tail = "", JO _dataJO = null) => NetworkManager.INSTANCE.SendPOST(_apiName, _tail, _dataJO);
}
