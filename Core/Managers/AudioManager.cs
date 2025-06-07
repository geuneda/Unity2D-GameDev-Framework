using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 게임의 오디오를 관리하는 싱글톤 매니저 클래스
/// 사운드 효과 및 배경 음악을 재생하고 관리합니다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region 싱글톤
    private static AudioManager _instance;
    
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                _instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion
    
    [System.Serializable]
    public class SoundGroup
    {
        public string groupName;
        public AudioMixerGroup mixerGroup;
        public float baseVolume = 1f;
    }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;
    
    [Header("Audio Groups")]
    [SerializeField] private SoundGroup _musicGroup;
    [SerializeField] private SoundGroup _sfxGroup;
    [SerializeField] private SoundGroup _uiGroup;
    [SerializeField] private SoundGroup _voiceGroup;
    
    [Header("Audio Settings")]
    [SerializeField] private int _sfxPoolSize = 10;
    [SerializeField] private bool _persistAcrossScenes = true;
    [SerializeField] private bool _fadeAudioOnSceneChange = true;
    [SerializeField] private float _fadeDuration = 1.0f;
    
    // 배경 음악용 오디오 소스
    private AudioSource _musicSource;
    
    // 효과음용 오디오 소스 풀
    private List<AudioSource> _sfxPool = new List<AudioSource>();
    
    // 사운드 캐싱
    private Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();
    
    private bool _isInitialized = false;
    
    /// <summary>
    /// 오디오 시스템을 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;
        
        if (_persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
        
        // 배경 음악용 오디오 소스 생성
        GameObject musicObj = new GameObject("Music Source");
        musicObj.transform.SetParent(transform);
        _musicSource = musicObj.AddComponent<AudioSource>();
        _musicSource.outputAudioMixerGroup = _musicGroup.mixerGroup;
        _musicSource.loop = true;
        _musicSource.volume = _musicGroup.baseVolume;
        
        // 효과음용 오디오 소스 풀 생성
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFX Source {i}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = _sfxGroup.mixerGroup;
            source.volume = _sfxGroup.baseVolume;
            _sfxPool.Add(source);
        }
        
        _isInitialized = true;
        Debug.Log("AudioManager가 초기화되었습니다.");
    }
    
    /// <summary>
    /// 배경 음악을 재생합니다.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool fade = true, float fadeDuration = 1.0f)
    {
        if (clip == null) return;
        
        if (_musicSource.clip == clip && _musicSource.isPlaying)
        {
            return;
        }
        
        if (fade && _musicSource.isPlaying)
        {
            FadeMusicAndPlay(clip, fadeDuration);
        }
        else
        {
            _musicSource.clip = clip;
            _musicSource.volume = _musicGroup.baseVolume;
            _musicSource.Play();
        }
    }
    
    /// <summary>
    /// 배경 음악을 재생합니다. (리소스 경로로 로드)
    /// </summary>
    public void PlayMusic(string clipPath, bool fade = true, float fadeDuration = 1.0f)
    {
        AudioClip clip = GetAudioClip(clipPath);
        if (clip != null)
        {
            PlayMusic(clip, fade, fadeDuration);
        }
    }
    
    /// <summary>
    /// 효과음을 재생합니다.
    /// </summary>
    public AudioSource PlaySFX(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        if (clip == null) return null;
        
        // 사용 가능한 오디오 소스를 찾음
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume * _sfxGroup.baseVolume;
            source.pitch = pitch;
            source.outputAudioMixerGroup = _sfxGroup.mixerGroup;
            source.loop = false;
            source.Play();
            return source;
        }
        
        Debug.LogWarning("사용 가능한 오디오 소스가 없습니다. 풀 크기를 늘리거나 불필요한 소리를 정리하세요.");
        return null;
    }
    
    /// <summary>
    /// 효과음을 재생합니다. (리소스 경로로 로드)
    /// </summary>
    public AudioSource PlaySFX(string clipPath, float volume = 1.0f, float pitch = 1.0f)
    {
        AudioClip clip = GetAudioClip(clipPath);
        if (clip != null)
        {
            return PlaySFX(clip, volume, pitch);
        }
        return null;
    }
    
    /// <summary>
    /// UI 효과음을 재생합니다.
    /// </summary>
    public AudioSource PlayUISound(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return null;
        
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume * _uiGroup.baseVolume;
            source.pitch = 1.0f;
            source.outputAudioMixerGroup = _uiGroup.mixerGroup;
            source.loop = false;
            source.Play();
            return source;
        }
        
        return null;
    }
    
    /// <summary>
    /// UI 효과음을 재생합니다. (리소스 경로로 로드)
    /// </summary>
    public AudioSource PlayUISound(string clipPath, float volume = 1.0f)
    {
        AudioClip clip = GetAudioClip(clipPath);
        if (clip != null)
        {
            return PlayUISound(clip, volume);
        }
        return null;
    }
    
    /// <summary>
    /// 음성을 재생합니다.
    /// </summary>
    public AudioSource PlayVoice(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return null;
        
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume * _voiceGroup.baseVolume;
            source.pitch = 1.0f;
            source.outputAudioMixerGroup = _voiceGroup.mixerGroup;
            source.loop = false;
            source.Play();
            return source;
        }
        
        return null;
    }
    
    /// <summary>
    /// 배경 음악을 정지합니다.
    /// </summary>
    public void StopMusic(bool fade = true, float fadeDuration = 1.0f)
    {
        if (!_musicSource.isPlaying) return;
        
        if (fade)
        {
            FadeOutMusic(fadeDuration);
        }
        else
        {
            _musicSource.Stop();
        }
    }
    
    /// <summary>
    /// 모든 효과음을 정지합니다.
    /// </summary>
    public void StopAllSFX()
    {
        foreach (var source in _sfxPool)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }
    
    /// <summary>
    /// 지정된 그룹의 볼륨을 설정합니다.
    /// </summary>
    public void SetGroupVolume(string groupName, float volume)
    {
        // 볼륨 값을 0.0001에서 1 사이로 제한 (로그 스케일을 위해 0은 피함)
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        
        // 로그 스케일로 변환 (-80dB ~ 0dB)
        float dbValue = Mathf.Log10(volume) * 20;
        _audioMixer.SetFloat(groupName, dbValue);
        
        // 현재 재생 중인 소스들에도 적용
        if (groupName == "MusicVolume")
        {
            _musicGroup.baseVolume = volume;
            _musicSource.volume = volume;
        }
        else if (groupName == "SFXVolume")
        {
            _sfxGroup.baseVolume = volume;
        }
        else if (groupName == "UIVolume")
        {
            _uiGroup.baseVolume = volume;
        }
        else if (groupName == "VoiceVolume")
        {
            _voiceGroup.baseVolume = volume;
        }
    }
    
    /// <summary>
    /// 모든 소리를 음소거합니다.
    /// </summary>
    public void MuteAll(bool mute)
    {
        _audioMixer.SetFloat("MasterVolume", mute ? -80f : 0f);
    }
    
    // 사용 가능한 오디오 소스를 가져옴
    private AudioSource GetAvailableAudioSource()
    {
        // 재생 중이지 않은 소스 찾기
        foreach (var source in _sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // 모든 소스가 사용 중이면 가장 오래된 소스 반환
        AudioSource oldestSource = _sfxPool[0];
        float oldestTime = float.MaxValue;
        
        foreach (var source in _sfxPool)
        {
            float remainingTime = source.clip.length - source.time;
            if (remainingTime < oldestTime)
            {
                oldestTime = remainingTime;
                oldestSource = source;
            }
        }
        
        oldestSource.Stop();
        return oldestSource;
    }
    
    // 페이드아웃 후 새 음악 재생
    private async void FadeMusicAndPlay(AudioClip newClip, float duration)
    {
        // 현재 음악 페이드아웃
        float startVolume = _musicSource.volume;
        float timer = 0;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            await Task.Yield();
        }
        
        // 새 음악으로 변경 후 페이드인
        _musicSource.Stop();
        _musicSource.clip = newClip;
        _musicSource.volume = 0;
        _musicSource.Play();
        
        timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0, _musicGroup.baseVolume, timer / duration);
            await Task.Yield();
        }
        
        _musicSource.volume = _musicGroup.baseVolume;
    }
    
    // 음악 페이드아웃
    private async void FadeOutMusic(float duration)
    {
        float startVolume = _musicSource.volume;
        float timer = 0;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            await Task.Yield();
        }
        
        _musicSource.Stop();
        _musicSource.volume = _musicGroup.baseVolume;
    }
    
    // 오디오 클립 가져오기 (캐싱)
    private AudioClip GetAudioClip(string path)
    {
        // 캐시에 있으면 반환
        if (_audioCache.TryGetValue(path, out AudioClip cachedClip))
        {
            return cachedClip;
        }
        
        // 리소스에서 로드
        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debug.LogError($"오디오 클립을 찾을 수 없습니다: {path}");
            return null;
        }
        
        // 캐시에 저장
        _audioCache[path] = clip;
        return clip;
    }
    
    // 테스트용 메서드
    public void PlayAllTestSounds()
    {
        Debug.Log("테스트 사운드 재생 중...");
    }
}