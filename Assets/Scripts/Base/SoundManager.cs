using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager INSTANCE;
    [SerializeField] private AudioSource m_BgMusicAS, m_EffectAS;
    [SerializeField] private AudioClip m_BgMusicAC, m_ClickAC;

    public void PlayMusic()
    {
        if (Database.DB.IsMusic)
        {
            if (m_BgMusicAS.clip == m_BgMusicAC && !m_BgMusicAS.isPlaying)
            {
                m_BgMusicAS.volume = 0.5f;
                m_BgMusicAS.clip = m_BgMusicAC;
                m_BgMusicAS.Play();
            }
            else if (m_BgMusicAS.clip != m_BgMusicAC)
            {
                m_BgMusicAS.Stop();
                m_BgMusicAS.clip = m_BgMusicAC;
                m_BgMusicAS.volume = 0.5f;
                m_BgMusicAS.Play();
            }
        }
        else m_BgMusicAS.Stop();
    }
    private void _PlaySound(AudioClip audioClip)
    {
        if (!Database.DB.IsSound) return;
        m_EffectAS.clip = audioClip;
        m_EffectAS.Play();
    }

    public void SoundClick() => _PlaySound(m_ClickAC);

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
