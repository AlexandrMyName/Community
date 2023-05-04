using PlansSystem;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace AnimationsSystem
{
    [Serializable]
    public class AnimationData : IPriority, IProbabilitySolver
    {
        #region Код связанный с генерацией дерева стейтов
        //internal void print(string message) => Debug.Log(message);
        public AnimationClip clip;
        public string clipName { get => clip == null ? "" : clip.name; }
        public bool isNull { get => clip == null; }

        #region oldNameKey
        public string dontChangeItIsSystemKey = "";
        public string realKey { get => dontChangeItIsSystemKey; set => dontChangeItIsSystemKey = value; }
        /*public string oldNameKey 
        {
            get
            {
                if (_oldNameKey == null) _oldNameKey = "";
                return _oldNameKey;
            }
            set {

                _oldNameKey = value;
            } 
        }*/
        #endregion

#if UNITY_EDITOR
        internal bool OnValidate(AnimationsSO so, int layer)
        {
            if (clip != null && clip.events.Length == 0) {
                var animEvent = new AnimationEvent();
                animEvent.time = clip.length;// - 0.01f;
                animEvent.stringParameter = clip.name;
                animEvent.functionName = "EndAnimation";
                UnityEditor.AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[1] { animEvent });
            }
            AnimatorController controller = so.controller;

            //if (_oldNameKey == null) _oldNameKey = nameKey;

            if (realKey != clipName && controller != null)
            {
                //Ищем старую копию анимационного элемента
                //print($"Ищу индекс по кею {realKey}");
                int index = AnimationWorkingData.FindIndex(so.workingData.oldAnimations, realKey);
                if (index != -1) realKey = so.workingData.oldAnimations[index].clipName;
                else realKey = "";

                //Если клип был удален, то удалим элемент
                if (clipName.Length < 1)
                {
                    //print($"клип пустой начать удаление {clipName} : {realKey}, clip = {clip?.name}");
                    if (realKey.Length > 0)
                    {
                        var first = controller.parameters.FirstOrDefault(p => p.name == realKey);
                        if (first != null)
                        {
                            controller.RemoveParameter(first);
                            var state = controller.layers[layer].stateMachine.states[so.findState(layer, realKey)].state;
                            controller.layers[layer].stateMachine.RemoveState(state);
                        }
                    }
                    realKey = clipName;
                    return true;
                }
                //Далее проверим нет ли уже в контроллере стейтов и параметров с новым ключом
                else if (so.IdleAnimations.FindAll((ad) => ad.clipName == clipName).Count > 1)
                {
                    //print($"найдена связь с таким именем уже {clipName} : {realKey}, clip = {clip?.name}");
                    //Если есть, то либо вернем прошлое значение
                    if (realKey.Length > 0)
                    {
                        clip = so.workingData.oldAnimations[index].clip;
                        realKey = so.workingData.oldAnimations[index].realKey;
                    }
                    //либо обнулим новый ключ
                    else { 
                        Debug.Log($"В дереве уже имеется ключ и анимация с именем {clipName}");
                        clip = null;
                        realKey = clipName;
                        return true;
                    }
                }
                //Новый ключ уникальный и не пустой
                else {
                    //Если старый ключ тоже существовал
                    if (realKey.Length > 0)
                    {
                        //print($"переиделываю связи со стейтом {clipName} : {realKey}, clip = {clip?.name}");
                        /*
                        //Удалим старые данные
                        var first = controller.parameters.FirstOrDefault(p => p.name == realKey);
                        if (first != null)
                        {
                            controller.RemoveParameter(first);
                            var state = controller.layers[layer].stateMachine.states[findState(controller.layers[layer].stateMachine, realKey)].state;
                            controller.layers[layer].stateMachine.RemoveState(state);
                        }*/

                        //Если новый ключ существует, то значит надо обновить данные для стейта по старому ключу
                        controller.AddParameter(clipName, AnimatorControllerParameterType.Bool);
                        so.renameTransitionsCondition(layer, realKey, clipName);
                        var parameter = controller.parameters.FirstOrDefault(p => p.name == realKey);
                        if (parameter != null) controller.RemoveParameter(parameter);                        
                    }
                    else {
                        //print($"СОЗДАЮ НОВЫЙ элемент {clipName} : {realKey}, clip = {clip.name}");

                        controller.AddParameter(clipName, AnimatorControllerParameterType.Bool);
                        //CreateNodeState(controller, clip, layer);
                        controller.AddMotion(clip, layer);
                        so.makeTransitions(layer, clip.name);
                    }


                }
            }

            realKey = clipName;
            return false;
        }
#endif
        public override string ToString()
        {
            var strClip = clip != null ? clip.name.ToString() : "null";
            return $"AnimationData(newK:{clipName},oldK:{realKey},clip:{strClip})";
        }

#if UNITY_EDITOR
        internal void Delete(AnimationsSO so, int layer)
        {
            var controller = so.controller;

            //if (clipName.Length < 0) print("Вероятно удаленный элемент, был пустым");
            if (clipName.Length < 0) return;
            var first = controller.parameters.FirstOrDefault(p => p.name == clipName);
            if (first != null)
            {
                controller.RemoveParameter(first);
                var state = controller.layers[layer].stateMachine.states[so.findState(layer, clipName)].state;
                controller.layers[layer].stateMachine.RemoveState(state);
                //controller.animationClips[0]
            }
            //else print($"ПАРАМЕТР {clipName} НЕ БЫЛ НАЙДЕН");

            clip = null;
        }
#endif
        internal void Clean()
        {
            realKey = "";
            clip = null;
        }

        internal AnimationData Copy()
        {
            var ad = new AnimationData();
            ad.realKey = realKey;
            ad.clip = clip;

            ad.result = result;
            ad._priority = _priority;
            ad._count = _count;
            ad._showRarityCoef = _showRarityCoef;
            ad.dontChangeItIsSystemKey = dontChangeItIsSystemKey;
            ad.iterationCount = iterationCount;
            ad.maximumIterations = maximumIterations;
            ad.minimumIterations = minimumIterations;

            ad.Probability = Probability;
            return ad;
        }
        #endregion


        #region ShowsCounter
        [SerializeField] private int iterationCount = 0;
        public int IterationCount { get => iterationCount; set => iterationCount = value; }
        [SerializeField] private int maximumIterations = 8;
        [SerializeField] private int minimumIterations = 1;
        //public int maximumIterations { get; set; }
        //public int minimumIterations { get; set; }
        public bool? shouldShow(int animI, AnimationController controller)
        {
             
            //if (!inst.ActiveGeneral.HasValue || !inst.ActivePlanIndex.HasValue) return null;
            if (controller.activeIndex == -1) return null;
            if (controller.activeIndex != animI) return null;
            //if (inst.ActivePlanIndex != planI) return null;

            if (IterationCount >= maximumIterations)
            {
                Debug.Log($"counter?.shouldShow() -> {IterationCount} > {maximumIterations}");
                IterationCount = 0;
                return false;
            }
            if (IterationCount < minimumIterations)
            {
                //if(incrementCounter) iterationCount++;
                return true;
            }
            return null;
        }
        public void setIterations(int minimum, int maximum)
        {
            maximumIterations = maximum;
            minimumIterations = minimum;
        }
        public void resetCounter() => IterationCount = 0;
        #endregion
        #region ProbabilitySolver
        public int MaximumIterations { get => maximumIterations; set => maximumIterations = value; }
        public int MinimumIterations { get => minimumIterations; set => minimumIterations = value; }

        //[SerializeField] 
        private float result;
        //[SerializeField] 
        private float _showRarityCoef;
        //[SerializeField] 
        [SerializeField] private int _count = 0;
        public int Count { get => _count; set => _count = value; }
        #region REAPEATER COEFFICENT - Не используемая фича
        //public float RepeaterCoef { get => (maximumIterations - IterationCount) / maximumIterations; }
        //НЕ ИСПОЛЬЗУЕМАЯ ФИЧА:
        //ЧТО ЕСЛИ ПРИ ПОДСЧЕТЕ ВЕРОЯТНОСТИ ИСПОЛЬЗОВАНИЯ ПЛАНА
        //СЧИТАТЬ СКОЛЬКО ИТЕРАЦИЙ ОТ МАКСИМУМА УЖЕ БЫЛО И ДОМНОЖАТЬ НА ЭТОТ КОЭФФИЦЕНТ
        //НАПРИМЕР ПЛАН УЖЕ БЫЛ ПОКАЗАН 3 раза ПОДРЯД, (iter = 3), а МАКСИМУМ 10
        //ТОГДА REPEATER = (10-3)/10 = 0.7
        #endregion
        public float calculateProbability(int total, int listCount)
        {
            //если всего было 20 показов, а этот план показали 5 раз то showRarityCoef = (20-5)/20 = 15 / 20 = 0.75
            //если всего было 20 показов, а этот план показали 19 раз то showRarityCoef = (20-19)/20 = 1 / 20 = 0.05
            _showRarityCoef = total == 0 || _count == 0 ? 1f : (total - _count) / (float)total;
            result = (Probability + _showRarityCoef) / listCount;
            return result;
        }
        #endregion
        #region Priority
        [SerializeField] private int _priority = 5;
        public int Priority { get => _priority; set => _priority = value; }
        public float Probability { get; set; }
        //public float Probability { get => _probability; set => _probability = value; }
        //[SerializeField] private float _probability;
        #endregion
    }
}