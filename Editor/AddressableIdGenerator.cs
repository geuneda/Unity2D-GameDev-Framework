using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Unity2DFramework.Editor
{
    /// <summary>
    /// Addressable ID 자동 생성 에디터 도구
    /// Addressable 그룹의 에셋들을 분석하여 AddressableId.cs 파일을 자동 생성합니다.
    /// </summary>
    public class AddressableIdGenerator : EditorWindow
    {
        private const string ADDRESSABLE_ID_FILE_PATH = "Core/Assets/AddressableId.cs";
        private const string NAMESPACE = "Unity2DFramework.Core.Assets";
        
        private Vector2 scrollPosition;
        private bool includeScenes = true;
        private bool includePrefabs = true;
        private bool includeScriptableObjects = true;
        private bool includeAudioClips = true;
        private bool includeTextures = false;
        private bool includeMaterials = false;
        private string customNamespace = NAMESPACE;
        
        [MenuItem("Unity2D Framework/Tools/Generate Addressable IDs")]
        public static void ShowWindow()
        {
            var window = GetWindow<AddressableIdGenerator>("Addressable ID Generator");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Addressable ID 생성기", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox(
                "이 도구는 Addressable 그룹의 에셋들을 분석하여 " +
                "타입 안전한 AddressableId 열거형과 설정 클래스를 자동 생성합니다.",
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            // 설정 옵션
            EditorGUILayout.LabelField("생성 옵션", EditorStyles.boldLabel);
            
            customNamespace = EditorGUILayout.TextField("네임스페이스", customNamespace);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("포함할 에셋 타입", EditorStyles.boldLabel);
            includeScenes = EditorGUILayout.Toggle("씬 파일", includeScenes);
            includePrefabs = EditorGUILayout.Toggle("프리팹", includePrefabs);
            includeScriptableObjects = EditorGUILayout.Toggle("ScriptableObject", includeScriptableObjects);
            includeAudioClips = EditorGUILayout.Toggle("오디오 클립", includeAudioClips);
            includeTextures = EditorGUILayout.Toggle("텍스처", includeTextures);
            includeMaterials = EditorGUILayout.Toggle("머티리얼", includeMaterials);
            
            EditorGUILayout.Space();
            
            // Addressable 설정 상태 확인
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorGUILayout.HelpBox(
                    "Addressable 설정이 초기화되지 않았습니다. " +
                    "Window > Asset Management > Addressables > Groups에서 설정을 생성하세요.",
                    MessageType.Warning);
                return;
            }
            
            // 현재 Addressable 그룹 정보 표시
            EditorGUILayout.LabelField("현재 Addressable 그룹", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var group in settings.groups)
            {
                if (group == null || group.ReadOnly) continue;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"그룹: {group.Name}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"에셋 수: {group.entries.Count}", EditorStyles.miniLabel, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                foreach (var entry in group.entries)
                {
                    if (entry.IsFolder) continue;
                    
                    var assetType = AssetDatabase.GetMainAssetTypeAtPath(entry.AssetPath);
                    if (ShouldIncludeAssetType(assetType))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"  - {entry.address}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"({assetType.Name})", EditorStyles.miniLabel, GUILayout.Width(100));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // 생성 버튼
            if (GUILayout.Button("AddressableId.cs 생성", GUILayout.Height(30)))
            {
                GenerateAddressableIds();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Addressable 설정 열기"))
            {
                EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
            }
        }
        
        private bool ShouldIncludeAssetType(Type assetType)
        {
            if (assetType == null) return false;
            
            if (includeScenes && (assetType == typeof(SceneAsset))) return true;
            if (includePrefabs && (assetType == typeof(GameObject))) return true;
            if (includeScriptableObjects && assetType.IsSubclassOf(typeof(ScriptableObject))) return true;
            if (includeAudioClips && (assetType == typeof(AudioClip))) return true;
            if (includeTextures && (assetType == typeof(Texture2D) || assetType == typeof(Texture))) return true;
            if (includeMaterials && (assetType == typeof(Material))) return true;
            
            return false;
        }
        
        private void GenerateAddressableIds()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("오류", "Addressable 설정을 찾을 수 없습니다.", "확인");
                return;
            }
            
            try
            {
                var addressableEntries = CollectAddressableEntries(settings);
                var labels = CollectLabels(settings);
                
                string generatedCode = GenerateCode(addressableEntries, labels);
                
                string fullPath = Path.Combine(Application.dataPath, ADDRESSABLE_ID_FILE_PATH);
                string directory = Path.GetDirectoryName(fullPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(fullPath, generatedCode, Encoding.UTF8);
                
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog(
                    "성공", 
                    $"AddressableId.cs 파일이 생성되었습니다.\n경로: Assets/{ADDRESSABLE_ID_FILE_PATH}\n\n" +
                    $"생성된 ID 개수: {addressableEntries.Count}\n" +
                    $"라벨 개수: {labels.Count}", 
                    "확인");
                
                // 생성된 파일을 선택
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{ADDRESSABLE_ID_FILE_PATH}");
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("오류", $"AddressableId 생성 중 오류가 발생했습니다:\n{e.Message}", "확인");
                Debug.LogError($"AddressableIdGenerator 오류: {e}");
            }
        }
        
        private List<AddressableEntryData> CollectAddressableEntries(AddressableAssetSettings settings)
        {
            var entries = new List<AddressableEntryData>();
            
            foreach (var group in settings.groups)
            {
                if (group == null || group.ReadOnly) continue;
                
                foreach (var entry in group.entries)
                {
                    if (entry.IsFolder) continue;
                    
                    var assetType = AssetDatabase.GetMainAssetTypeAtPath(entry.AssetPath);
                    if (!ShouldIncludeAssetType(assetType)) continue;
                    
                    var entryData = new AddressableEntryData
                    {
                        address = entry.address,
                        assetPath = entry.AssetPath,
                        assetType = assetType,
                        labels = entry.labels.ToArray()
                    };
                    
                    entries.Add(entryData);
                }
            }
            
            // 주소 기준으로 정렬
            entries.Sort((a, b) => string.Compare(a.address, b.address, StringComparison.Ordinal));
            
            return entries;
        }
        
        private List<string> CollectLabels(AddressableAssetSettings settings)
        {
            var labels = new HashSet<string>();
            
            foreach (var group in settings.groups)
            {
                if (group == null || group.ReadOnly) continue;
                
                foreach (var entry in group.entries)
                {
                    foreach (var label in entry.labels)
                    {
                        labels.Add(label);
                    }
                }
            }
            
            var labelList = labels.ToList();
            labelList.Sort();
            
            return labelList;
        }
        
        private string GenerateCode(List<AddressableEntryData> entries, List<string> labels)
        {
            var sb = new StringBuilder();
            
            // 파일 헤더
            sb.AppendLine("/* AUTO GENERATED CODE - DO NOT MODIFY */");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Collections.ObjectModel;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"namespace {customNamespace}");
            sb.AppendLine("{");
            
            // AddressableId 열거형 생성
            GenerateAddressableIdEnum(sb, entries);
            sb.AppendLine();
            
            // AddressableLabel 열거형 생성
            GenerateAddressableLabelEnum(sb, labels);
            sb.AppendLine();
            
            // AddressableConfig 클래스 생성
            GenerateAddressableConfigClass(sb);
            sb.AppendLine();
            
            // AddressablePathLookup 클래스 생성
            GenerateAddressablePathLookupClass(sb, entries);
            sb.AppendLine();
            
            // AddressableConfigLookup 클래스 생성
            GenerateAddressableConfigLookupClass(sb, entries, labels);
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private void GenerateAddressableIdEnum(StringBuilder sb, List<AddressableEntryData> entries)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Addressable 에셋 ID 열거형");
            sb.AppendLine("    /// 모든 Addressable 에셋의 식별자를 정의합니다.");
            sb.AppendLine("    /// 이 파일은 에디터 도구에 의해 자동 생성됩니다.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public enum AddressableId");
            sb.AppendLine("    {");
            
            for (int i = 0; i < entries.Count; i++)
            {
                string enumName = AddressToEnumName(entries[i].address);
                sb.AppendLine($"        {enumName}{(i < entries.Count - 1 ? "," : "")}");
            }
            
            sb.AppendLine("    }");
        }
        
        private void GenerateAddressableLabelEnum(StringBuilder sb, List<string> labels)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Addressable 라벨 열거형");
            sb.AppendLine("    /// 에셋 그룹화를 위한 라벨을 정의합니다.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public enum AddressableLabel");
            sb.AppendLine("    {");
            
            for (int i = 0; i < labels.Count; i++)
            {
                string enumName = LabelToEnumName(labels[i]);
                sb.AppendLine($"        {enumName}{(i < labels.Count - 1 ? "," : "")}");
            }
            
            sb.AppendLine("    }");
        }
        
        private void GenerateAddressableConfigClass(StringBuilder sb)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Addressable 에셋 설정 정보");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    [Serializable]");
            sb.AppendLine("    public class AddressableConfig");
            sb.AppendLine("    {");
            sb.AppendLine("        public int id;");
            sb.AppendLine("        public string address;");
            sb.AppendLine("        public string assetPath;");
            sb.AppendLine("        public Type assetType;");
            sb.AppendLine("        public string[] labels;");
            sb.AppendLine();
            sb.AppendLine("        public AddressableConfig(int id, string address, string assetPath, Type assetType, string[] labels)");
            sb.AppendLine("        {");
            sb.AppendLine("            this.id = id;");
            sb.AppendLine("            this.address = address;");
            sb.AppendLine("            this.assetPath = assetPath;");
            sb.AppendLine("            this.assetType = assetType;");
            sb.AppendLine("            this.labels = labels;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }
        
        private void GenerateAddressablePathLookupClass(StringBuilder sb, List<AddressableEntryData> entries)
        {
            var pathGroups = new HashSet<string>();
            
            foreach (var entry in entries)
            {
                string[] pathParts = entry.address.Split('/');
                for (int i = 1; i <= pathParts.Length; i++)
                {
                    string path = string.Join("/", pathParts.Take(i));
                    pathGroups.Add(path);
                }
            }
            
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Addressable 경로 조회 클래스");
            sb.AppendLine("    /// 자주 사용되는 경로들을 상수로 정의");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class AddressablePathLookup");
            sb.AppendLine("    {");
            
            foreach (var path in pathGroups.OrderBy(p => p))
            {
                string fieldName = PathToFieldName(path);
                sb.AppendLine($"        public static readonly string {fieldName} = \"{path}\";");
            }
            
            sb.AppendLine("    }");
        }
        
        private void GenerateAddressableConfigLookupClass(StringBuilder sb, List<AddressableEntryData> entries, List<string> labels)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Addressable 설정 조회 클래스");
            sb.AppendLine("    /// AddressableId와 실제 주소를 매핑하고 관리합니다.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class AddressableConfigLookup");
            sb.AppendLine("    {");
            sb.AppendLine("        public static IList<AddressableConfig> Configs => _addressableConfigs;");
            sb.AppendLine("        public static IList<string> Labels => _addressableLabels;");
            sb.AppendLine();
            
            // GetConfig 메서드
            sb.AppendLine("        public static AddressableConfig GetConfig(this AddressableId addressable)");
            sb.AppendLine("        {");
            sb.AppendLine("            int index = (int)addressable;");
            sb.AppendLine("            if (index >= 0 && index < _addressableConfigs.Count)");
            sb.AppendLine("            {");
            sb.AppendLine("                return _addressableConfigs[index];");
            sb.AppendLine("            }");
            sb.AppendLine("            Debug.LogError($\"AddressableConfigLookup: 유효하지 않은 AddressableId - {addressable}\");");
            sb.AppendLine("            return null;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // GetAddress 메서드
            sb.AppendLine("        public static string GetAddress(this AddressableId addressable)");
            sb.AppendLine("        {");
            sb.AppendLine("            var config = addressable.GetConfig();");
            sb.AppendLine("            return config?.address ?? string.Empty;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // 기타 메서드들...
            sb.AppendLine("        // 추가 메서드들은 기존 AddressableId.cs 파일을 참조하세요.");
            sb.AppendLine();
            
            // 라벨 배열 생성
            GenerateLabelsArray(sb, labels);
            sb.AppendLine();
            
            // 설정 배열 생성
            GenerateConfigsArray(sb, entries);
            
            sb.AppendLine("    }");
        }
        
        private void GenerateLabelsArray(StringBuilder sb, List<string> labels)
        {
            sb.AppendLine("        private static readonly IList<string> _addressableLabels = new List<string>");
            sb.AppendLine("        {");
            
            for (int i = 0; i < labels.Count; i++)
            {
                sb.AppendLine($"            \"{labels[i]}\"{(i < labels.Count - 1 ? "," : "")}");
            }
            
            sb.AppendLine("        }.AsReadOnly();");
        }
        
        private void GenerateConfigsArray(StringBuilder sb, List<AddressableEntryData> entries)
        {
            sb.AppendLine("        private static readonly IList<AddressableConfig> _addressableConfigs = new List<AddressableConfig>");
            sb.AppendLine("        {");
            
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                string labelsArray = $"new[] {{\"{string.Join("\", \"", entry.labels)}\"}}";
                string typeName = GetTypeFullName(entry.assetType);
                
                sb.AppendLine($"            new AddressableConfig({i}, \"{entry.address}\", \"{entry.assetPath}\", typeof({typeName}), {labelsArray}){(i < entries.Count - 1 ? "," : "")}");
            }
            
            sb.AppendLine("        }.AsReadOnly();");
        }
        
        private string AddressToEnumName(string address)
        {
            return address.Replace("/", "_").Replace(" ", "_").Replace("-", "_").Replace(".", "_");
        }
        
        private string LabelToEnumName(string label)
        {
            return label.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
        }
        
        private string PathToFieldName(string path)
        {
            return path.Replace("/", "_").Replace(" ", "_").Replace("-", "_").Replace(".", "_");
        }
        
        private string GetTypeFullName(Type type)
        {
            if (type == typeof(GameObject)) return "UnityEngine.GameObject";
            if (type == typeof(SceneAsset)) return "UnityEngine.SceneManagement.Scene";
            if (type == typeof(AudioClip)) return "UnityEngine.AudioClip";
            if (type == typeof(Texture2D)) return "UnityEngine.Texture2D";
            if (type == typeof(Material)) return "UnityEngine.Material";
            if (type.IsSubclassOf(typeof(ScriptableObject))) return "UnityEngine.ScriptableObject";
            
            return type.FullName ?? type.Name;
        }
        
        private class AddressableEntryData
        {
            public string address;
            public string assetPath;
            public Type assetType;
            public string[] labels;
        }
    }
}