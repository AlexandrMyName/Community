using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationsSystem
{
    public class AnimationController : MonoBehaviour
    {
        #region Animation
        [SerializeField] AnimationsSO animationDescriber;
        [SerializeField] CharacterAudioPlayer topController;
        [SerializeField] Animator animator;
        [SerializeField] List<AnimationData> animations;
        //[SerializeField] AnimationData lastAnimation = null;
        private string lastAnimKey = "";
        //private MessageAnimationInfo lastAnimation;
        public static bool DebugKey = false;
        public bool debug = false;

        private int listCount { get => animations.Count; }

        #region Animation Actions
        public void EndAnimation(string animationKey) {

            //if (lastAnimKey != "") animator.SetBool(lastAnimKey, false);
            if(DebugKey) Debug.Log($"END ANIMATION {animationKey}");
            NextAnimation();

            topController.AnimationCallback();
        }

        public void PlayAnimation(MessageAnimationInfo messageAnimationInfo)
        {
            //if (lastAnimKey != null) animator.SetBool(lastAnimKey, false);

           // if (messageAnimationInfo != null)
           // {
           //     animator.SetBool(messageAnimationInfo.animationKeyName, true);
           // }
            
        }
        private void DisableAnimation(string key, float withDelay = 0.0f)
        {
            if (withDelay != 0.0f) StartCoroutine(disableCorutine(key, withDelay));
            else if (key != null && key != "") animator.SetBool(key, false);
        }
        IEnumerator disableCorutine(string key, float withDelay) {
            yield return new WaitForSeconds(withDelay);
            if (key != null) animator.SetBool(key, false);
        }
        #endregion
        #endregion

        public int TotalCount = -1;
        public int activeIndex = -1;


        private void Start()
        {
            animations = AnimationsSO.CopyAnimations(animationDescriber.IdleAnimations);
            NextAnimation();
            TotalCount = 0;
            DebugKey = debug;
            //Time.timeScale = 16.0f;
        }

        #region Choosing Animation Logic
        private float summaryProbability = 0.0f;
        public void NextAnimation()
        {
            /*
            bool firstTime = TotalCount == -1;
            if (firstTime)
            {
                TotalCount = 0;
                //animations = animationDescriber.IdleAnimations;
                //animations.ForEach(plan => plan.setIterations(2, 8));
            }*/
            summaryProbability = 0.0f;
            if (listCount < 1)
            {
                Debug.Log("Отсутствуют хоть какие-то сохраненные камеры");
                FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter("Отсутствуют хоть какие-то сохраненные камеры");
                return;
            }

            float randValue = UnityEngine.Random.Range(0.0f, 1.0f);
            //int iter = 0;
            bool choosed = false;
            bool? showPlan;
            //while (!choosed && iter < 1000)
            //{
            //iter++;
            for (int animI = 0; !choosed && animI < listCount; animI++)
            {
                //if (choosed) setDisableCharacterCamera(planI, characterId);
                if (!choosed)
                {
                    showPlan = animations[animI].shouldShow(animI,this);
                    if (DebugKey) Debug.Log($"animations[{animI}].sholdShow() return {(showPlan.HasValue ? showPlan.Value.ToString() : "null")}");
                    if (animI == listCount - 1) {
                        if (showPlan.HasValue)
                        {
                            if (showPlan.Value)
                            {
                                choosed = setActiveAnimation(animI);
                            }
                            else
                            {
                                choosed = setActiveAnimation(0);
                                //setDisableGeneralCamera(planI);
                            }
                        }
                        else choosed = setActiveAnimation(animI, animI != activeIndex);
                        return;
                    }

                    if (showPlan.HasValue)
                    {
                        if (showPlan.Value)
                        {
                            if (DebugKey) Debug.Log($"setActiveCamera for GENERAL[{animI}] because repeater {showPlan.Value}");
                            choosed = setActiveAnimation(animI);
                        }
                        else
                        {
                            summaryProbability += animations[animI].calculateProbability(TotalCount, listCount);
                            //setDisableGeneralCamera(planI);
                        }
                    }
                    else
                    {
                        //if(!currentActivePlanShouldShow()) animations[planI].iterationCount++;
                        summaryProbability += animations[animI].calculateProbability(TotalCount, listCount);
                        if (summaryProbability >= randValue)
                        {
                            if (DebugKey) Debug.Log($"{summaryProbability} >= {randValue}  :   animations[{animI}].{animations[animI].clipName}");
                            if (DebugKey) Debug.Log($"(animI{animI} != activeIndex{activeIndex} {animI != activeIndex}");
                            choosed = setActiveAnimation(animI, animI != activeIndex);
                        }
                        else if (DebugKey) Debug.Log($"{summaryProbability} < {randValue}  :   animations[{animI}].{animations[animI].clipName}");
                    }
                }
            }
        }
        private bool currentActivePlanShouldShow()
        {
            if (activeIndex == -1) return false;

            var mustBeShow = animations[activeIndex].shouldShow(activeIndex,this);
            //Debug.Log($"mustBeShow = {(mustBeShow.HasValue ? mustBeShow.Value : "null")}");
            return mustBeShow.HasValue && mustBeShow.Value;
        }
        private bool setActiveAnimation(int animI, bool resetCounter = false)
        {
            if (DebugKey) Debug.Log($"SetActiveAnim {animI}, reset? {resetCounter}");
            //Debug.Log($"setActiveGeneralCamera() -> {(counter == null ? "counter is null" : $"current ShouldShow = {currentActivePlanShouldShow()}")}");

            //1. Если идет установка той же самой активной анимации, и текущая анимация должна показаться ещё раз,
            //то метод проходит дальше
            //2. но Если это другая анимация, хотя какая-то другая анимация должна показаться, то тут мы выходим

            //в if описан 2 случай
            if (animI != activeIndex && currentActivePlanShouldShow() && DebugKey)
                Debug.Log("ВЫХОД ИЗ SetActiveAnimation");
            if (animI != activeIndex && currentActivePlanShouldShow()) return false;

            //Debug.Log($"setActiveGeneralCamera() -> if ({resetCounter} && {counter != null})");
            if (resetCounter && activeIndex != -1) animations[activeIndex].resetCounter();
            
            //Switch Camera
            DisableAnimation(lastAnimKey);
            activeIndex = animI;
            lastAnimKey = animations[activeIndex].clipName;
            animator.SetBool(lastAnimKey, true);
            if (DebugKey) Debug.Log($"SetActiveAnim lastAnimKey = {lastAnimKey}");

            animations[activeIndex].IterationCount++;
            if (animations[activeIndex].IterationCount >= 4) {
                if (DebugKey) Debug.LogError($"animations[{activeIndex}].{animations[activeIndex].clipName}.IterationCount{animations[activeIndex].IterationCount} >= 3");
                if (DebugKey) Debug.Break();
            }
            animations[activeIndex].Count++;
            TotalCount++;
            //DisableAnimation(lastAnimKey, 0.2f);

            //Utils.ClearLogConsole();
            return true;
        }
        #region setSettings commented
        /*
        public void setSettings(CamerasSettings camerasSettings)
        {
            if (camerasSettings.settingsForAllanimations != null)
                foreach (var plan in animations)
                {
                    plan.setSettings(camerasSettings.settingsForAllanimations);
                }
            if (camerasSettings.settingsForAllCharactersPlans != null)
                foreach (var plan in CharactersPlans)
                {
                    plan.setSettings(camerasSettings.settingsForAllCharactersPlans);
                }

            if (camerasSettings.animations != null)
                foreach (var planSettings in camerasSettings.animations)
                    if (planSettings.index < animations.Count)
                        animations[planSettings.index].setSettings(planSettings);
            if (camerasSettings.charactersPlans != null)
                foreach (var planSettings in camerasSettings.charactersPlans)
                    if (planSettings.index < CharactersPlans.Count)
                        CharactersPlans[planSettings.index].setSettings(planSettings);
        }*/
        #endregion

        #endregion

        #region OnValidate
        private void OnValidate()
        {
            if (animator == null) TryGetComponent<Animator>(out animator);
            if (topController == null) TryGetComponent<CharacterAudioPlayer>(out topController);
            if (animationDescriber != null) animations = AnimationsSO.CopyAnimations(animationDescriber.IdleAnimations);
#if UNITY_EDITOR
            if (animationDescriber != null && animator != null) animator.runtimeAnimatorController = animationDescriber.controller;
#endif
        }
#endregion
    }
}