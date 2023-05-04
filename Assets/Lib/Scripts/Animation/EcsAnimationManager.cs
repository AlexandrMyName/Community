using AnimationsSystem;
using Client;
using Leopotam.EcsLite;
using System;
using UnityEngine;

namespace PlansSystem
{
    public class EcsAnimationManager : MonoBehaviour, IEcsRunSystem, IEcsInitSystem
    {
        protected EcsWorld world;
        //protected EcsPool<MessageCameraInfo> messagesPool;
        protected EcsPool<CamerasSettings> settingsPool;
        //public AnimationsInfo info;

        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            //messagesPool = world.GetPool<MessageCameraInfo>();
            settingsPool = world.GetPool<CamerasSettings>();
        }

        void IEcsRunSystem.Run(IEcsSystems systems)
        {
            try
            {
                var filterMessages = world.Filter<MessageCameraInfo>().End();
                var filterSettings = world.Filter<CamerasSettings>().End();

                foreach (int entity in filterMessages)
                {
                    //var message = messagesPool.Get(entity);
                    //info.pickCamera(message.characterId);
                }
                foreach (int entity in filterSettings)
                {
                    //info.setSettings(settingsPool.Get(entity));
                }
            }
            catch (Exception e)
            {
                Debug.Log($"E  - {e.Message}");
                Debug.Log($"ST - {e.StackTrace}");
                FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter($"DEBUG_TAG : EcsAnimationManager() e -> {e.Message}");
                FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter($"DEBUG_TAG : EcsAnimationManager() st-> {e.StackTrace}");
            }
        }
        //private void OnValidate() => info.OnValidate();
    }
}