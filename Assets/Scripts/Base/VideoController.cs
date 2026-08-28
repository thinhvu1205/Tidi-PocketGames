using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using DG.Tweening;

public class VideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer m_DataVP;
    [SerializeField] private GameObject m_BgPlay, m_BgPause;
    [SerializeField] private CanvasGroup m_ControlPanelCG;
    [SerializeField] private Slider m_TimelineS;
    [SerializeField] private TextMeshProUGUI m_TimelineTMPUGUI;
    [SerializeField] private string m_Url;
    private const float TIME_AUTO_HIDE_CONTROL_PANEL = 2f;
    private Transform _ButtonPlayPauseTf;
    private Tween _FadingControlPanelT;
    private float _AutoHideControlPanelElapsedTime;
    private bool _IsDraggingTimeline, _IsControlPanelEnabled;

    #region Button
    public void DoClickPanelControl()
    {
        if (!_IsControlPanelEnabled) _ShowFullAlphaControlPanel();
        else _FadeAlphaAndHideControlPanel();
    }
    public void DoClickPlayPause()
    {
        _ShowFullAlphaControlPanel();
        if (m_DataVP.isPlaying) _Pause();
        else _Play();
        _ButtonPlayPauseTf.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }
    public void DoClickGo5sForward()
    {
        _ShowFullAlphaControlPanel();
        float newTimeLine = Mathf.Min((float)m_DataVP.length, (float)m_DataVP.time + 5);
        m_DataVP.time = newTimeLine;
        if (newTimeLine >= m_DataVP.length) _Pause();
    }
    public void DoClickGo5sBackward()
    {
        _ShowFullAlphaControlPanel();
        float newTimeLine = Mathf.Max(0, (float)m_DataVP.time - 5);
        m_DataVP.time = newTimeLine;
        if (newTimeLine <= 0) _Play();
    }
    public void OnBeginDragSlider() => _IsDraggingTimeline = true;
    public void OnEndDragSlider()
    {
        _ShowFullAlphaControlPanel();
        _IsDraggingTimeline = false;
        m_DataVP.time = m_TimelineS.value * m_DataVP.length;
    }
    #endregion

    private void _ShowFullAlphaControlPanel()
    {
        _FadingControlPanelT?.Kill();
        _AutoHideControlPanelElapsedTime = 0;
        m_ControlPanelCG.alpha = 1;
        m_ControlPanelCG.gameObject.SetActive(true);
        _IsControlPanelEnabled = true;
    }
    private void _FadeAlphaAndHideControlPanel()
    {
        _IsControlPanelEnabled = false;
        _FadingControlPanelT = m_ControlPanelCG.DOFade(0, .2f).OnComplete(() => m_ControlPanelCG.gameObject.SetActive(false));
    }
    private void _Play()
    {
        m_DataVP.Play();
        m_BgPause.SetActive(true);
        m_BgPlay.SetActive(false);
    }
    private void _Pause()
    {
        m_DataVP.Pause();
        m_BgPause.SetActive(false);
        m_BgPlay.SetActive(true);
    }
    private void _OnPrepareComplete(VideoPlayer _aVP)
    {
        m_TimelineTMPUGUI.SetText("0:00 / " + Database.FormatTime(m_DataVP.length));
        _Pause();
    }
    private void _OnEndVideo(VideoPlayer _aVP) => _Pause();

    private void OnDestroy()
    {
        m_DataVP.prepareCompleted -= _OnPrepareComplete;
        m_DataVP.loopPointReached -= _OnEndVideo;
    }
    private void Update()
    {
        if (!m_DataVP.isPrepared || m_DataVP.length <= 0) return;
        if (!_IsDraggingTimeline)
        {
            if (_IsControlPanelEnabled)
            {
                _AutoHideControlPanelElapsedTime += Time.deltaTime;
                if (_AutoHideControlPanelElapsedTime >= TIME_AUTO_HIDE_CONTROL_PANEL) _FadeAlphaAndHideControlPanel();
            }
            m_TimelineS.SetValueWithoutNotify((float)(m_DataVP.time / m_DataVP.length));
            m_TimelineTMPUGUI.SetText(Database.FormatTime(m_DataVP.time) + " / " + Database.FormatTime(m_DataVP.length));
        }
    }
    private void Awake()
    {
        _ButtonPlayPauseTf = m_BgPause.transform.parent;
        m_BgPause.SetActive(m_DataVP.playOnAwake);
        m_BgPlay.SetActive(!m_DataVP.playOnAwake);
        if (!string.IsNullOrEmpty(m_Url))
        {
            m_DataVP.source = VideoSource.Url;
            m_DataVP.url = m_Url;
        }
        m_DataVP.prepareCompleted += _OnPrepareComplete;
        m_DataVP.errorReceived += (_vp, msg) => Debug.LogError("|   ) )=3 video error: " + msg);
        m_DataVP.loopPointReached += _OnEndVideo;
        m_TimelineS.onValueChanged.AddListener((x) =>
            {
                if (_IsDraggingTimeline) m_TimelineTMPUGUI.SetText(Database.FormatTime(x * m_DataVP.length) + " / " + Database.FormatTime(m_DataVP.length));
            });
        m_DataVP.Prepare();
    }
}