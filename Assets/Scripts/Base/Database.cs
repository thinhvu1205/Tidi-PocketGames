using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class Database
{
    public static Database DB
    {
        get { _MainDB ??= new(); return _MainDB; }
        private set => _MainDB = value;
    }
    private static Database _MainDB;
    private Database() { }
    public enum LOGIN_TYPE { NONE = -1, NORMAL = 0, PLAYNOW = 1, FACEBOOK = 2, FACEBOOK_INSTANT = 3, APPLE_ID = 4, REG_ACC = 5 }
    public enum WITHDRAW_STATUS
    {
        WITHDRAWAL_STATUS_UNSPECIFIED = 0,
        WITHDRAWAL_STATUS_PENDING = 1,
        WITHDRAWAL_STATUS_REJECTED = 2,
        WITHDRAWAL_STATUS_APPROVED = 3,
        WITHDRAWAL_STATUS_PROCESSING = 4,
        WITHDRAWAL_STATUS_COMPLETED = 5,
        WITHDRAWAL_STATUS_FAILED = 6,
        WITHDRAWAL_STATUS_REFUNDED = 7,
        WITHDRAWAL_STATUS_CANCELLED = 8,
        WITHDRAWAL_STATUS_UNCONFIRMED = 9
    }
    public const string BASE_URL = "https://rubyclubph.com/", USERNAME = "USERNAME", PASSWORD = "PASSWORD",
        PU_MAIL = "Popups/PopupMail", PU_ANNOUNCEMENT = "Popups/PopupAnnouncement", PU_ACCOUNT = "Popups/PopupAccount", PU_SUPPORT = "Popups/PopupSupport",
        PU_UPGRADE_ACCOUNT = "Popups/PopupUpgradeAccount", PU_VIP = "Popups/PopupVip", PU_BANNER = "Popups/PopupBanner", PU_DEPOSIT = "Popups/PopupDeposit",
        PU_WITHDRAW = "Popups/PopupWithdraw", PU_RULE_CASH_FLOW = "Popups/PopupRuleCashFlow", MAIN_SCENE = "MainScene";
    public Dictionary<int, GameInfo> GamesInfoD = new();
    public List<VipInfo> VipVIs = new();
    public List<PaymentChannel> DepositPCs = new(), WithdrawPCs = new();
    public List<HistoryWithdrawInfo> WithdrawHWIs = new();
    public LOGIN_TYPE LoginType = LOGIN_TYPE.NORMAL;
    public string PlayToken = "", Username, UserId, Currency, SocialTelegram, SocialMessenger, SupportTelegram, SupportMessenger;
    public long Asset, TotalDeposit, TotalBet, RequiredBet;
    public int VipLevel;
    public bool IsMusic, IsSound, IsOfficial;
    private readonly SemaphoreSlim _GateSS = new(6, 6);

    public async UniTask DownloadAndCacheGameIcons(GameInfo _aGI, CancellationToken _aCT = default, Action _onCompleteCb = null)
    {
        await _GateSS.WaitAsync(_aCT); // max 6 in flight
        try
        {
            using UnityWebRequest aUWR = UnityWebRequestTexture.GetTexture(_aGI.ImageUrl);
            aUWR.downloadHandler = new DownloadHandlerBuffer();
            aUWR.timeout = 30;
            await aUWR.SendWebRequest().WithCancellation(_aCT);

            if (aUWR.result == UnityWebRequest.Result.Success)
            {
                byte[] rawBytes = aUWR.downloadHandler.data;
                Texture2D resultT2D = WebP.Texture2DExt.CreateTexture2DFromWebP(rawBytes, lMipmaps: false, lLinear: false, out WebP.Error aE);
                if (aE == WebP.Error.Success)
                {
                    _aGI.IconS = Sprite.Create(resultT2D, new Rect(0, 0, resultT2D.width, resultT2D.height), new Vector2(0.5f, 0.5f), 100f);
                    _onCompleteCb?.Invoke();
                }
                else Debug.LogError("|   ) )=3 WebP decode failed | " + _aGI.ImageUrl + " | " + aE);

            }
        }
        finally { _GateSS.Release(); }
    }
    public static string FormatTime(double _seconds)
    {
        int t = Mathf.FloorToInt((float)_seconds);
        return (t / 60).ToString("00") + ":" + (t % 60).ToString("00");
    }
    public static string FormatDateTime(string _original, bool _isLineSeparation = true)
    {
        DateTime aDT = DateTimeOffset.Parse(_original, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime().DateTime;
        string time = aDT.ToString("h:mm tt", CultureInfo.InvariantCulture).ToLower(), date = aDT.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        return time + (_isLineSeparation ? "\n" : " ") + date;
    }
    public static string FormatNumber(long _number) => string.Format("{0:n0}", _number);
    public static string FormatAndShortenNumber(long _number, int _floatPartLength = 2, long _minShortenedValue = 1000, bool _isHaveSpace = true)
    {
        double absolute = Mathf.Abs(_number);
        if (absolute < Mathf.Abs(_minShortenedValue)) return FormatNumber(_number);
        string input = absolute.ToString(), floatPart = "", shortenedChar = "";
        int idNumberNextToDotFromTail = 0, intPart = 0, k = 1000, m = 1000000, b = 1000000000;
        if (absolute >= b)
        {
            shortenedChar = "B";
            intPart = (int)(absolute / b);
            idNumberNextToDotFromTail = absolute.ToString().Length - 9;
        }
        else if (absolute >= m)
        {
            shortenedChar = "M";
            intPart = (int)(absolute / m);
            idNumberNextToDotFromTail = absolute.ToString().Length - 6;
        }
        else
        {
            if (absolute < k) return FormatNumber(_number);
            shortenedChar = "K";
            intPart = (int)(absolute / k);
            idNumberNextToDotFromTail = absolute.ToString().Length - 3;
        }
        bool foundNotZero = false;
        for (int i = idNumberNextToDotFromTail + 2; i >= idNumberNextToDotFromTail; i--)
        {
            if (!foundNotZero && input[i] == '0') continue;
            floatPart = input[i] + floatPart;
            foundNotZero = true;
        }
        if (floatPart.Length > 0)
        {
            string filteredFloatPart = _floatPartLength <= 0 ? "" : floatPart[..Mathf.Min(_floatPartLength, floatPart.Length)];
            floatPart = filteredFloatPart.Length <= 0 ? "" : ("." + filteredFloatPart);
        }
        return (_number < 0 ? "-" : "") + FormatNumber(intPart) + floatPart + (_isHaveSpace ? " " : "") + shortenedChar;
    }
}
public class GameInfo
{
    public List<GameTag> DataGTs;
    public Sprite IconS;
    public string Name, ImageUrl;
    public int Id;
    public bool IsRunShowingEffect = true;
}
public class GameTag
{
    public string Name, Slug;
    public int Id, SortOrder;
}
public class VipInfo
{
    public int Level;
    public long TotalDeposit, TotalBet;
}
public class PaymentChannel
{
    public List<DepositAccount> DepositDAs = new();
    public List<long> Amounts = new();
    public string Name;
}
public class DepositAccount
{
    public string AccountName, AccountNumber;
}