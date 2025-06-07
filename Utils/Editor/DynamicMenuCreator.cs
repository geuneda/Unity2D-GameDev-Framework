#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection;

namespace Unity2DFramework.Utils
{
    /// <summary>
    /// 동적으로 에디터 메뉴를 생성하고 JSON 파일을 ScriptableObject로 변환하는 유틸리티 클래스
    /// </summary>
    public static class DynamicMenuCreator
    {
        /// <summary>
        /// JSON 파일에서 데이터를 읽어 ScriptableObject로 변환합니다.
        /// </summary>
        /// <typeparam name="T">데이터 클래스 타입</typeparam>
        /// <param name="jsonFileName">JSON 파일 이름 (확장자 포함)</param>
        /// <param name="soType">생성할 ScriptableObject 타입</param>
        public static void CreateMenusFromJson<T>(string jsonFileName, Type soType) where T : class
        {
            try
            {
                // Resources 폴더 내의 JSON 파일 경로 구성
                TextAsset jsonFile = Resources.Load<TextAsset>($"JsonFiles/{Path.GetFileNameWithoutExtension(jsonFileName)}");
                
                if (jsonFile == null)
                {
                    Debug.LogError($"JSON 파일을 찾을 수 없습니다: {jsonFileName}");
                    EditorUtility.DisplayDialog("오류", $"JSON 파일을 찾을 수 없습니다: {jsonFileName}", "확인");
                    return;
                }

                // JSON 파싱
                JObject jObject = JObject.Parse(jsonFile.text);
                JArray dataArray = (JArray)jObject["datas"];

                if (dataArray == null || !dataArray.Any())
                {
                    Debug.LogWarning($"JSON 파일에 데이터가 없습니다: {jsonFileName}");
                    EditorUtility.DisplayDialog("경고", $"JSON 파일에 데이터가 없습니다: {jsonFileName}", "확인");
                    return;
                }

                // SO 저장 폴더 생성
                string saveFolder = $"Assets/Resources/ScriptableObjects/{Path.GetFileNameWithoutExtension(jsonFileName)}";
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }

                // 각 데이터 항목을 SO로 변환
                int successCount = 0;
                int errorCount = 0;
                
                foreach (JObject dataItem in dataArray)
                {
                    try
                    {
                        // 데이터 객체로 변환
                        T dataObject = dataItem.ToObject<T>();
                        
                        // ID 또는 이름 필드를 식별자로 사용
                        string identifier = GetIdentifierFromData(dataItem);
                        
                        // SO 생성 및 저장
                        ScriptableObject so = ScriptableObject.CreateInstance(soType);
                        
                        // 데이터 속성을 SO에 복사
                        CopyProperties(dataObject, so);
                        
                        // 에셋 저장
                        string assetPath = $"{saveFolder}/{identifier}.asset";
                        AssetDatabase.CreateAsset(so, assetPath);
                        
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"데이터 변환 중 오류 발생: {ex.Message}");
                        errorCount++;
                    }
                }

                // 변경사항 저장
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // 결과 메시지 표시
                string resultMessage = $"{successCount}개의 ScriptableObject가 생성되었습니다.";
                if (errorCount > 0)
                {
                    resultMessage += $"\n{errorCount}개의 항목에서 오류가 발생했습니다.";
                }
                
                EditorUtility.DisplayDialog("완료", resultMessage, "확인");
                Debug.Log($"JSON -> SO 변환 완료: {Path.GetFileNameWithoutExtension(jsonFileName)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScriptableObject 생성 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("오류", $"ScriptableObject 생성 중 오류 발생: {ex.Message}", "확인");
            }
        }

        /// <summary>
        /// 데이터 객체에서 식별자를 추출합니다. ID, Name, Key 등의 필드를 우선적으로 찾습니다.
        /// </summary>
        private static string GetIdentifierFromData(JObject dataItem)
        {
            // 우선순위: id > ID > Id > key > Key > name > Name
            string[] possibleIdentifiers = { "id", "ID", "Id", "key", "Key", "name", "Name", "title", "Title" };
            
            foreach (string identifier in possibleIdentifiers)
            {
                if (dataItem[identifier] != null)
                {
                    return dataItem[identifier].ToString();
                }
            }
            
            // 식별자가 없는 경우 GUID 사용
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        /// <summary>
        /// 소스 객체의 속성을 대상 객체에 복사합니다.
        /// </summary>
        private static void CopyProperties(object source, object destination)
        {
            // 소스 객체의 모든 속성 가져오기
            PropertyInfo[] sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // 대상 객체의 모든 속성 가져오기
            PropertyInfo[] destinationProperties = destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // 대상 객체의 모든 필드 가져오기 (속성이 아닌 필드)
            FieldInfo[] destinationFields = destination.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            // 소스 속성을 대상 속성에 매핑
            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                // 일치하는 속성 찾기
                PropertyInfo destinationProperty = Array.Find(destinationProperties, 
                    p => p.Name == sourceProperty.Name && p.PropertyType == sourceProperty.PropertyType && p.CanWrite);
                
                // 속성이 있고 쓰기 가능하면 값 복사
                if (destinationProperty != null)
                {
                    try
                    {
                        object value = sourceProperty.GetValue(source);
                        destinationProperty.SetValue(destination, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"속성 복사 중 오류: {sourceProperty.Name} - {ex.Message}");
                    }
                    continue;
                }
                
                // 속성이 없으면 일치하는 필드 찾기
                FieldInfo destinationField = Array.Find(destinationFields, 
                    f => f.Name == sourceProperty.Name && f.FieldType == sourceProperty.PropertyType);
                
                // 필드가 있으면 값 복사
                if (destinationField != null)
                {
                    try
                    {
                        object value = sourceProperty.GetValue(source);
                        destinationField.SetValue(destination, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"필드 복사 중 오류: {sourceProperty.Name} -> {destinationField.Name} - {ex.Message}");
                    }
                }
            }
            
            // 소스 객체의 모든 필드 가져오기
            FieldInfo[] sourceFields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            // 소스 필드를 대상 필드에 매핑
            foreach (FieldInfo sourceField in sourceFields)
            {
                // 일치하는 필드 찾기
                FieldInfo destinationField = Array.Find(destinationFields, 
                    f => f.Name == sourceField.Name && f.FieldType == sourceField.FieldType);
                
                // 필드가 있으면 값 복사
                if (destinationField != null)
                {
                    try
                    {
                        object value = sourceField.GetValue(source);
                        destinationField.SetValue(destination, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"필드 복사 중 오류: {sourceField.Name} - {ex.Message}");
                    }
                    continue;
                }
                
                // 필드가 없으면 일치하는 속성 찾기
                PropertyInfo destinationProperty = Array.Find(destinationProperties, 
                    p => p.Name == sourceField.Name && p.PropertyType == sourceField.FieldType && p.CanWrite);
                
                // 속성이 있고 쓰기 가능하면 값 복사
                if (destinationProperty != null)
                {
                    try
                    {
                        object value = sourceField.GetValue(source);
                        destinationProperty.SetValue(destination, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"속성 복사 중 오류: {sourceField.Name} -> {destinationProperty.Name} - {ex.Message}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// JSON 데이터 래퍼 클래스
    /// </summary>
    [Serializable]
    public class DataWrapper<T> where T : class
    {
        public List<T> datas;
    }
}
#endif 