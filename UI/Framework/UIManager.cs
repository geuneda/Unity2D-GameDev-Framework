using UnityEngine;
using System.Collections.Generic;
using Unity2DFramework.Animation.Tweening;

namespace Unity2DFramework.UI.Framework
{
    /// <summary>
    /// UI 시스템의 전반적인 관리를 담당하는 매니저
    /// 패널 관리, 네비게이션, 애니메이션 등을 통합 관리
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("UI 설정")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Transform panelContainer;
        
        // 등록된 UI 패널들
        private Dictionary<string, UIPanel> registeredPanels = new Dictionary<string, UIPanel>();
        private Stack<UIPanel> panelHistory = new Stack<UIPanel>();
        private UIPanel currentPanel;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// UI 시스템 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 씬의 모든 UI 패널 자동 등록
            UIPanel[] panels = FindObjectsOfType<UIPanel>(true);
            foreach (var panel in panels)
            {
                RegisterPanel(panel.PanelName, panel);
            }
            
            Debug.Log($"[UIManager] {registeredPanels.Count}개의 UI 패널 등록 완료");
        }
        
        /// <summary>
        /// UI 패널 등록
        /// </summary>
        public void RegisterPanel(string panelName, UIPanel panel)
        {
            if (registeredPanels.ContainsKey(panelName))
            {
                Debug.LogWarning($"[UIManager] 이미 등록된 패널: {panelName}");
                return;
            }
            
            registeredPanels[panelName] = panel;
            panel.Initialize();
        }
        
        /// <summary>
        /// UI 패널 표시
        /// </summary>
        public void ShowPanel(string panelName, bool addToHistory = true)
        {
            if (!registeredPanels.ContainsKey(panelName))
            {
                Debug.LogError($"[UIManager] 등록되지 않은 패널: {panelName}");
                return;
            }
            
            UIPanel panel = registeredPanels[panelName];
            
            // 현재 패널을 히스토리에 추가
            if (currentPanel != null && addToHistory)
            {
                panelHistory.Push(currentPanel);
                currentPanel.Hide();
            }
            
            // 새 패널 표시
            currentPanel = panel;
            panel.Show();
        }
        
        /// <summary>
        /// 현재 패널 숨기기
        /// </summary>
        public void HideCurrentPanel()
        {
            if (currentPanel != null)
            {
                currentPanel.Hide();
                currentPanel = null;
            }
        }
        
        /// <summary>
        /// 이전 패널로 돌아가기
        /// </summary>
        public void GoBack()
        {
            if (panelHistory.Count > 0)
            {
                if (currentPanel != null)
                {
                    currentPanel.Hide();
                }
                
                currentPanel = panelHistory.Pop();
                currentPanel.Show();
            }
        }
        
        /// <summary>
        /// 모든 패널 숨기기
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panel in registeredPanels.Values)
            {
                panel.Hide();
            }
            
            currentPanel = null;
            panelHistory.Clear();
        }
        
        /// <summary>
        /// 패널이 표시 중인지 확인
        /// </summary>
        public bool IsPanelVisible(string panelName)
        {
            if (registeredPanels.ContainsKey(panelName))
            {
                return registeredPanels[panelName].IsVisible;
            }
            return false;
        }
        
        // 프로퍼티
        public UIPanel CurrentPanel => currentPanel;
        public Canvas MainCanvas => mainCanvas;
    }
}