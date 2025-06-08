using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Unity2DFramework.Core.Assets
{
    /// <summary>
    /// Addressable 에셋 ID 열거형
    /// 모든 Addressable 에셋의 식별자를 정의합니다.
    /// 이 파일은 에디터 도구에 의해 자동 생성됩니다.
    /// </summary>
    public enum AddressableId
    {
        // UI 프리팹
        UI_MainMenu,
        UI_GameHUD,
        UI_PauseMenu,
        UI_SettingsMenu,
        UI_LoadingScreen,
        UI_GameOverScreen,
        UI_VictoryScreen,
        
        // 게임플레이 프리팹
        Player_Character,
        Enemy_Basic,
        Enemy_Boss,
        Weapon_Sword,
        Weapon_Bow,
        Item_HealthPotion,
        Item_ManaPotion,
        
        // 이펙트 프리팹
        Effect_Explosion,
        Effect_Heal,
        Effect_Damage,
        Effect_Pickup,
        
        // 오디오 클립
        Audio_BGM_MainTheme,
        Audio_BGM_Battle,
        Audio_SFX_Jump,
        Audio_SFX_Attack,
        Audio_SFX_Pickup,
        
        // 씬
        Scene_MainMenu,
        Scene_GameLevel1,
        Scene_GameLevel2,
        Scene_BossLevel,
        
        // 설정 파일
        Config_GameSettings,
        Config_AudioSettings,
        Config_InputSettings,
        Config_UISettings
    }

    /// <summary>
    /// Addressable 라벨 열거형
    /// 에셋 그룹화를 위한 라벨을 정의합니다.
    /// </summary>
    public enum AddressableLabel
    {
        UI,
        Gameplay,
        Audio,
        Effects,
        Scenes,
        Configs,
        Essential,
        Optional
    }

    /// <summary>
    /// Addressable 에셋 설정 정보
    /// </summary>
    [Serializable]
    public class AddressableConfig
    {
        public int id;
        public string address;
        public string assetPath;
        public Type assetType;
        public string[] labels;
        
        public AddressableConfig(int id, string address, string assetPath, Type assetType, string[] labels)
        {
            this.id = id;
            this.address = address;
            this.assetPath = assetPath;
            this.assetType = assetType;
            this.labels = labels;
        }
    }

    /// <summary>
    /// Addressable 경로 조회 클래스
    /// 자주 사용되는 경로들을 상수로 정의
    /// </summary>
    public static class AddressablePathLookup
    {
        public static readonly string UI = "UI";
        public static readonly string Gameplay = "Gameplay";
        public static readonly string Audio = "Audio";
        public static readonly string Effects = "Effects";
        public static readonly string Scenes = "Scenes";
        public static readonly string Configs = "Configs";
        
        // 세부 경로
        public static readonly string UI_Menus = "UI/Menus";
        public static readonly string UI_HUD = "UI/HUD";
        public static readonly string UI_Popups = "UI/Popups";
        
        public static readonly string Gameplay_Characters = "Gameplay/Characters";
        public static readonly string Gameplay_Enemies = "Gameplay/Enemies";
        public static readonly string Gameplay_Weapons = "Gameplay/Weapons";
        public static readonly string Gameplay_Items = "Gameplay/Items";
        
        public static readonly string Audio_BGM = "Audio/BGM";
        public static readonly string Audio_SFX = "Audio/SFX";
        public static readonly string Audio_UI = "Audio/UI";
    }

    /// <summary>
    /// Addressable 설정 조회 클래스
    /// AddressableId와 실제 주소를 매핑하고 관리합니다.
    /// </summary>
    public static class AddressableConfigLookup
    {
        /// <summary>
        /// 모든 Addressable 설정 목록
        /// </summary>
        public static IList<AddressableConfig> Configs => _addressableConfigs;
        
        /// <summary>
        /// 모든 라벨 목록
        /// </summary>
        public static IList<string> Labels => _addressableLabels;

        /// <summary>
        /// AddressableId로 설정 정보를 가져옵니다.
        /// </summary>
        /// <param name="addressable">Addressable ID</param>
        /// <returns>설정 정보</returns>
        public static AddressableConfig GetConfig(this AddressableId addressable)
        {
            int index = (int)addressable;
            if (index >= 0 && index < _addressableConfigs.Count)
            {
                return _addressableConfigs[index];
            }
            
            Debug.LogError($"AddressableConfigLookup: 유효하지 않은 AddressableId - {addressable}");
            return null;
        }

        /// <summary>
        /// AddressableId로 주소를 가져옵니다.
        /// </summary>
        /// <param name="addressable">Addressable ID</param>
        /// <returns>에셋 주소</returns>
        public static string GetAddress(this AddressableId addressable)
        {
            var config = addressable.GetConfig();
            return config?.address ?? string.Empty;
        }

        /// <summary>
        /// 라벨로 설정 목록을 가져옵니다.
        /// </summary>
        /// <param name="label">라벨</param>
        /// <returns>해당 라벨의 설정 목록</returns>
        public static IList<AddressableConfig> GetConfigs(this AddressableLabel label)
        {
            string labelString = label.ToLabelString();
            return GetConfigs(labelString);
        }

        /// <summary>
        /// 라벨 문자열로 설정 목록을 가져옵니다.
        /// </summary>
        /// <param name="label">라벨 문자열</param>
        /// <returns>해당 라벨의 설정 목록</returns>
        public static IList<AddressableConfig> GetConfigs(string label)
        {
            if (_addressableLabelMap.TryGetValue(label, out var configs))
            {
                return configs;
            }
            
            Debug.LogWarning($"AddressableConfigLookup: 라벨을 찾을 수 없습니다 - {label}");
            return new List<AddressableConfig>().AsReadOnly();
        }

        /// <summary>
        /// AddressableLabel을 문자열로 변환합니다.
        /// </summary>
        /// <param name="label">라벨</param>
        /// <returns>라벨 문자열</returns>
        public static string ToLabelString(this AddressableLabel label)
        {
            int index = (int)label;
            if (index >= 0 && index < _addressableLabels.Count)
            {
                return _addressableLabels[index];
            }
            
            Debug.LogError($"AddressableConfigLookup: 유효하지 않은 AddressableLabel - {label}");
            return string.Empty;
        }

        /// <summary>
        /// 특정 타입의 에셋 설정들을 가져옵니다.
        /// </summary>
        /// <typeparam name="T">에셋 타입</typeparam>
        /// <returns>해당 타입의 설정 목록</returns>
        public static IList<AddressableConfig> GetConfigsByType<T>() where T : UnityEngine.Object
        {
            var result = new List<AddressableConfig>();
            var targetType = typeof(T);
            
            foreach (var config in _addressableConfigs)
            {
                if (config.assetType == targetType || config.assetType.IsSubclassOf(targetType))
                {
                    result.Add(config);
                }
            }
            
            return result.AsReadOnly();
        }

        /// <summary>
        /// 주소로 AddressableId를 찾습니다.
        /// </summary>
        /// <param name="address">에셋 주소</param>
        /// <returns>AddressableId (찾지 못하면 첫 번째 값)</returns>
        public static AddressableId FindIdByAddress(string address)
        {
            for (int i = 0; i < _addressableConfigs.Count; i++)
            {
                if (_addressableConfigs[i].address == address)
                {
                    return (AddressableId)i;
                }
            }
            
            Debug.LogWarning($"AddressableConfigLookup: 주소에 해당하는 ID를 찾을 수 없습니다 - {address}");
            return (AddressableId)0;
        }

        // 라벨 목록 (실제 프로젝트에서는 에디터 도구로 자동 생성)
        private static readonly IList<string> _addressableLabels = new List<string>
        {
            "UI",
            "Gameplay", 
            "Audio",
            "Effects",
            "Scenes",
            "Configs",
            "Essential",
            "Optional"
        }.AsReadOnly();

        // 라벨별 설정 매핑 (실제 프로젝트에서는 에디터 도구로 자동 생성)
        private static readonly IReadOnlyDictionary<string, IList<AddressableConfig>> _addressableLabelMap = 
            new ReadOnlyDictionary<string, IList<AddressableConfig>>(new Dictionary<string, IList<AddressableConfig>>
            {
                {"UI", new List<AddressableConfig>
                {
                    new AddressableConfig(0, "UI/MainMenu", "Assets/Addressables/UI/MainMenu.prefab", typeof(GameObject), new[] {"UI", "Essential"}),
                    new AddressableConfig(1, "UI/GameHUD", "Assets/Addressables/UI/GameHUD.prefab", typeof(GameObject), new[] {"UI", "Essential"}),
                    new AddressableConfig(2, "UI/PauseMenu", "Assets/Addressables/UI/PauseMenu.prefab", typeof(GameObject), new[] {"UI"}),
                    new AddressableConfig(3, "UI/SettingsMenu", "Assets/Addressables/UI/SettingsMenu.prefab", typeof(GameObject), new[] {"UI"}),
                    new AddressableConfig(4, "UI/LoadingScreen", "Assets/Addressables/UI/LoadingScreen.prefab", typeof(GameObject), new[] {"UI", "Essential"}),
                    new AddressableConfig(5, "UI/GameOverScreen", "Assets/Addressables/UI/GameOverScreen.prefab", typeof(GameObject), new[] {"UI"}),
                    new AddressableConfig(6, "UI/VictoryScreen", "Assets/Addressables/UI/VictoryScreen.prefab", typeof(GameObject), new[] {"UI"})
                }.AsReadOnly()},
                
                {"Gameplay", new List<AddressableConfig>
                {
                    new AddressableConfig(7, "Gameplay/Player", "Assets/Addressables/Gameplay/Player.prefab", typeof(GameObject), new[] {"Gameplay", "Essential"}),
                    new AddressableConfig(8, "Gameplay/Enemy_Basic", "Assets/Addressables/Gameplay/Enemy_Basic.prefab", typeof(GameObject), new[] {"Gameplay"}),
                    new AddressableConfig(9, "Gameplay/Enemy_Boss", "Assets/Addressables/Gameplay/Enemy_Boss.prefab", typeof(GameObject), new[] {"Gameplay"}),
                    new AddressableConfig(10, "Gameplay/Weapon_Sword", "Assets/Addressables/Gameplay/Weapon_Sword.prefab", typeof(GameObject), new[] {"Gameplay"}),
                    new AddressableConfig(11, "Gameplay/Weapon_Bow", "Assets/Addressables/Gameplay/Weapon_Bow.prefab", typeof(GameObject), new[] {"Gameplay"}),
                    new AddressableConfig(12, "Gameplay/Item_HealthPotion", "Assets/Addressables/Gameplay/Item_HealthPotion.prefab", typeof(GameObject), new[] {"Gameplay"}),
                    new AddressableConfig(13, "Gameplay/Item_ManaPotion", "Assets/Addressables/Gameplay/Item_ManaPotion.prefab", typeof(GameObject), new[] {"Gameplay"})
                }.AsReadOnly()},
                
                {"Effects", new List<AddressableConfig>
                {
                    new AddressableConfig(14, "Effects/Explosion", "Assets/Addressables/Effects/Explosion.prefab", typeof(GameObject), new[] {"Effects"}),
                    new AddressableConfig(15, "Effects/Heal", "Assets/Addressables/Effects/Heal.prefab", typeof(GameObject), new[] {"Effects"}),
                    new AddressableConfig(16, "Effects/Damage", "Assets/Addressables/Effects/Damage.prefab", typeof(GameObject), new[] {"Effects"}),
                    new AddressableConfig(17, "Effects/Pickup", "Assets/Addressables/Effects/Pickup.prefab", typeof(GameObject), new[] {"Effects"})
                }.AsReadOnly()},
                
                {"Audio", new List<AddressableConfig>
                {
                    new AddressableConfig(18, "Audio/BGM_MainTheme", "Assets/Addressables/Audio/BGM_MainTheme.mp3", typeof(AudioClip), new[] {"Audio", "Essential"}),
                    new AddressableConfig(19, "Audio/BGM_Battle", "Assets/Addressables/Audio/BGM_Battle.mp3", typeof(AudioClip), new[] {"Audio"}),
                    new AddressableConfig(20, "Audio/SFX_Jump", "Assets/Addressables/Audio/SFX_Jump.wav", typeof(AudioClip), new[] {"Audio"}),
                    new AddressableConfig(21, "Audio/SFX_Attack", "Assets/Addressables/Audio/SFX_Attack.wav", typeof(AudioClip), new[] {"Audio"}),
                    new AddressableConfig(22, "Audio/SFX_Pickup", "Assets/Addressables/Audio/SFX_Pickup.wav", typeof(AudioClip), new[] {"Audio"})
                }.AsReadOnly()},
                
                {"Scenes", new List<AddressableConfig>
                {
                    new AddressableConfig(23, "Scenes/MainMenu", "Assets/Addressables/Scenes/MainMenu.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes", "Essential"}),
                    new AddressableConfig(24, "Scenes/GameLevel1", "Assets/Addressables/Scenes/GameLevel1.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes"}),
                    new AddressableConfig(25, "Scenes/GameLevel2", "Assets/Addressables/Scenes/GameLevel2.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes"}),
                    new AddressableConfig(26, "Scenes/BossLevel", "Assets/Addressables/Scenes/BossLevel.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes"})
                }.AsReadOnly()},
                
                {"Configs", new List<AddressableConfig>
                {
                    new AddressableConfig(27, "Configs/GameSettings", "Assets/Addressables/Configs/GameSettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"}),
                    new AddressableConfig(28, "Configs/AudioSettings", "Assets/Addressables/Configs/AudioSettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"}),
                    new AddressableConfig(29, "Configs/InputSettings", "Assets/Addressables/Configs/InputSettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"}),
                    new AddressableConfig(30, "Configs/UISettings", "Assets/Addressables/Configs/UISettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"})
                }.AsReadOnly()}
            });

        // 전체 설정 목록 (실제 프로젝트에서는 에디터 도구로 자동 생성)
        private static readonly IList<AddressableConfig> _addressableConfigs = new List<AddressableConfig>
        {
            // UI (0-6)
            new AddressableConfig(0, "UI/MainMenu", "Assets/Addressables/UI/MainMenu.prefab", typeof(GameObject), new[] {"UI", "Essential"}),
            new AddressableConfig(1, "UI/GameHUD", "Assets/Addressables/UI/GameHUD.prefab", typeof(GameObject), new[] {"UI", "Essential"}),
            new AddressableConfig(2, "UI/PauseMenu", "Assets/Addressables/UI/PauseMenu.prefab", typeof(GameObject), new[] {"UI"}),
            new AddressableConfig(3, "UI/SettingsMenu", "Assets/Addressables/UI/SettingsMenu.prefab", typeof(GameObject), new[] {"UI"}),
            new AddressableConfig(4, "UI/LoadingScreen", "Assets/Addressables/UI/LoadingScreen.prefab", typeof(GameObject), new[] {"UI", "Essential"}),
            new AddressableConfig(5, "UI/GameOverScreen", "Assets/Addressables/UI/GameOverScreen.prefab", typeof(GameObject), new[] {"UI"}),
            new AddressableConfig(6, "UI/VictoryScreen", "Assets/Addressables/UI/VictoryScreen.prefab", typeof(GameObject), new[] {"UI"}),
            
            // Gameplay (7-13)
            new AddressableConfig(7, "Gameplay/Player", "Assets/Addressables/Gameplay/Player.prefab", typeof(GameObject), new[] {"Gameplay", "Essential"}),
            new AddressableConfig(8, "Gameplay/Enemy_Basic", "Assets/Addressables/Gameplay/Enemy_Basic.prefab", typeof(GameObject), new[] {"Gameplay"}),
            new AddressableConfig(9, "Gameplay/Enemy_Boss", "Assets/Addressables/Gameplay/Enemy_Boss.prefab", typeof(GameObject), new[] {"Gameplay"}),
            new AddressableConfig(10, "Gameplay/Weapon_Sword", "Assets/Addressables/Gameplay/Weapon_Sword.prefab", typeof(GameObject), new[] {"Gameplay"}),
            new AddressableConfig(11, "Gameplay/Weapon_Bow", "Assets/Addressables/Gameplay/Weapon_Bow.prefab", typeof(GameObject), new[] {"Gameplay"}),
            new AddressableConfig(12, "Gameplay/Item_HealthPotion", "Assets/Addressables/Gameplay/Item_HealthPotion.prefab", typeof(GameObject), new[] {"Gameplay"}),
            new AddressableConfig(13, "Gameplay/Item_ManaPotion", "Assets/Addressables/Gameplay/Item_ManaPotion.prefab", typeof(GameObject), new[] {"Gameplay"}),
            
            // Effects (14-17)
            new AddressableConfig(14, "Effects/Explosion", "Assets/Addressables/Effects/Explosion.prefab", typeof(GameObject), new[] {"Effects"}),
            new AddressableConfig(15, "Effects/Heal", "Assets/Addressables/Effects/Heal.prefab", typeof(GameObject), new[] {"Effects"}),
            new AddressableConfig(16, "Effects/Damage", "Assets/Addressables/Effects/Damage.prefab", typeof(GameObject), new[] {"Effects"}),
            new AddressableConfig(17, "Effects/Pickup", "Assets/Addressables/Effects/Pickup.prefab", typeof(GameObject), new[] {"Effects"}),
            
            // Audio (18-22)
            new AddressableConfig(18, "Audio/BGM_MainTheme", "Assets/Addressables/Audio/BGM_MainTheme.mp3", typeof(AudioClip), new[] {"Audio", "Essential"}),
            new AddressableConfig(19, "Audio/BGM_Battle", "Assets/Addressables/Audio/BGM_Battle.mp3", typeof(AudioClip), new[] {"Audio"}),
            new AddressableConfig(20, "Audio/SFX_Jump", "Assets/Addressables/Audio/SFX_Jump.wav", typeof(AudioClip), new[] {"Audio"}),
            new AddressableConfig(21, "Audio/SFX_Attack", "Assets/Addressables/Audio/SFX_Attack.wav", typeof(AudioClip), new[] {"Audio"}),
            new AddressableConfig(22, "Audio/SFX_Pickup", "Assets/Addressables/Audio/SFX_Pickup.wav", typeof(AudioClip), new[] {"Audio"}),
            
            // Scenes (23-26)
            new AddressableConfig(23, "Scenes/MainMenu", "Assets/Addressables/Scenes/MainMenu.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes", "Essential"}),
            new AddressableConfig(24, "Scenes/GameLevel1", "Assets/Addressables/Scenes/GameLevel1.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes"}),
            new AddressableConfig(25, "Scenes/GameLevel2", "Assets/Addressables/Scenes/GameLevel2.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes"}),
            new AddressableConfig(26, "Scenes/BossLevel", "Assets/Addressables/Scenes/BossLevel.unity", typeof(UnityEngine.SceneManagement.Scene), new[] {"Scenes"}),
            
            // Configs (27-30)
            new AddressableConfig(27, "Configs/GameSettings", "Assets/Addressables/Configs/GameSettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"}),
            new AddressableConfig(28, "Configs/AudioSettings", "Assets/Addressables/Configs/AudioSettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"}),
            new AddressableConfig(29, "Configs/InputSettings", "Assets/Addressables/Configs/InputSettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"}),
            new AddressableConfig(30, "Configs/UISettings", "Assets/Addressables/Configs/UISettings.asset", typeof(ScriptableObject), new[] {"Configs", "Essential"})
        }.AsReadOnly();
    }
}