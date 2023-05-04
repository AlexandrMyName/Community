using PlansSystem;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace AnimationsSystem
{
    #region AnimatiosSO
    [CreateAssetMenu(fileName = "AnimationTreeGenerator", menuName = "ScriptableObjects/Animation3GenSO")]
    public class AnimationsSO : ScriptableObject
    {

#if UNITY_EDITOR
        public void Regenerate() {
            IdleAnimations.ForEach(anim => anim.Delete(this,0));
            IdleAnimations.Clear();

            workingData.oldAnimations.ForEach(old =>
            {
                    
            });
        }
#endif
        public static List<AnimationData> CopyAnimations(List<AnimationData> idleAnimations)
        {
            List<AnimationData> anims = new List<AnimationData>();
            idleAnimations.ForEach(animation =>
            {
                anims.Add(animation.Copy());
            });
            return anims;
        }

        [SerializeField] int _sumPriority = 0;
        internal void print(string message) => Debug.Log(message);
        public float exitTime = 0.95f;
        public float transitionDuration = 0.6f;
#if UNITY_EDITOR
        public AnimatorController controller;
#endif
        public List<AnimationData> IdleAnimations;
        public AnimationWorkingData workingData;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _sumPriority = 0;
            if (workingData == null || IdleAnimations == null || workingData.oldAnimations == null) {                
                if (workingData == null) Debug.Log("Не возможно проверить список анимаций, в SO, из-за того что workingData == null");
                if (IdleAnimations == null) Debug.Log("Не возможно проверить список анимаций, в SO, из-за того что IdleAnimations == null");
                if (workingData.oldAnimations == null) Debug.Log("Не возможно проверить список анимаций, в SO, из-за того что workingData.oldAnimations == null");
                return;
            }
            if (IdleAnimations.Count != workingData.oldAnimations.Count)
            {
                if (workingData.oldAnimations.Count < IdleAnimations.Count)
                {
                    var lastAnimations = IdleAnimations.Last();
                    lastAnimations.Clean();
                }
                else
                {
                    var missing = AnimationWorkingData.FindMissing(IdleAnimations, workingData.oldAnimations);
                    missing.Delete(this,0);
                }        
            }
            else for (int i = 0; i < IdleAnimations.Count; i++)
                {
                    if (IdleAnimations[i].OnValidate(this, 0) && i != IdleAnimations.Count - 1)
                    {
                        IdleAnimations.RemoveAt(i);
                        i--;
                    }
                    else _sumPriority += IdleAnimations[i].Priority;
                }

            foreach (var anim in IdleAnimations)
                ((IPriority)anim).setProbability(_sumPriority);

            workingData.CopyAnimations(IdleAnimations);
        }
#endif

#if UNITY_EDITOR
        internal int findState(int layer, string nameState)
        {
            AnimatorStateMachine stateMachine = controller.layers[layer].stateMachine;

            for (int i = 0; i < stateMachine.states.Length; i++)
                if (stateMachine.states[i].state.name == nameState)
                {
                    return i;
                }
            return -1;
        }

        internal void makeTransitions(int layer, string nameState)
        {
            AnimatorStateMachine stateMachine = controller.layers[layer].stateMachine;

            int foundedIndex = findState(layer, nameState);
            if (foundedIndex == -1) return;
            stateMachine.states[foundedIndex].state.name = nameState;

            AnimatorStateTransition transition;
            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                if (i != foundedIndex)
                {
                    transition = stateMachine.states[foundedIndex].state.AddTransition(stateMachine.states[i].state);
                    transition.hasExitTime = true;
                    transition.duration = transitionDuration;
                    transition.exitTime = exitTime;
                    transition.interruptionSource = TransitionInterruptionSource.DestinationThenSource;
                    transition.AddCondition(AnimatorConditionMode.If, 0, stateMachine.states[i].state.name);

                    transition = stateMachine.states[i].state.AddTransition(stateMachine.states[foundedIndex].state);
                    transition.hasExitTime = true;
                    transition.duration = transitionDuration;
                    transition.exitTime = exitTime;
                    transition.interruptionSource = TransitionInterruptionSource.DestinationThenSource;
                    transition.AddCondition(AnimatorConditionMode.If, 0, nameState);
                }
            }
            transition = stateMachine.states[foundedIndex].state.AddTransition(stateMachine.states[foundedIndex].state);
            transition.hasExitTime = true;
            transition.duration = transitionDuration;
            transition.exitTime = exitTime;
            transition.interruptionSource = TransitionInterruptionSource.DestinationThenSource;
            transition.AddCondition(AnimatorConditionMode.If, 0, nameState);            
        }
        internal void renameTransitionsCondition(int layer, string oldName, string nameState)
        {
            Debug.Log($"oldName {oldName} : nameState {nameState}");
            AnimatorStateMachine stateMachine = controller.layers[layer].stateMachine;

            int foundedIndex = findState(layer, oldName);
            if (foundedIndex == -1) return;
            stateMachine.states[foundedIndex].state.name = nameState;



            AnimatorStateTransition transition;
            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                if (i != foundedIndex)
                {
                    transition = stateMachine.states[i].state.transitions.FirstOrDefault(t => t.conditions[0].parameter == oldName);
                    if (transition != null && transition.conditions.Length > 0) 
                    {
                        transition.RemoveCondition(transition.conditions[0]);
                        transition.AddCondition(AnimatorConditionMode.If, 0, nameState);

                        //transition.conditions[0].parameter = nameState;
                    }
                        
                }
            }
            //Перезапись транзишена к самому себе



            transition = stateMachine.states[foundedIndex].state.transitions.FirstOrDefault(t => t.conditions[0].parameter == oldName);
            if (transition != null && transition.conditions.Length > 0)
            {
                transition.RemoveCondition(transition.conditions[0]);
                transition.AddCondition(AnimatorConditionMode.If, 0, nameState);

                //transition.conditions[0].parameter = nameState;
            }


            /*
            var foundedTransitionIndex = findTransition(stateMachine.states[foundedIndex].state.transitions, oldName);
            if (foundedTransitionIndex != -1) { 
                stateMachine.states[foundedIndex].state.transitions[foundedTransitionIndex].conditions = new AnimatorCondition[0];             
                stateMachine.states[foundedIndex].state.transitions[foundedTransitionIndex].AddCondition(AnimatorConditionMode.If, 0, nameState);
            }*/
        }
        internal int findTransition(AnimatorStateTransition[] transitions, string endpointNameState)
        {
            for (int i = 0; i < transitions.Length; i++)
                if (transitions[i].destinationState.name == endpointNameState)
                {
                    return i;
                }
            return -1;
        }
#endif
    }
    #endregion
}