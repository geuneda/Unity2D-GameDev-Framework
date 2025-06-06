using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

namespace Unity2DFramework.Core.Managers
{
    /// <summary>
    /// 오디오 재생과 관리를 담당하는 매니저
    /// BGM, SFX, UI 사운드를 분리하여 관리
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("오디오 믹서")]
        [SerializeField] private AudioMixerGroup masterMixerGroup;
        [SerializeField] private AudioMixerGroup bgmMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup uiMixerGroup;
        
        [Header("오디오 소스")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource[] sfxSources;
        [SerializeField] private AudioSource uiSource;
        
        [Header("볼륨 설정")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float uiVolume = 1f;
        
        // 오디오 클립 캐시
        private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
        
        // SFX 소스 풀링을 위한 인덱스
        private int currentSfxSourceIndex = 0;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 오디오 매니저 초기화
        /// </summary>
        public void Initialize()
        {
            // 오디오 소스 설정
            SetupAudioSources();
            
            // 볼륨 설정 적용
            ApplyVolumeSettings();
            
            Debug.Log("[AudioManager] 오디오 매니저 초기화 완료");
        }
        
        /// <summary>
        /// 오디오 소스들을 설정
        /// </summary>
        private void SetupAudioSources()
        {
            // BGM 소스 설정
            if (bgmSource != null)
            {
                bgmSource.outputAudioMixerGroup = bgmMixerGroup;
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }
            
            // SFX 소스들 설정
            if (sfxSources != null)
            {
                foreach (var source in sfxSources)
                {
                    if (source != null)
                    {
                        source.outputAudioMixerGroup = sfxMixerGroup;
                        source.loop = false;
                        source.playOnAwake = false;
                    }
                }
            }
            
            // UI 소스 설정
            if (uiSource != null)
            {
                uiSource.outputAudioMixerGroup = uiMixerGroup;
                uiSource.loop = false;
                uiSource.playOnAwake = false;
            }
        }
        
        /// <summary>
        /// BGM 재생
        /// </summary>
        public void PlayBGM(AudioClip clip, bool fadeIn = true)
        {
            if (bgmSource == null || clip == null) return;
            
            if (fadeIn && bgmSource.isPlaying)
            {
                StartCoroutine(FadeOutAndPlayBGM(clip));
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }
        
        /// <summary>
        /// SFX 재생
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (sfxSources == null || clip == null) return;
            
            // 사용 가능한 SFX 소스 찾기
            AudioSource availableSource = GetAvailableSfxSource();
            if (availableSource != null)
            {
                availableSource.volume = volume * sfxVolume;
                availableSource.PlayOneShot(clip);
            }
        }
        
        /// <summary>
        /// UI 사운드 재생
        /// </summary>
        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (uiSource == null || clip == null) return;
            
            uiSource.volume = volume * uiVolume;
            uiSource.PlayOneShot(clip);
        }
        
        /// <summary>
        /// 사용 가능한 SFX 소스 반환
        /// </summary>
        private AudioSource GetAvailableSfxSource()
        {
            // 현재 재생 중이지 않은 소스 찾기
            for (int i = 0; i < sfxSources.Length; i++)
            {
                if (!sfxSources[i].isPlaying)
                {
                    return sfxSources[i];
                }
            }
            
            // 모든 소스가 사용 중이면 순환하여 사용
            AudioSource source = sfxSources[currentSfxSourceIndex];
            currentSfxSourceIndex = (currentSfxSourceIndex + 1) % sfxSources.Length;
            return source;
        }
        
        /// <summary>
        /// BGM 페이드 아웃 후 새로운 BGM 재생
        /// </summary>
        private IEnumerator FadeOutAndPlayBGM(AudioClip newClip)
        {
            float startVolume = bgmSource.volume;
            
            // 페이드 아웃
            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * Time.deltaTime / 1f; // 1초 페이드
                yield return null;
            }
            
            // 새로운 BGM 설정 및 재생
            bgmSource.clip = newClip;
            bgmSource.Play();
            
            // 페이드 인
            while (bgmSource.volume < startVolume)
            {
                bgmSource.volume += startVolume * Time.deltaTime / 1f;
                yield return null;
            }
            
            bgmSource.volume = startVolume;
        }
        
        /// <summary>
        /// 볼륨 설정 적용
        /// </summary>
        private void ApplyVolumeSettings()
        {
            if (masterMixerGroup != null)
            {
                masterMixerGroup.audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
                masterMixerGroup.audioMixer.SetFloat("BGMVolume", Mathf.Log10(bgmVolume) * 20);
                masterMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
                masterMixerGroup.audioMixer.SetFloat("UIVolume", Mathf.Log10(uiVolume) * 20);
            }
        }
        
        // 볼륨 설정 메서드들
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
    }
}