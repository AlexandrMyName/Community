using Utils;
using AnimationsSystem;
using System.Collections;
using UnityEngine;

namespace Client
{
    public class CharacterAudioPlayer : MonoBehaviour
    {
        public HeadController lookController;
        public Animator animator;
        public AnimationController animationController;
        public AudioSource audioSource;
        private AudioClip lastAudioClip;
        bool _sayAfterTransition = false;
        public void HandleMessage(CharacterMessage message, int outputRate = 48000)
        {
            LastMessage = message;

            var bytes = CustomAudioConverter.getBytes(message.audioBytes64);
            if(bytes.Length > 0) lastAudioClip = CustomAudioConverter.FromMp3Data(bytes, outputRate: outputRate);

            lookController.LookWith(message, lastAudioClip);

            _sayAfterTransition = LastMessage.messageAnimationInfo?.sayAfterTransition ?? false;
            if (!_sayAfterTransition) ChooseTrackAction();

            animationController.PlayAnimation(LastMessage.messageAnimationInfo);
        }
        public void AnimationCallback() {
            if (_sayAfterTransition)
            {
                _sayAfterTransition = false;
                ChooseTrackAction();
            }
            CallbackAnimation callback = new CallbackAnimation(LastMessage.characterId, EventStatus.end);
            FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter(callback.toJson());
        }

        #region Audio Actions
        public CharacterMessage LastMessage { get => lastMessage.HasValue ? lastMessage.Value : new CharacterMessage(); set => lastMessage = value; }
        private CharacterMessage? lastMessage;
        public void ChooseTrackAction(PlayerState? state = null) {
            state ??= LastMessage.playerState;
            if (state == null) return;
            switch (state)
            {
                case PlayerState.play: PlayTrack(); break;
                case PlayerState.pause: PauseTrack(); break;
                case PlayerState.stop: StopTrack(); break;
            }
        }
        public void PlayTrack()
        {
            if (lastAudioClip == null) return;
            if (waitEndAudioCorutine != null) StopCoroutine(waitEndAudioCorutine);
            waitEndAudioCorutine = WaitEndAudio(lastAudioClip.length);
            StartCoroutine(waitEndAudioCorutine);
            audioSource.clip = lastAudioClip;
            audioSource.Play();
        }
        IEnumerator waitEndAudioCorutine;


        IEnumerator WaitEndAudio(float length) { 
            yield return new WaitForSeconds(length);
            //yield return new WaitForSecondsRealtime(length);
            FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter(
                new CallbackAudio(EventStatus.end).toJson());
            waitEndAudioCorutine = null;
        }
        private void PauseTrack()
        {
            FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter(
                new CallbackAudio(EventStatus.pause).toJson());
            audioSource.Pause();
            if (waitEndAudioCorutine != null) StopCoroutine(waitEndAudioCorutine);
        }
        private void StopTrack()
        {
            FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter(
                new CallbackAudio(EventStatus.stop).toJson());
            audioSource.Stop();
            if(waitEndAudioCorutine!=null) StopCoroutine(waitEndAudioCorutine);
        }
        #endregion
        #region OnValidate and Reset
        private void OnValidate()
        {
            if (audioSource == null) TryGetComponent<AudioSource>(out audioSource);
            if (animationController == null) TryGetComponent<AnimationController>(out animationController);
            if (animator == null) TryGetComponent<Animator>(out animator);
            if (lookController == null) TryGetComponent<HeadController>(out lookController);
        }
        private void Reset()
        {
            if (audioSource == null) TryGetComponent<AudioSource>(out audioSource);
            if (animationController == null) TryGetComponent<AnimationController>(out animationController);
            if (animator == null) TryGetComponent<Animator>(out animator);
            if (lookController == null) TryGetComponent<HeadController>(out lookController);
        }
        #endregion
    }
}
