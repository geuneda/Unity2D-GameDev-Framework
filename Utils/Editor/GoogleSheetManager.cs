#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unity2DFramework.Utils
{
    /// <summary>
    /// 구글 시트를 Fetch하고 파싱하는 로직을 Facade로 감싼 클래스입니다.
    /// 이 클래스는 GoogleSheetManager의 복잡한 로직을 단순화하여 외부에서 쉽게 호출할 수 있게 합니다.
    /// </summary>
    public class GoogleSheetFacade
    {
        private GoogleSheetManager manager;

        public GoogleSheetFacade(GoogleSheetManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// 모든 시트를 페치한 뒤 바로 모든 시트를 파싱하는 기능을 수행합니다.
        /// 외부에서 이 메서드를 직접 호출하여 로딩 창 등에서 자동으로 시트 파싱을 할 수 있습니다.
        /// 참고로 에디터 전용입니다.
        /// </summary>
        public void FetchAndParseAllSheets()
        {
            // 에디터 코루틴을 사용하여 페치 완료 후 파싱 로직을 수행
            EditorCoroutineUtility.StartCoroutine(FetchAndParseAllSheetsCoroutine(), manager);
        }

        /// <summary>
        /// 모든 시트를 페치하고 파싱하는 코루틴입니다.
        /// </summary>
        private IEnumerator FetchAndParseAllSheetsCoroutine()
        {
            // 시트 데이터 가져오기
            yield return manager.FetchSheetsDataCoroutine();

            // 시트 데이터를 모두 파싱하여 클래스/JSON 생성
            manager.ParseAllSheets();
        }
    }

    /// <summary>
    /// 구글 스프레드시트 데이터를 파싱하여 JSON 파일 및 데이터 클래스를 생성하는 에디터 도구
    /// </summary>
    public class GoogleSheetManager : EditorWindow
    {
        [SerializeField] private string sheetAPIurl = "";
        [SerializeField] private string sheetUrl = "";

        private List<SheetData> sheets = new List<SheetData>();
        private int selectedSheetIndex = 0;
        private bool isFetching = false;
        private GoogleSheetFacade facade;

        [MenuItem("Tools/Unity2D Framework/Google Sheet Parser")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(GoogleSheetManager));
            window.titleContent = new GUIContent("Google Sheet Parser");
            window.maxSize = new Vector2(600, 400);
            window.minSize = new Vector2(600, 400);
        }
        
        private void OnEnable()
        {
            // OnEnable에서 Facade 인스턴스화
            facade = new GoogleSheetFacade(this);
        }
        
        #region OnGUI
        private void OnGUI()
        {
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("구글 스프레드시트 데이터를 파싱하여 JSON 파일 및 데이터 클래스를 생성합니다.", MessageType.Info);
            
            GUILayout.Space(10);
            sheetAPIurl = EditorGUILayout.TextField("Sheet API URL", sheetAPIurl);
            sheetUrl = EditorGUILayout.TextField("Sheet URL", sheetUrl);
            
            GUILayout.Space(10);
            
            if (isFetching)
            {
                EditorGUILayout.LabelField("데이터를 가져오는 중...");
            }
            else
            {
                if (sheets.Count > 0)
                {
                    string[] sheetNames = sheets.Select(s => s.sheetName).ToArray();
                    selectedSheetIndex = EditorGUILayout.Popup("시트 선택", selectedSheetIndex, sheetNames);
                }
                else
                {
                    EditorGUILayout.LabelField("시트를 찾을 수 없습니다.");
                }

                GUILayout.Space(20);
                if (GUILayout.Button("시트 데이터 가져오기", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(FetchSheetsData(), this);
                }
                
                // 신규 버튼: 모든 시트를 Fetch 후 Parse까지 한 번에 처리
                GUILayout.Space(20);
                if (GUILayout.Button("모든 시트 가져오기 및 파싱 (완료 알림까지 기다리기)", GUILayout.Height(40)))
                {
                    // Facade를 사용하여 한 번에 처리
                    facade.FetchAndParseAllSheets();
                }
            }

            GUILayout.Space(30);
            if (GUILayout.Button("선택한 시트 파싱 및 클래스 생성", GUILayout.Height(40)))
            {
                if (sheets.Count > 0)
                {
                    ParseSelectedSheet();
                }
                else
                {
                    EditorUtility.DisplayDialog("오류", "먼저 시트를 가져오고 선택해주세요.", "확인");
                }
            }

            GUILayout.Space(30);
            if (GUILayout.Button("JSON에서 ScriptableObject 생성 메뉴 만들기", GUILayout.Height(40)))
            {
                if (sheets.Count > 0)
                {
                    ParseJsonToSO();
                }
                else
                {
                    EditorUtility.DisplayDialog("오류", "먼저 시트를 가져오고 선택해주세요.", "확인");
                }
            }
        }
        #endregion

        /// <summary>
        /// 외부에서 바로 모든 시트를 Fetch 후 Parse하도록 호출할 수 있는 메서드입니다.
        /// 로딩창, 초기화 과정 등에서 사용 가능.
        /// 참고로 에디터 전용입니다.
        /// </summary>
        public void FetchAndParseAllSheetsDirectly()
        {
            // Facade를 통해 코루틴 실행
            facade.FetchAndParseAllSheets();
        }

        #region FetchSheetData
        /// <summary>
        /// FetchSheetsData 버튼과 Facade에서 사용하기 위한 코루틴
        /// 시트 정보를 웹에서 가져오는 기능 수행
        /// </summary>
        public IEnumerator FetchSheetsDataCoroutine()
        {
            if (string.IsNullOrEmpty(sheetAPIurl))
            {
                EditorUtility.DisplayDialog("오류", "Sheet API URL을 입력해주세요.", "확인");
                yield break;
            }
            
            isFetching = true;
            UnityWebRequest request = UnityWebRequest.Get(sheetAPIurl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);
                ProcessSheetsData(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("데이터 가져오기 오류: " + request.error);
                EditorUtility.DisplayDialog("오류", "데이터를 가져오는 중 오류가 발생했습니다: " + request.error, "확인");
            }

            isFetching = false;
            Repaint();
        }

        /// <summary>
        /// 에디터 내에서 바로 시작하는 FetchSheetsData용 코루틴(버튼 클릭용)
        /// </summary>
        private IEnumerator FetchSheetsData()
        {
            yield return FetchSheetsDataCoroutine();
        }

        private void ProcessSheetsData(string json)
        {
            try
            {
                var sheetsData = JsonUtility.FromJson<SheetDataList>(json);
                sheets.Clear();
                
                if (sheetsData != null && sheetsData.sheetData != null)
                {
                    sheets.AddRange(sheetsData.sheetData);

                    if (sheets.Count > 0)
                    {
                        selectedSheetIndex = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("JSON 파싱 오류: " + e.Message);
                EditorUtility.DisplayDialog("오류", "JSON 파싱 중 오류가 발생했습니다: " + e.Message, "확인");
            }
        }
        #endregion
        
        #region ParseSheet
        private void ParseSelectedSheet()
        {
            var selectedSheet = sheets[selectedSheetIndex];
            string jsonFileName = RemoveSpecialCharacters(selectedSheet.sheetName);
            Debug.Log($"선택한 시트: {selectedSheet.sheetName}, 시트 ID: {selectedSheet.sheetId}");

            EditorCoroutineUtility.StartCoroutine(ParseGoogleSheet(jsonFileName, selectedSheet.sheetId.ToString()), this);
        }
        
        /// <summary>
        /// Fetch 이후 모든 시트를 자동으로 파싱하는 함수
        /// </summary>
        public void ParseAllSheets()
        {
            EditorCoroutineUtility.StartCoroutine(ParseAllSheetsCoroutine(), this);
        }

        /// <summary>
        /// 모든 시트를 순회하며 파싱하는 코루틴
        /// </summary>
        private IEnumerator ParseAllSheetsCoroutine()
        {
            // 모든 시트를 순회하며 JSON 및 클래스 생성
            for (int i = 0; i < sheets.Count; i++)
            {
                var sheet = sheets[i];
                string jsonFileName = RemoveSpecialCharacters(sheet.sheetName);
                yield return ParseGoogleSheet(jsonFileName, sheet.sheetId.ToString(), false);
            }

            // 모든 시트 처리 완료 후 알림
            EditorUtility.DisplayDialog("성공", "모든 시트 파싱 완료", "확인");
            AssetDatabase.Refresh();
        }

        private string RemoveSpecialCharacters(string sheetName)
        {
            return Regex.Replace(sheetName, @"[^a-zA-Z0-9\s]", "").Replace(" ", "_");
        }

        private IEnumerator ParseGoogleSheet(string jsonFileName, string gid, bool notice = true)
        {
            if (string.IsNullOrEmpty(sheetUrl))
            {
                EditorUtility.DisplayDialog("오류", "Sheet URL을 입력해주세요.", "확인");
                yield break;
            }
            
            string sheetExportUrl = $"{sheetUrl}/export?format=tsv&gid={gid}";

            UnityWebRequest request = UnityWebRequest.Get(sheetExportUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                EditorUtility.DisplayDialog("실패", "구글 연결 실패!", "확인");
                yield break;
            }

            string data = request.downloadHandler.text;
            List<string> rows = ParseTSVData(data);

            if (rows == null || rows.Count < 4)
            {
                Debug.LogError("파싱할 데이터가 충분하지 않습니다.");
                EditorUtility.DisplayDialog("오류", "파싱할 데이터가 충분하지 않습니다.", "확인");
                yield break;
            }

            HashSet<int> dbIgnoreColumns = GetDBIgnoreColumns(rows[0]);
            var keys = rows[1].Split('\t').ToList();
            var types = rows[2].Split('\t').ToList();

            JArray jArray = new JArray();
            for (int i = 3; i < rows.Count; i++)
            {
                var rowData = rows[i].Split('\t').ToList();

                if (rowData[0].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"행 {i + 1}이 DB_IGNORE로 인해 무시됨");
                    continue;
                }

                var rowObject = ParseRow(keys, types, rowData, dbIgnoreColumns);
                if (rowObject != null)
                {
                    jArray.Add(rowObject);
                }
            }

            SaveJsonToFile(jsonFileName, jArray);
            CreateDataClass(jsonFileName, keys, types, dbIgnoreColumns);
            CreateDataSO(jsonFileName, keys, types, dbIgnoreColumns);

            if (notice)
            {
                EditorUtility.DisplayDialog("성공", "시트를 파싱하여 JSON으로 저장했습니다!", "확인");
                AssetDatabase.Refresh();
            }
        }

        private void SaveJsonToFile(string jsonFileName, JArray jArray)
        {
            string directoryPath = Path.Combine(Application.dataPath, "Resources", "JsonFiles");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string jsonFilePath = Path.Combine(directoryPath, $"{jsonFileName}.json");
            string jsonData = "{\"datas\":" + jArray.ToString() + "}";

            File.WriteAllText(jsonFilePath, jsonData);
            Debug.Log($"JSON 저장 경로: {jsonFilePath}");
        }

        private string CreateDataSO(string fileName, List<string> keys, List<string> types, HashSet<int> dbIgnoreColumns)
        {
            string className = fileName;
            string directoryPath = Path.Combine(Application.dataPath, "Scripts/Data");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string dataClassPath = Path.Combine(directoryPath, $"{className}SO.cs");

            using (StreamWriter writer = new StreamWriter(dataClassPath))
            {
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("");
                writer.WriteLine("namespace Unity2DFramework.Data");
                writer.WriteLine("{");
                writer.WriteLine($"    [CreateAssetMenu(fileName = \"{className}\", menuName = \"Unity2D Framework/Data/{className}\")]");
                writer.WriteLine($"    public class {className}SO : ScriptableObject");
                writer.WriteLine("    {");

                for (int i = 0; i < keys.Count; i++)
                {
                    if (dbIgnoreColumns.Contains(i)) continue;

                    string fieldType = ConvertTypeToCSharp(types[i]);
                    string fieldName = keys[i];

                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        writer.WriteLine($"        public {fieldType} {fieldName};");
                    }
                }

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            Debug.Log($"SO 클래스 저장 경로: {dataClassPath}");
            AssetDatabase.Refresh();

            return $"{className}SO";
        }

        private string CreateDataClass(string fileName, List<string> keys, List<string> types, HashSet<int> dbIgnoreColumns)
        {
            string className = fileName;
            string directoryPath = Path.Combine(Application.dataPath, "Resources/DataClass");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string dataClassPath = Path.Combine(directoryPath, $"{className}Data.cs");

            using (StreamWriter writer = new StreamWriter(dataClassPath))
            {
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("");
                writer.WriteLine("namespace Unity2DFramework.Data");
                writer.WriteLine("{");
                writer.WriteLine("    [System.Serializable]");
                writer.WriteLine($"    public class {className}Data");
                writer.WriteLine("    {");

                for (int i = 0; i < keys.Count; i++)
                {
                    if (dbIgnoreColumns.Contains(i)) continue;

                    string fieldType = ConvertTypeToCSharp(types[i]);
                    string fieldName = keys[i];

                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        writer.WriteLine($"        public {fieldType} {fieldName};");
                    }
                }
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            Debug.Log($"데이터 클래스 저장 경로: {dataClassPath}");
            AssetDatabase.Refresh();

            return className;
        }
        #endregion

        #region MakeParseSOMenu
        private void ParseJsonToSO()
        {
            EditorCoroutineUtility.StartCoroutine(ParseToSO(), this);
        }

        private IEnumerator ParseToSO(bool notice = true)
        {
            CreateToolMenu();
            yield return null;

            if (notice)
            {
                EditorUtility.DisplayDialog("성공", "JSON to SO 메뉴를 성공적으로 생성했습니다!", "확인");
                AssetDatabase.Refresh();
            }
        }

        private void CreateToolMenu()
        {
            string directoryPath = Path.Combine(Application.dataPath, "Scripts/Utils");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string dataClassPath = Path.Combine(directoryPath, $"JsonToSO.cs");

            using (StreamWriter writer = new StreamWriter(dataClassPath))
            {
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("using UnityEditor;");
                writer.WriteLine("");
                writer.WriteLine("namespace Unity2DFramework.Utils");
                writer.WriteLine("{");
                writer.WriteLine("    [System.Serializable]");
                writer.WriteLine("    public class JsonToSO : MonoBehaviour");
                writer.WriteLine("    {");

                for (int i = 0; i < sheets.Count; i++)
                {
                    string sheetName = RemoveSpecialCharacters(sheets[i].sheetName);
                    writer.WriteLine($"        [MenuItem(\"Tools/Unity2D Framework/JsonToSO/Create{sheetName}SO\")]");
                    writer.WriteLine($"        static void {sheetName}DataInit()");
                    writer.WriteLine("        {");
                    writer.WriteLine($"            DynamicMenuCreator.CreateMenusFromJson<Unity2DFramework.Data.{sheetName}Data>(\"{sheetName}.json\", typeof(Unity2DFramework.Data.{sheetName}SO));");
                    writer.WriteLine("        }");
                    writer.WriteLine("");
                }
                
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            Debug.Log($"JsonToSO 클래스 저장 경로: {dataClassPath}");
            AssetDatabase.Refresh();
        }
        #endregion
        
        #region Util
        // TSV 데이터 파싱
        private List<string> ParseTSVData(string data)
        {
            return data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        // DB_IGNORE 컬럼 필터
        private HashSet<int> GetDBIgnoreColumns(string headerRow)
        {
            var dbIgnoreColumns = new HashSet<int>();
            var firstRow = headerRow.Split('\t').ToList();

            for (int i = 0; i < firstRow.Count; i++)
            {
                if (firstRow[i].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
                {
                    dbIgnoreColumns.Add(i);
                    Debug.Log($"컬럼 {i + 1}이 DB_IGNORE로 인해 무시됨");
                }
            }

            return dbIgnoreColumns;
        }

        // 행 파싱
        private JObject ParseRow(List<string> keys, List<string> types, List<string> rowData, HashSet<int> dbIgnoreColumns)
        {
            var rowObject = new JObject();

            for (int j = 0; j < keys.Count && j < rowData.Count; j++)
            {
                if (dbIgnoreColumns.Contains(j)) continue;

                string key = keys[j];
                string type = types[j];
                string value = rowData[j].Trim();

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

                rowObject[key] = ConvertValue(value, type);
            }

            return rowObject.HasValues ? rowObject : null;
        }

        private JToken ConvertValue(string value, string type)
        {
            type = type.Trim();

            if (type.StartsWith("List<"))
            {
                string innerType = type.Substring(5, type.Length - 6);
                var values = value.Split(',').Select(v => v.Trim());

                if (innerType == "int")
                    return JArray.FromObject(values.Select(v => int.TryParse(v, out int tempInt) ? tempInt : 0));
                else if (innerType == "float")
                    return JArray.FromObject(values.Select(v => float.TryParse(v, out float tempFloat) ? tempFloat : 0.0f));
                else if (innerType == "string")
                    return JArray.FromObject(values);
                else
                    return JArray.FromObject(values);
            }

            if (type == "Vector2")
            {
                if (string.IsNullOrEmpty(value)) return new JObject();
                var values = value.Split(',');
                if (values.Length != 2) return new JObject();

                return new JObject
                {
                    ["x"] = float.TryParse(values[0], out float x) ? x : 0f,
                    ["y"] = float.TryParse(values[1], out float y) ? y : 0f
                };
            }

            if (type.StartsWith("Dictionary<"))
            {
                var typeParams = type.Substring(10, type.Length - 11).Split(',');
                string keyType = typeParams[0].Trim();
                string valueType = typeParams[1].Trim();

                if (string.IsNullOrEmpty(value)) return new JObject();

                var dict = new JObject();
                var pairs = value.Split(';');

                foreach (var pair in pairs)
                {
                    if (string.IsNullOrEmpty(pair)) continue;
                    var keyValue = pair.Split(':');
                    if (keyValue.Length != 2) continue;

                    string keyString = keyValue[0].Trim();
                    string valueString = keyValue[1].Trim();

                    if (IsEnumType(keyType))
                    {
                        Type enumType = GetEnumType(keyType);
                        if (enumType != null && Enum.TryParse(enumType, keyString, true, out object enumValue))
                        {
                            int enumIntValue = (int)enumValue;
                            dict[enumIntValue.ToString()] = ConvertValue(valueString, valueType);
                        }
                    }
                    else
                    {
                        var dictKey = ConvertValue(keyString, keyType);
                        dict[dictKey.ToString()] = ConvertValue(valueString, valueType);
                    }
                }
                return dict;
            }

            switch (type)
            {
                case "int": return int.TryParse(value, out int intValue) ? intValue : 0;
                case "long": return long.TryParse(value, out long longValue) ? longValue : 0L;
                case "float": return float.TryParse(value, out float floatValue) ? floatValue : 0.0f;
                case "double": return double.TryParse(value, out double doubleValue) ? doubleValue : 0.0d;
                case "bool": return bool.TryParse(value, out bool boolValue) ? boolValue : false;
                case "byte": return byte.TryParse(value, out byte byteValue) ? byteValue : (byte)0;
                case "int[]": return JArray.FromObject(value.Split(',').Select(v => int.TryParse(v.Trim(), out int tempInt) ? tempInt : 0));
                case "float[]": return JArray.FromObject(value.Split(',').Select(v => float.TryParse(v.Trim(), out float tempFloat) ? tempFloat : 0.0f));
                case "string[]": return JArray.FromObject(value.Split(',').Select(v => v.Trim()));
                case "DateTime": return DateTime.TryParse(value, out DateTime dateTimeValue) ? dateTimeValue : DateTime.MinValue;
                case "TimeSpan": return TimeSpan.TryParse(value, out TimeSpan timeSpanValue) ? timeSpanValue : TimeSpan.Zero;
                case "Guid": return Guid.TryParse(value, out Guid guidValue) ? guidValue.ToString() : Guid.Empty.ToString();
                case "Vector2": return value; // 여기까지 오지 않게 위에서 처리
                default:
                    if (IsEnumType(type))
                    {
                        Type enumType = GetEnumType(type);
                        if (enumType != null && Enum.TryParse(enumType, value, true, out object enumValue))
                        {
                            return Convert.ToInt32(enumValue);
                        }
                        return 0;
                    }
                    return value;
            }
        }

        private string ConvertTypeToCSharp(string type)
        {
            type = type.Trim();

            if (type.StartsWith("List<"))
            {
                string innerType = type.Substring(5, type.Length - 6);
                return $"List<{ConvertTypeToCSharp(innerType)}>";
            }

            if (type.StartsWith("Dictionary<"))
            {
                var typeParams = type.Substring(10, type.Length - 11).Split(',');
                string keyType = typeParams[0].Trim();
                string valueType = typeParams[1].Trim();
                return $"Dictionary<{ConvertTypeToCSharp(keyType)}, {ConvertTypeToCSharp(valueType)}>";
            }

            switch (type)
            {
                case "int": return "int";
                case "long": return "long";
                case "float": return "float";
                case "double": return "double";
                case "bool": return "bool";
                case "byte": return "byte";
                case "int[]": return "int[]";
                case "float[]": return "float[]";
                case "string[]": return "string[]";
                case "DateTime": return "System.DateTime";
                case "TimeSpan": return "System.TimeSpan";
                case "Guid": return "System.Guid";
                case "Vector2": return "Vector2";
                default: return type;
            }
        }

        private bool IsEnumType(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null && type.IsEnum)
                    return true;
            }
            return false;
        }

        private Type GetEnumType(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null && type.IsEnum)
                    return type;
            }
            return null;
        }
        #endregion
    }

    [System.Serializable]
    public class SheetData
    {
        public string sheetName;
        public int sheetId;
    }

    [System.Serializable]
    public class SheetDataList
    {
        public SheetData[] sheetData;
    }
}
#endif 