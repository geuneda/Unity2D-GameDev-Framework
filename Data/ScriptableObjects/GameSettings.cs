using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 설정을 관리하는 ScriptableObject
/// 게임의 다양한 설정값을 중앙에서 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings/GameSettings")]
public class GameSettings : ScriptableObject
{
    [System.Serializable]
    public class AudioSettings
    {
        [Range(0, 1)] public float masterVolume = 1.0f;
        [Range(0, 1)] public float musicVolume = 0.8f;
        [Range(0, 1)] public float sfxVolume = 1.0f;
        [Range(0, 1)] public float uiSoundVolume = 0.7f;
        [Range(0, 1)] public float voiceVolume = 1.0f;
        public bool muteWhenInBackground = true;
    }
    
    [System.Serializable]
    public class GraphicsSettings
    {
        public enum QualityLevel { Low, Medium, High, Ultra }
        
        public QualityLevel qualityLevel = QualityLevel.High;
        [Range(0, 1)] public float brightness = 0.5f;
        [Range(0, 1)] public float contrast = 0.5f;
        public bool fullscreen = true;
        public bool vsync = true;
        [Range(30, 144)] public int targetFrameRate = 60;
        public bool showFps = false;
    }
    
    [System.Serializable]
    public class GameplaySettings
    {
        [Range(0, 1)] public float cameraShakeIntensity = 0.5f;
        public bool showDamageNumbers = true;
        public bool showHitEffects = true;
        public bool autoAim = false;
        [Range(0, 1)] public float difficultyLevel = 0.5f;
        public bool tutorialEnabled = true;
    }
    
    [System.Serializable]
    public class InputSettings
    {
        public float joystickSensitivity = 1.0f;
        public bool invertYAxis = false;
        public float touchSensitivity = 1.0f;
        public bool autoFireEnabled = false;
    }
    
    [System.Serializable]
    public class AccessibilitySettings
    {
        public bool largeUIEnabled = false;
        public bool highContrastMode = false;
        public bool screenReaderEnabled = false;
        public bool colorblindMode = false;
        public enum ColorblindType { None, Protanopia, Deuteranopia, Tritanopia }
        public ColorblindType colorblindType = ColorblindType.None;
    }
    
    [Header("Audio")]
    public AudioSettings audioSettings = new AudioSettings();
    
    [Header("Graphics")]
    public GraphicsSettings graphicsSettings = new GraphicsSettings();
    
    [Header("Gameplay")]
    public GameplaySettings gameplaySettings = new GameplaySettings();
    
    [Header("Input")]
    public InputSettings inputSettings = new InputSettings();
    
    [Header("Accessibility")]
    public AccessibilitySettings accessibilitySettings = new AccessibilitySettings();
    
    [Header("Debug")]
    public bool debugModeEnabled = false;
    public bool godModeEnabled = false;
    public bool infiniteResources = false;
    
    private static GameSettings _instance;
    
    /// <summary>
    /// 게임 설정 인스턴스를 가져옵니다.
    /// </summary>
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameSettings>("GameSettings");
                
                if (_instance == null)
                {
                    Debug.LogWarning("GameSettings를 찾을 수 없습니다. 기본 설정을 사용합니다.");
                    _instance = CreateInstance<GameSettings>();
                }
                
