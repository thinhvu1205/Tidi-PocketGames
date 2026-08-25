using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MicrophoneRecorder : MonoBehaviour
{
    [SerializeField] private GameObject m_Root, m_GroupLine;
    [SerializeField] private AudioSource m_DataAS;
    [SerializeField] private TextMeshProUGUI m_RecordingTimeTMPUI;
    private byte[] _AudioBytes;
    private const int _SAMPLE_RATE = 16000;
    private AudioClip _MicAC;
    private Action _OnStartRecordingCb, _OnRecordingCb, _OnEndRecordingCb;
    private Coroutine _RecordingC;
    private string _MicDevice;
    private int _RecordingDuration = 30;
    private bool _IsRecording;

    private float _StartTime;
    #region Button
    public void DoClickClose()
    {
        ResetRecordingState();
        m_Root.SetActive(false);
    }
    #endregion
    public void test(float[] data)
    {
        _MicAC.SetData(data, 0);
        m_DataAS.clip = _MicAC;
        m_DataAS.Play();
    }
    [ContextMenu("test")]
    public void t()
    {
        byte[] returnedBytes;
        using (MemoryStream output = new())
        {
            using (GZipStream gzip = new(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(_AudioBytes, 0, _AudioBytes.Length);
            }
            returnedBytes = output.ToArray();
        }
        byte[] reversedBytes;
        using (MemoryStream input = new(returnedBytes))
        {
            using (GZipStream gzip = new(input, System.IO.Compression.CompressionMode.Decompress))
            {
                using (MemoryStream output = new())
                {
                    gzip.CopyTo(output);
                    reversedBytes = output.ToArray();
                }
            }
        }
        int totalSamples = reversedBytes.Length / 4;
        float[] samples = new float[totalSamples];
        Buffer.BlockCopy(reversedBytes, 0, samples, 0, reversedBytes.Length);
        _MicAC.SetData(samples, 0);
        m_DataAS.clip = _MicAC;
        m_DataAS.Play();
    }
    public void SetData(int duration, Action onStartCb = null, Action onRecordingCb = null, Action onEndCb = null)
    {
        if (duration <= 0) return;
        _RecordingDuration = duration;
        _OnStartRecordingCb = onStartCb;
        _OnRecordingCb = onRecordingCb;
        _OnEndRecordingCb = onEndCb;

    }
    public bool IsDeviceHasMicro() { return Microphone.devices.Length > 0; }
    public byte[] GetBytes() { return _AudioBytes; }
    public void PlayRecord(float[] samples = null)
    {
        if (samples != null) _MicAC.SetData(samples, 0);
        m_DataAS.clip = _MicAC;
        m_DataAS.Play();
    }
    private Stopwatch testSW = new Stopwatch();
    public void StartRecording()
    {
#if UNITY_ANDROID
        if (!HasMicPermission())
        {
            Permission.RequestUserPermission(Permission.Microphone);
            return;
        }
#endif
        if (!HasMicPermission())
            return;

        var devices = Microphone.devices;
        if (devices == null || devices.Length == 0)
            return;

        _MicDevice = devices[0];

        // Reset & Start Stopwatch
        testSW.Reset();
        testSW.Start();

        // Lưu thời điểm bắt đầu
        _StartTime = Time.realtimeSinceStartup;

        // Bắt đầu Coroutine
        if (_RecordingC != null)
            StopCoroutine(_RecordingC);

        _RecordingC = StartCoroutine(_Recording());
    }




    public void StopRedcording()
    {
        if (!_IsRecording) return;
        _EndRecording();
        if (_RecordingC != null) StopCoroutine(_RecordingC);
        _OnEndRecordingCb?.Invoke();
    }
    private void _EndRecording()
    {
        Microphone.End(_MicDevice);

        if (testSW.IsRunning)
            testSW.Stop();

        _MicAC = _TrimAudioClip();
        m_RecordingTimeTMPUI.gameObject.SetActive(false);
    }

    public void ResetRecordingState()
    {
#if UNITY_ANDROID
        if (!HasMicPermission())
        {
            if (_RecordingC != null) StopCoroutine(_RecordingC);
            _RecordingC = null;

            _IsRecording = false;
            _MicAC = null;
            _AudioBytes = null;
            return;
        }
#endif

        // Ngừng ghi âm
        if (Microphone.IsRecording(_MicDevice))
            Microphone.End(_MicDevice);

        if (_RecordingC != null)
            StopCoroutine(_RecordingC);
        _RecordingC = null;
        _IsRecording = false;
        _MicAC = null;
        _AudioBytes = null;
        _StartTime = 0f;

        if (m_RecordingTimeTMPUI != null)
        {
            m_RecordingTimeTMPUI.text = "00:00:00";
            m_RecordingTimeTMPUI.gameObject.SetActive(false);
        }

        if (m_DataAS != null)
        {
            m_DataAS.Stop();
            m_DataAS.clip = null;
        }

        if (testSW != null)
        {
            testSW.Reset();
        }

    }
   private AudioClip _TrimAudioClip()
    {
        float recordedLength = Time.realtimeSinceStartup - _StartTime;
        int samplesLength = (int)(_MicAC.frequency * recordedLength);
        float[] samples = new float[samplesLength];
        _MicAC.GetData(samples, 0);
        _AudioBytes = new byte[samples.Length * 4];
        Buffer.BlockCopy(samples, 0, _AudioBytes, 0, _AudioBytes.Length);
        UnityEngine.Debug.Log("xem frequence may" + _MicAC.frequency + "  " + AudioSettings.outputSampleRate);
        AudioClip trimmedAC = AudioClip.Create(_MicAC.name, samplesLength, _MicAC.channels, _SAMPLE_RATE, false);
        trimmedAC.SetData(samples, 0);
        return trimmedAC;
    }

    private IEnumerator _Recording()
    {
        _IsRecording = true;
        _MicAC = Microphone.Start(_MicDevice, false, _RecordingDuration, _SAMPLE_RATE);
        m_RecordingTimeTMPUI.gameObject.SetActive(true);
        m_RecordingTimeTMPUI.text = "00:00:00";
        _OnStartRecordingCb?.Invoke();
        int countDownTime = _RecordingDuration;
        while (countDownTime > 0)
        {

            countDownTime -= 1;
            _OnRecordingCb?.Invoke();
            int time = _RecordingDuration - countDownTime;
            if (time > 0) m_RecordingTimeTMPUI.text = string.Format("{0:00}:{1:00}:{2:00}",
                Mathf.FloorToInt(time / 60 / 60), Mathf.FloorToInt(time / 60 % 60), Mathf.FloorToInt(time % 60));
            yield return new WaitForSecondsRealtime(1f);
        }
        _EndRecording();
        if (_OnEndRecordingCb == null) DoClickClose();
        else _OnEndRecordingCb.Invoke();
        _IsRecording = false;
    }


    private void Start()
    {
#if UNITY_ANDROID
        if (!HasMicPermission())
        {
            _MicDevice = null;   // Không động vào microphone
            return;
        }
#endif

        if (IsDeviceHasMicro())
            _MicDevice = Microphone.devices[0];
    }

    private void Update()
    {
        if (!_IsRecording) return;

#if UNITY_ANDROID
        if (!HasMicPermission()) return;
#endif

        DrawLiveWaveform();
    }

    private bool HasMicPermission()
    {
#if UNITY_ANDROID
        return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
    return true;
#endif
    }

    private void DrawLiveWaveform()
    {
        if (_MicAC == null || m_GroupLine == null) return;

        int micPos = Microphone.GetPosition(_MicDevice);
        if (micPos < 1024) return;

        const int sampleLength = 1024;
        float[] samples = new float[sampleLength];

        int startPos = micPos - sampleLength;
        if (startPos < 0) return;

        _MicAC.GetData(samples, startPos);

        int barCount = m_GroupLine.transform.childCount;
        int segmentLength = sampleLength / barCount;

        for (int i = 0; i < barCount; i++)
        {
            float max = 0f;

            for (int j = 0; j < segmentLength; j++)
            {
                int idx = i * segmentLength + j;
                if (idx >= samples.Length) break;
                max = Mathf.Max(max, Mathf.Abs(samples[idx]));
            }

            float normalized = Mathf.Clamp01(max * 20f); // khuếch đại
            float scaleY = Mathf.Lerp(0.1f, 1f, normalized); // đảm bảo vạch tối thiểu

            Transform bar = m_GroupLine.transform.GetChild(i);
            if (bar != null)
            {
                bar.localScale = new Vector3(1f, scaleY, 1f);

                Image img = bar.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.green;
                }
            }
        }
    }

}