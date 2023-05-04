using Leopotam.EcsLite;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client {
    public class EcsCharacterManager : MonoBehaviour, IEcsRunSystem, IEcsInitSystem {
        public int outputRate = 48000;
        public List<CharacterAudioPlayer> orderedPlayers = new List<CharacterAudioPlayer>();
        protected EcsWorld world;
        protected EcsPool<CharacterMessage> messagesPool;
        protected int lastCharacterId = -1;
        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            messagesPool = world.GetPool<CharacterMessage>();
        }

        public void Run(IEcsSystems systems) {
            try
            {
                var filter = world.Filter<CharacterMessage>().End();

                foreach (int entity in filter)
                {
                    var message = messagesPool.Get(entity);
                    if (message.characterId < orderedPlayers.Count)
                    {
                        lastCharacterId = message.characterId;
                        StopSpeaking(lastCharacterId);
                        orderedPlayers[lastCharacterId].HandleMessage(message, outputRate: outputRate);//, audioClip: clip);
                        //var bytes = CustomAudioSystemConverters.CustomAudioConverter.getBytes(message.audioBytes64);
                        //clip = CustomAudioSystemConverters.CustomAudioConverter.FromMp3Data(bytes, outputRate: outputRate)
                    }
                    // Для остановки/приостановки сообщений нужно написать логику, 
                    // возможно нужно иметь где-то в этом классе информацию о последнем выбраном персонаже,
                    // что-бы для него приостанавливать проигрыш трека
                    else if (lastCharacterId !=  -1)
                    {
                        orderedPlayers[lastCharacterId].HandleMessage(message, outputRate: outputRate);
                        //foreach (var player in orderedPlayers)
                        //    player.ChooseTrackAction(message.playerState);
                    }

                }
            }
            catch (Exception e) {
                Debug.Log($"E  - {e.Message}");
                Debug.Log($"ST - {e.StackTrace}");
                FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter($"DEBUG_TAG : EcsCharacterManager() e -> {e.Message}");
                FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter($"DEBUG_TAG : EcsCharacterManager() st-> {e.StackTrace}");
            }
        }
        private void StopSpeaking(int charId)
        {
            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                if (i != charId) 
                    orderedPlayers[i].ChooseTrackAction(PlayerState.stop);
            }
        }
    }
}