                // 저장된 설정 로드
                _instance.LoadSettings();
            }
            
            return _instance;
        }
    }
    
    /// <summary>
    /// 설정을 저장합니다.
    /// </summary>
    public void SaveSettings()
    {
        // 오디오 설정 저장
        PlayerPrefs.SetFloat("MasterVolume", audioSettings.masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", audioSettings.musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", audioSettings.sfxVolume);
        PlayerPrefs.SetFloat("UiSoundVolume", audioSettings.uiSoundVolume);
        PlayerPrefs.SetFloat("VoiceVolume", audioSettings.voiceVolume);
        PlayerPrefs.SetInt("MuteWhenInBackground", audioSettings.muteWhenInBackground ? 1 : 0);
        
        // 그래픽 설정 저장
        PlayerPrefs.SetInt("QualityLevel", (int)graphicsSettings.qualityLevel);
        PlayerPrefs.SetFloat("Brightness", graphicsSettings.brightness);
        PlayerPrefs.SetFloat("Contrast", graphicsSettings.contrast);
        PlayerPrefs.SetInt("Fullscreen", graphicsSettings.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", graphicsSettings.vsync ? 1 : 0);
        PlayerPrefs.SetInt("TargetFrameRate", graphicsSettings.targetFrameRate);
        PlayerPrefs.SetInt("ShowFps", graphicsSettings.showFps ? 1 : 0);
        
        // 게임플레이 설정 저장
        PlayerPrefs.SetFloat("CameraShakeIntensity", gameplaySettings.cameraShakeIntensity);
        PlayerPrefs.SetInt("ShowDamageNumbers", gameplaySettings.showDamageNumbers ? 1 : 0);
        PlayerPrefs.SetInt("ShowHitEffects", gameplaySettings.showHitEffects ? 1 : 0);
        PlayerPrefs.SetInt("AutoAim", gameplaySettings.autoAim ? 1 : 0);
        PlayerPrefs.SetFloat("DifficultyLevel", gameplaySettings.difficultyLevel);
        PlayerPrefs.SetInt("TutorialEnabled", gameplaySettings.tutorialEnabled ? 1 : 0);
        
        // 입력 설정 저장
        PlayerPrefs.SetFloat("JoystickSensitivity", inputSettings.joystickSensitivity);
        PlayerPrefs.SetInt("InvertYAxis", inputSettings.invertYAxis ? 1 : 0);
        PlayerPrefs.SetFloat("TouchSensitivity", inputSettings.touchSensitivity);
        PlayerPrefs.SetInt("AutoFireEnabled", inputSettings.autoFireEnabled ? 1 : 0);
        
        // 접근성 설정 저장
        PlayerPrefs.SetInt("LargeUIEnabled", accessibilitySettings.largeUIEnabled ? 1 : 0);
        PlayerPrefs.SetInt("HighContrastMode", accessibilitySettings.highContrastMode ? 1 : 0);
        PlayerPrefs.SetInt("ScreenReaderEnabled", accessibilitySettings.screenReaderEnabled ? 1 : 0);
        PlayerPrefs.SetInt("ColorblindMode", accessibilitySettings.colorblindMode ? 1 : 0);
        PlayerPrefs.SetInt("ColorblindType", (int)accessibilitySettings.colorblindType);
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 저장된 설정을 로드합니다.
    /// </summary>
    public void LoadSettings()
    {
        // 오디오 설정 로드
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            audioSettings.masterVolume = PlayerPrefs.GetFloat("MasterVolume");
            audioSettings.musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            audioSettings.sfxVolume = PlayerPrefs.GetFloat("SfxVolume");
            audioSettings.uiSoundVolume = PlayerPrefs.GetFloat("UiSoundVolume");
            audioSettings.voiceVolume = PlayerPrefs.GetFloat("VoiceVolume");
            audioSettings.muteWhenInBackground = PlayerPrefs.GetInt("MuteWhenInBackground") == 1;
            
            // 그래픽 설정 로드
            graphicsSettings.qualityLevel = (GraphicsSettings.QualityLevel)PlayerPrefs.GetInt("QualityLevel");
            graphicsSettings.brightness = PlayerPrefs.GetFloat("Brightness");
            graphicsSettings.contrast = PlayerPrefs.GetFloat("Contrast");
            graphicsSettings.fullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            graphicsSettings.vsync = PlayerPrefs.GetInt("VSync") == 1;
            graphicsSettings.targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate");
            graphicsSettings.showFps = PlayerPrefs.GetInt("ShowFps") == 1;
            
            // 게임플레이 설정 로���
            gameplaySettings.cameraShakeIntensity = PlayerPrefs.GetFloat("CameraShakeIntensity");
            gameplaySettings.showDamageNumbers = PlayerPrefs.GetInt("ShowDamageNumbers") == 1;
            gameplaySettings.showHitEffects = PlayerPrefs.GetInt("ShowHitEffects") == 1;
            gameplaySettings.autoAim = PlayerPrefs.GetInt("AutoAim") == 1;
            gameplaySettings.difficultyLevel = PlayerPrefs.GetFloat("DifficultyLevel");
            gameplaySettings.tutorialEnabled = PlayerPrefs.GetInt("TutorialEnabled") == 1;
            
            // 입력 설정 로드
            inputSettings.joystickSensitivity = PlayerPrefs.GetFloat("JoystickSensitivity");
            inputSettings.invertYAxis = PlayerPrefs.GetInt("InvertYAxis") == 1;
            inputSettings.touchSensitivity = PlayerPrefs.GetFloat("TouchSensitivity");
            inputSettings.autoFireEnabled = PlayerPrefs.GetInt("AutoFireEnabled") == 1;
            
            // 접근성 설정 로드
            accessibilitySettings.largeUIEnabled = PlayerPrefs.GetInt("LargeUIEnabled") == 1;
            accessibilitySettings.highContrastMode = PlayerPrefs.GetInt("HighContrastMode") == 1;
            accessibilitySettings.screenReaderEnabled = PlayerPrefs.GetInt("ScreenReaderEnabled") == 1;
            accessibilitySettings.colorblindMode = PlayerPrefs.GetInt("ColorblindMode") == 1;
            accessibilitySettings.colorblindType = (AccessibilitySettings.ColorblindType)PlayerPrefs.GetInt("ColorblindType");
        }
        
        // 설정 적용
        ApplySettings();
    }
    
    /// <summary>
    /// 현재 설정을 게임에 적용합니다.
    /// </summary>
    public void ApplySettings()
    {
        // 그래픽 설정 적용
        QualitySettings.SetQualityLevel((int)graphicsSettings.qualityLevel);
        Screen.fullScreen = graphicsSettings.fullscreen;
        QualitySettings.vSyncCount = graphicsSettings.vsync ? 1 : 0;
        Application.targetFrameRate = graphicsSettings.targetFrameRate;
        
        // 다른 설정들을 적용하는 로직은 해당 매니저에서 처리
    }
    
    /// <summary>
    /// 설정을 기본값으로 리셋합니다.
    /// </summary>
    public void ResetToDefault()
    {
        _instance = CreateInstance<GameSettings>();
        SaveSettings();
        ApplySettings();
    }
}