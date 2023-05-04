using FlutterUnityIntegration;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Client;
using Leopotam.EcsLite;
using Unity.IO.LowLevel.Unsafe;

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(BridgeEventHandler))]
public class BridgeEventHandlerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {

        BridgeEventHandler myScript = (BridgeEventHandler)target;

        if (GUILayout.Button("Push Test JSON Message"))
        {
            myScript.DebugAcceptMessage(myScript.testJsonMessage, "CharacterMessage");
        }
        if (GUILayout.Button("Load Interview Scene"))
        {
                var settings = new SceneSettings();
                settings.sceneName = "Interview ver2";
                settings.cameras = new CamerasSettings
                {
                    settingsForAllGeneralPlans = new PlanSettings() {
                        maximumIterations = 6,
                        minimumIterations = 2
                    },
                    settingsForAllCharactersPlans = new PlanSettings()
                    {
                        maximumIterations = 6,
                        minimumIterations = 2
                    },
                    generalPlans = new System.Collections.Generic.List<PlanSettings>()
                    {
                        new PlanSettings(){
                            index = 0,
                            priority = 10,
                            maximumIterations = 9,
                            minimumIterations = 3
                        }
                    },
                    charactersPlans = new System.Collections.Generic.List<PlanSettings>()
                    {
                        new PlanSettings(){
                            index = 0,
                            priority = 15,
                            maximumIterations = 6,
                            minimumIterations = 3
                        }
                    },
                };
                myScript.processSettings(settings);
        }
        if (GUILayout.Button("Load Kids Scene"))
        {
                var settings = new SceneSettings();
                settings.sceneName = "LightPreFinal01";
                settings.cameras = new CamerasSettings
                {
                    settingsForAllGeneralPlans = new PlanSettings() {
                        maximumIterations = 6,
                        minimumIterations = 2
                    },
                    settingsForAllCharactersPlans = new PlanSettings()
                    {
                        maximumIterations = 6,
                        minimumIterations = 2
                    },
                    generalPlans = new System.Collections.Generic.List<PlanSettings>()
                    {
                        new PlanSettings(){
                            index = 0,
                            priority = 10,
                            maximumIterations = 9,
                            minimumIterations = 3
                        }
                    },
                    charactersPlans = new System.Collections.Generic.List<PlanSettings>()
                    {
                        new PlanSettings(){
                            index = 0,
                            priority = 15,
                            maximumIterations = 6,
                            minimumIterations = 3
                        }
                    },
                };
                myScript.processSettings(settings);
        }
        DrawDefaultInspector();
    }
}
#endif
public class BridgeEventHandler : MonoBehaviour, IEventSystemHandler, IEcsInitSystem
{
    [TextArea(3, 20)]
    public string testJsonMessage;
    EcsWorld world;
    EcsPool<CharacterMessage> messagesPool;
    EcsPool<MessageCameraInfo> cameraPool;
    EcsPool<SceneSettings> settingsPool;
    //EcsPool<MessageStateType> eventStatePool;
    //EcsPool<MessageAudioInfo> audioPool;

    public void DebugAcceptMessage(String deserializedData, String eventKey)
    {
        try
        {
            switch (eventKey)
            {
                case "SceneSettings": processSettings(SceneSettings.DeserializeJson(deserializedData)); break;
                case "CharacterMessage": processMessage(CharacterMessage.DeserializeJson(deserializedData)); break;
            }
        }
        catch (Exception e)
        {
            UnityMessageManager.Instance.SendMessageToFlutter($"SimpleAudioManager (Manager) -> AcceptMessage получил сообщение {deserializedData}, но получил ошибку при попытке обработки");
            UnityMessageManager.Instance.SendMessageToFlutter($"{e.Message}, {e.StackTrace}");
            Debug.Log($"{e.Message}, {e.StackTrace}");
        }
    }
    public void AcceptMessage(String message)
    {
        try
        {
            var eventMessage = BridgeEventMessage.DeserializeJson(message);

            switch (eventMessage.eventKey)
            {
                case "SceneSettings": processSettings(SceneSettings.DeserializeJson(eventMessage.data)); break;
                case "CharacterMessage": processMessage(CharacterMessage.DeserializeJson(eventMessage.data)); break;
            }
        }
        catch (Exception e)
        {
            UnityMessageManager.Instance.SendMessageToFlutter($"SimpleAudioManager (Manager) -> AcceptMessage получил сообщение {message}, но получил ошибку при попытке обработки");
            UnityMessageManager.Instance.SendMessageToFlutter($"{e.Message}, {e.StackTrace}");
            Debug.Log($"{e.Message}, {e.StackTrace}");
        }
    }

    public void processMessage(CharacterMessage message) {
        var entity = world.NewEntity();
        messagesPool.Add(entity).Copy(message);
        cameraPool.Add(entity).characterId = message.characterId;
        //characterPool.Add(entity).Copy(message.CharacterInfo);
        //eventStatePool.Add(entity).Copy(message.MessageStateType);
        //audioPool.Add(entity).Copy(message.MessageAudioInfo);
    }
    public void processSettings(SceneSettings settings) {
        var entity = world.NewEntity();
        settingsPool.Add(entity).Copy(settings);
    }

    public void Init(IEcsSystems systems)
    {
        world = systems.GetWorld();
        messagesPool = world.GetPool<CharacterMessage>();
        cameraPool = world.GetPool<MessageCameraInfo>();
        settingsPool = world.GetPool<SceneSettings>();
        //eventStatePool = world.GetPool<MessageStateType>();
        //audioPool = world.GetPool<MessageAudioInfo>();
    }
}
