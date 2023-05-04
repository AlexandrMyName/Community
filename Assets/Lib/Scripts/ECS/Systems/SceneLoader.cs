using FlutterUnityIntegration;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client
{
    public class SceneLoader : MonoBehaviour, IEcsRunSystem, IEcsInitSystem
    {
        protected EcsWorld world;
        protected EcsPool<CamerasSettings> camerasSettingsPool;
        protected EcsPool<SceneSettings> settingsPool;
        [SerializeField] string currentSceneName;
        private SceneSettings? processingSettings;
        private void Start()
        {
            DontDestroyOnLoad(this);
        }
        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            settingsPool = world.GetPool<SceneSettings>();
            camerasSettingsPool = world.GetPool<CamerasSettings>();
            continueSetSettings();
            SceneWasLoaded();
        }

        public void Run(IEcsSystems systems)
        {
            var filter = world.Filter<SceneSettings>().End();
            foreach (int entity in filter)
            {
                var settings = settingsPool.Get(entity);
                setSettings(settings);
            }
        }
        void setSettings(SceneSettings settings)
        {
            Debug.Log($"if ([settings]{settings.sceneName} == [current]{currentSceneName} || [settings]{settings.sceneName} == {"\"\""})");
            if (settings.sceneName == currentSceneName || settings.sceneName == "")
            {
                processingSettings = settings;
                continueSetSettings();
            }
            else {
                currentSceneName = settings.sceneName;
                processingSettings = settings;
                LoadScene(settings.sceneName);
            }
        }
        void continueSetSettings()
        {
            if (!processingSettings.HasValue) return;
            Debug.Log($"continueSetSettings()");

            var entity = world.NewEntity();
            camerasSettingsPool.Add(entity).Copy(processingSettings.Value.cameras);

            processingSettings = null;
        }
        private void OnValidate()
        {
            currentSceneName = SceneManager.GetActiveScene().name;
        }
        private void Reset() => OnValidate();

        public static void LoadScene(string scene)
        {
            CallbackLoader cl = new CallbackLoader(CallbackLoader.LoaderStatus.loading);
            SceneManager.LoadScene(scene);
            //SceneManager.LoadScene(scene, LoadSceneMode.Single);
            Debug.Log($"LoadScene({scene}) -> {cl.toJson()}");
            UnityMessageManager.Instance.SendMessageToFlutter(cl.toJson());
        }
        public static void SceneWasLoaded()
        {
            CallbackLoader cl = new CallbackLoader(CallbackLoader.LoaderStatus.loaded);
            Debug.Log($"SceneWasLoaded() -> {cl.toJson()}");
            UnityMessageManager.Instance.SendMessageToFlutter(cl.toJson());
        }

        public static void SwitchNative()
        {
            CallbackLoader cl = new CallbackLoader(CallbackLoader.LoaderStatus.showWindow);
            UnityMessageManager.Instance.ShowHostMainWindow();
            Debug.Log($"SwitchNative() -> {cl.toJson()}");
            UnityMessageManager.Instance.SendMessageToFlutter(cl.toJson());
        }

        public static void UnloadNative()
        {
            CallbackLoader cl = new CallbackLoader(CallbackLoader.LoaderStatus.unloadWindow);
            UnityMessageManager.Instance.UnloadMainWindow();
            Debug.Log($"UnloadNative() -> {cl.toJson()}");
            UnityMessageManager.Instance.SendMessageToFlutter(cl.toJson());
        }

        public static void QuitNative()
        {
            CallbackLoader cl = new CallbackLoader(CallbackLoader.LoaderStatus.quitWindow);
            UnityMessageManager.Instance.QuitUnityWindow();
            Debug.Log($"QuitNative() -> {cl.toJson()}");
            UnityMessageManager.Instance.SendMessageToFlutter(cl.toJson());
        }
    }
}
