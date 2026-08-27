using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer m_DataVP;
    [SerializeField] private TextMeshProUGUI m_TimelineTMPUGUI;
    [SerializeField] private string m_Url;
    private Slider _TimelineS;
    private bool _IsDraggingTimeline;

    #region Button
    public void DoClickPlayPause()
    {
        if (m_DataVP.isPlaying) m_DataVP.Pause();
        else m_DataVP.Play();
    }
    public void OnBeginDragSlider() => _IsDraggingTimeline = true;
    public void OnEndDragSlider()
    {
        _IsDraggingTimeline = false;
        m_DataVP.time = _TimelineS.value * m_DataVP.length;
    }
    #endregion

    private void Update()
    {
        if (!m_DataVP.isPrepared || m_DataVP.length <= 0) return;
        if (!_IsDraggingTimeline)
        {
            _TimelineS.SetValueWithoutNotify((float)(m_DataVP.time / m_DataVP.length));
            m_TimelineTMPUGUI.SetText(Database.FormatTime(m_DataVP.time) + " / " + Database.FormatTime(m_DataVP.length));
        }
    }
    private void Awake()
    {
        _TimelineS = GetComponentInChildren<Slider>();
        if (!string.IsNullOrEmpty(m_Url))
        {
            m_DataVP.source = VideoSource.Url;
            m_DataVP.url = m_Url;
        }
        m_DataVP.playOnAwake = false;
        m_DataVP.isLooping = false;
        m_DataVP.prepareCompleted += (aVP) =>
        {
            m_TimelineTMPUGUI.SetText("0:00 / " + Database.FormatTime(m_DataVP.length));
            m_DataVP.Play();
        };
        m_DataVP.errorReceived += (_vp, msg) => Debug.LogError("|   ) )=3 video error: " + msg);
        _TimelineS.onValueChanged.AddListener((x) =>
        {
            if (_IsDraggingTimeline)
                m_TimelineTMPUGUI.SetText(Database.FormatTime(x * m_DataVP.length) + " / " + Database.FormatTime(m_DataVP.length));
        });
        m_DataVP.Prepare();   // start buffering; picture/length ready in prepareCompleted
    }
}