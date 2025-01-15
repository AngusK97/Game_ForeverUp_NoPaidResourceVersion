using System;
using Data;
using UI;
using Event;
using Level;
using Scene;
using Sound;
using Resource;
using UnityEngine;
using Resource.ResourceModules;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core
{
    public partial class GameCore : MonoBehaviour
    {
        public static UIManager UI { get; private set; }
        public static EventManager Event { get; private set; }
        public static ResourceManager Resource { get; private set; }
        public static LevelManager Level { get; private set; }
        public static SceneManager Scene { get; private set; }
        public static DataManager Data { get; private set; }
        public static SoundManager Sound { get; private set; }

        [SerializeField] private UIManager uiManager;
        [SerializeField] private EventManager eventManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SceneManager sceneManager;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private SoundManager soundManager;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitManagers();
            StartGame();
        }
        
        private void InitManagers()
        {
            UI = uiManager;
            Event = eventManager;
            Resource = resourceManager;
            Level = levelManager;
            Scene = sceneManager;
            Data = dataManager;
            Sound = soundManager;

            RegisterAllViewModules();
            RegisterAllResourceModules();
        }

        private void Update()
        {
            UI.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
            Event.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
            Level.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }
        
        private void FixedUpdate()
        {
            Level.OnFixedUpdate();
        }

        private void StartGame()
        {
            Data.Init();
            
            // InitScene -> MenuScene
            UI.ShowTransition();
            
            // 加载资源
            var mainResourceModule = Resource.GetResourceModule<MainResourceModule>(ResourceModuleName.Main);
            mainResourceModule.LoadResources(() =>
            {
                Scene.LoadSceneAsync("Assets/GameResources/Scenes/MenuScene.scene").Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        return;
                    }

                    UI.OpenUI(UILayer.First, UIName.Menu, null, _ =>
                    {
                        UI.TransitionFadeIn();
                    });
                };
            });
        }
    }
}