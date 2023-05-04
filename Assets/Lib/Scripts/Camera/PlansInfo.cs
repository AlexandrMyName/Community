using Client;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace PlansSystem
{
    [Serializable]
    public class PlansInfo : IPriority
    {
        public static PlansInfo instance;
        #region IPriority
        [SerializeField] private int _priority = 0;
        public int Priority { get => _priority; set => _priority = value; }
        public float Probability { get; set; }
        public void setProbability(int total = 0)
        {
            instance = this;
            _priority = 0;
            foreach (GeneralPlan plan in GeneralPlans) _priority += plan.Priority;
            foreach (CharactersPlan character in CharactersPlans) _priority += character.Priority;

            foreach (GeneralPlan plan in GeneralPlans) 
                ((IPriority)plan).setProbability(_priority);
            
            foreach (CharactersPlan character in CharactersPlans) 
                ((IPriority)character).setProbability(_priority);            
        }
        #endregion

        //public bool useRandomMix = true;
        public int PlansListCount = -1;
        public List<GeneralPlan> GeneralPlans;
        public List<CharactersPlan> CharactersPlans;
        public int TotalCount = 0;

        [SerializeField] private Camera enabledCamera = null;
        private IShowsCounter counter;
        [SerializeField] private bool? activeGeneral = null;
        [SerializeField] private int activePlanIndex = -1;
        [SerializeField] private int activeCharacterIndex = -1;
        //[SerializeField] private float randValue = 0.0f;

        public float SummaryProbability { get => summaryProbability; set {
                Debug.Log($"Summary Probability was {summaryProbability}, but = new({value}), difference {summaryProbability-value}");
                summaryProbability = value; } }
        float summaryProbability = 0.0f;

        public bool? ActiveGeneral { get => activeGeneral; set => activeGeneral = value; }
        public int? ActivePlanIndex { get => activePlanIndex; set => activePlanIndex = value ?? -1; }
        public int? ActiveCharacterIndex { get => activeCharacterIndex; set => activeCharacterIndex = value ?? -1; }

        public void pickCamera(int characterId)
        {
            bool firstTime = PlansListCount == -1;
            if (firstTime)
            {
                PlansListCount = CharactersPlans.Count + GeneralPlans.Count;
                GeneralPlans.ForEach(plan => plan.setIterations(2, 8));
                CharactersPlans.ForEach(plan => plan.setIterations(2, 8));
            }
            SummaryProbability = 0.0f;
            instance = this;
            if (GeneralPlans.Count < 1 && CharactersPlans.Count < 1)
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
                for (int planI = 0; !choosed && planI < GeneralPlans.Count; planI++)
                {   
                    //if (choosed) setDisableCharacterCamera(planI, characterId);
                    if (!choosed)
                    {
                        showPlan = GeneralPlans[planI].shouldShow(planI);
                        Debug.Log($"GeneralPlans[{planI}].sholdShow() return {(showPlan.HasValue ? showPlan.Value.ToString() : "null")}");
                        if (showPlan.HasValue)
                        {
                            if (showPlan.Value)
                            {
                                Debug.Log($"setActiveCamera for GENERAL[{planI}] because repeater {showPlan.Value}");
                                choosed = setActiveGeneralCamera(planI,characterId);
                            }
                            else
                            {
                                //setDisableGeneralCamera(planI);
                            }
                        }
                        else
                        {
                            //if(!currentActivePlanShouldShow()) GeneralPlans[planI].iterationCount++;
                            SummaryProbability += GeneralPlans[planI].calculateProbability(TotalCount, PlansListCount);
                            if (SummaryProbability >= randValue)
                            {
                                Debug.Log($"{SummaryProbability} >= {randValue}  :   GeneralPlans[{planI}]");
                                Debug.Log($"{ActiveGeneral.HasValue} && ({(!ActiveGeneral.HasValue ? "null" : !ActiveGeneral.Value)} || {planI != ActivePlanIndex}");
                                choosed = setActiveGeneralCamera(planI, characterId,
                                    ActiveGeneral.HasValue && (!ActiveGeneral.Value || planI != ActivePlanIndex)
                                );
                            }
                        }
                    }
                }
                for (int planI = 0; !choosed && planI < CharactersPlans.Count; planI++)
                {
                    //if (choosed) setDisableCharacterCamera(planI, characterId);
                    if (!choosed)
                    {
                        showPlan = CharactersPlans[planI].shouldShow(planI, characterId);
                        //var shpStr = showPlan.HasValue ? showPlan.Value.ToString() : "null";
                        Debug.Log($"CharactersPlans[{planI}].sholdShow({characterId}) return {(showPlan.HasValue ? showPlan.Value.ToString() : "null")}");
                        if (showPlan.HasValue)
                        {
                            if (showPlan.Value)
                            {
                                Debug.Log($"setActiveCamera for CHARACTER[{planI}] because repeater {showPlan.Value}");
                                choosed = setActiveCharacterCamera(planI, characterId);
                            }
                            else
                            {
                                //setDisableCharacterCamera(planI, characterId);
                            }
                        }
                        else
                        {
                            //CharactersPlans[planI].IterationCount[characterId]++;
                            SummaryProbability += CharactersPlans[planI].calculateProbability(TotalCount, PlansListCount);
                            if (SummaryProbability >= randValue)
                            {
                                Debug.Log($"{SummaryProbability} >= {randValue}  :   CharactersPlans[{planI}]");
                                Debug.Log($"{ActiveGeneral.HasValue} && ({(!ActiveGeneral.HasValue ? "null" : ActiveGeneral.Value)} || {planI != ActivePlanIndex}");
                                choosed = setActiveCharacterCamera(planI, characterId,
                                    ActiveGeneral.HasValue && (ActiveGeneral.Value || planI != ActivePlanIndex)
                                );
                            }
                        }
                    }
                }
                
            //}
            //if (iter >= 1000) Debug.Log($"ITER = {iter}");
            //Debug.Log($"ITER log = {iter}");
        }
        /*
        private bool isSamePlan(int planI, bool general)
        {
            if(!ActiveGeneral.HasValue || ActiveGeneral.Value != general) return false;
            if(general) return ActivePlanIndex == null ? false : ActivePlanIndex.Value == planI;
            else return ActiveCharacterIndex == null ? false : ActiveCharacterIndex.Value == planI;
        }*/
        private bool currentActivePlanShouldShow()
        {
            if (counter == null) Debug.Log("COUNTER == null");
            if (counter == null) return false;

            var mustBeShow = counter.shouldShow(ActivePlanIndex ?? -1, ActiveCharacterIndex ?? -1);
            Debug.Log($"mustBeShow = {(mustBeShow.HasValue ? mustBeShow.Value : "null")}");
            return mustBeShow.HasValue && mustBeShow.Value;
        }
        private bool setActiveGeneralCamera(int planI, int characterId, bool resetCounter = false)
        {
            Debug.Log($"setActiveGeneralCamera() -> {(counter == null ? "counter is null" : $"current ShouldShow = {currentActivePlanShouldShow()}")}");

            //1. Если идет установка той же самой активной генеральной камеры, и текущая камера должна показаться ещё раз,
            //то метод проходит дальше
            //2. но Если это другая камера, хотя какая-то другая активная камера должна показаться, то тут мы выходим

            //в if описан 2 случай
            Debug.Log($"setActiveGeneralCamera() -> if (!({ActiveGeneral ?? false} && {planI} == {ActivePlanIndex}) && {currentActivePlanShouldShow()})");
            Debug.Log($"setActiveGeneralCamera() -> if result ({!(ActiveGeneral ?? false && planI == ActivePlanIndex)} && {currentActivePlanShouldShow()})");
            if (!(ActiveGeneral ?? false && planI == ActivePlanIndex) && currentActivePlanShouldShow()) return false;

            Debug.Log($"setActiveGeneralCamera() -> if ({resetCounter} && {counter != null})");
            if (resetCounter && counter != null) counter.resetCounter();
            counter = GeneralPlans[planI];

            //Switch Camera
            if (enabledCamera != null) enabledCamera.enabled = false;
            enabledCamera = GeneralPlans[planI].Camera;
            enabledCamera.enabled = true;

            ActivePlanIndex = planI;
            ActiveCharacterIndex = null;
            ActiveGeneral = true;

            GeneralPlans[planI].IterationCount++;
            GeneralPlans[planI].Count++;
            TotalCount++;
            return true;
        }
        private bool setActiveCharacterCamera(int planI, int characterId, bool resetCounter = false)
        {
            Debug.Log($"setActiveCharacterCamera() -> {(counter == null ? "counter is null" : $"current ShouldShow = {currentActivePlanShouldShow()}")}");
            //1. Если идет установка камеры для персонажей, а активная была общим планом, или это была другая камера для персонажей
            //И вместе с тем в этот раз активная камера должна показать план ещё раз, то мы выходим
            //2. Но если это активная камера - совпадает по индексу и по типу с этой, то мы продолжаем установку этой камеры

            //в if описан 1 случай, но если активная камера вообще не была выбрана
            Debug.Log($"setActiveCharacterCamera() -> if (({ActiveGeneral ?? false} || [{planI!=ActivePlanIndex}]{planI} != {ActivePlanIndex}) && {currentActivePlanShouldShow()})");
            Debug.Log($"setActiveCharacterCamera() -> if result ({((ActiveGeneral ?? false) || (planI != ActivePlanIndex))} && {currentActivePlanShouldShow()})");
            if (((ActiveGeneral ?? false) || (planI != ActivePlanIndex)) && currentActivePlanShouldShow()) return false;

            Debug.Log($"setActiveCharacterCamera() -> if ({resetCounter} && {counter != null})");
            if (resetCounter && counter != null) counter.resetCounter();
            counter = CharactersPlans[planI];

            //Switch Camera
            if (enabledCamera != null) enabledCamera.enabled = false;
            enabledCamera = CharactersPlans[planI].getCamera(characterId);
            enabledCamera.enabled = true;

            ActivePlanIndex = planI;
            ActiveCharacterIndex = characterId;
            ActiveGeneral = false;

            CharactersPlans[planI].IterationCount[characterId]++;
            CharactersPlans[planI].Count++;
            TotalCount++;
            return true;
        }

        public void setSettings(CamerasSettings camerasSettings)
        {
            if (camerasSettings.settingsForAllGeneralPlans != null)
                foreach (var plan in GeneralPlans) {
                    plan.setSettings(camerasSettings.settingsForAllGeneralPlans);                    
                }
            if (camerasSettings.settingsForAllCharactersPlans != null)
                foreach (var plan in CharactersPlans) {
                    plan.setSettings(camerasSettings.settingsForAllCharactersPlans);                    
                }

            if (camerasSettings.generalPlans != null)
                foreach (var planSettings in camerasSettings.generalPlans)
                    if (planSettings.index < GeneralPlans.Count)
                        GeneralPlans[planSettings.index].setSettings(planSettings);
            if (camerasSettings.charactersPlans != null)
                foreach (var planSettings in camerasSettings.charactersPlans)
                    if (planSettings.index < CharactersPlans.Count)
                        CharactersPlans[planSettings.index].setSettings(planSettings);
        }
    }
    #region Plan Classes
    [Serializable]
    public class GeneralPlan : IPriority, IProbabilitySolver, IShowsCounter
    {
        public Camera Camera;
        internal void setSettings(PlanSettings planSettings)
        {
            maximumIterations = planSettings.maximumIterations;
            minimumIterations = planSettings.minimumIterations;
            if (planSettings.priority != -1) Priority = planSettings.priority;
        }

        #region ShowsCounter
        //[SerializeField]
        private int iterationCount = 0;
        public int IterationCount { get => iterationCount; set => iterationCount = value; }
        public int maximumIterations { get; set; }
        public int minimumIterations { get; set; }
        public bool? shouldShow(int planI, int characterId = -1)
        {
            var inst = PlansInfo.instance;
            if (!inst.ActiveGeneral.HasValue || !inst.ActivePlanIndex.HasValue) return null;
            if (!inst.ActiveGeneral.Value) return null;
            if (inst.ActivePlanIndex != planI) return null;
            //else if (inst.ActivePlanIndex == planI)
            //{
            if (IterationCount > maximumIterations)
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
            //}
        }
        public void setIterations(int minimum, int maximum)
        {
            maximumIterations = maximum;
            minimumIterations = minimum;
        }
        public void resetCounter() {
            IterationCount = 0;
        }
        #endregion
        #region ProbabilitySolver
        //[SerializeField] 
        private float result;
        //[SerializeField] 
        private float _showRarityCoef;
        //[SerializeField] 
        private int _count = 0;
        public int Count { get => _count; set => _count = value; }
        public float RepeaterCoef { get => (maximumIterations - IterationCount) / maximumIterations; }
        //НЕ ИСПОЛЬЗУЕМАЯ ФИЧА:
        //ЧТО ЕСЛИ ПРИ ПОДСЧЕТЕ ВЕРОЯТНОСТИ ИСПОЛЬЗОВАНИЯ ПЛАНА
        //СЧИТАТЬ СКОЛЬКО ИТЕРАЦИЙ ОТ МАКСИМУМА УЖЕ БЫЛО И ДОМНОЖАТЬ НА ЭТОТ КОЭФФИЦЕНТ
        //НАПРИМЕР ПЛАН УЖЕ БЫЛ ПОКАЗАН 3 раза ПОДРЯД, (iter = 3), а МАКСИМУМ 10
        //ТОГДА REPEATER = (10-3)/10 = 0.7
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
        [SerializeField] private int _priority;
        public int Priority { get => _priority; set => _priority = value; }
        public float Probability { get; set; }
        //public float Probability { get => _probability; set => _probability = value; }
        //[SerializeField] private float _probability;
        #endregion
    }
    [Serializable]
    public class CharactersPlan : IPriority, IProbabilitySolver, IShowsCounter
    {
        internal Camera getCamera(int characterId) => Cameras[characterId];
        public List<Camera> Cameras;
        internal void setSettings(PlanSettings planSettings)
        {
            maximumIterations = planSettings.maximumIterations;
            minimumIterations = planSettings.minimumIterations;
            if (planSettings.priority != -1) Priority = planSettings.priority;
        }


        #region ShowsCounter
        //[SerializeField]
        private List<int> _iterationCount = new List<int>();
        public List<int> IterationCount
        {
            get
            {
                if (_iterationCount == null) _iterationCount = new List<int>();
                if (Cameras == null) Cameras = new List<Camera>();

                if (_iterationCount.Count != Cameras.Count)
                {
                    _iterationCount.Clear();
                    Cameras.ForEach(c => _iterationCount.Add(0));
                }
                return _iterationCount;
            }
        }
        private int maximumIterations { get; set; }
        private int minimumIterations { get; set; }
        public bool? shouldShow(int planI, int characterId)
        {
            var inst = PlansInfo.instance;
            if (!inst.ActiveGeneral.HasValue || !inst.ActiveCharacterIndex.HasValue) return null;
            if (inst.ActiveGeneral.Value) return null;
            if (inst.ActivePlanIndex != planI) return null;
            if (characterId >= IterationCount.Count || characterId < 0) return null;
            //else if (inst.ActivePlanIndex == planI)
            //{
            if (IterationCount[characterId] > maximumIterations)
            {
                Debug.Log($"counter?.shouldShow() -> IterationCount[{characterId}] : {IterationCount[characterId]} > {maximumIterations}");
                IterationCount[characterId] = 0;
                return false;
            }
            if (IterationCount[characterId] < minimumIterations)
            {
                //if (incrementCounter) IterationCount[characterId]++;
                return true;
            }
            return null;
            //}
        }
        public void setIterations(int minimum, int maximum)
        {
            maximumIterations = maximum;
            minimumIterations = minimum;
        }
        public void resetCounter() {
            //var lastCharIndex = PlansInfo.instance.ActiveCharacterIndex;
            //if (lastCharIndex.HasValue && _iterationCount[lastCharIndex.Value] < minimumIterations) return false;
            //foreach(var iter in IterationCount)
            //    if (iter < minimumIterations) return false;
            _iterationCount.Clear();
            //return true;
        }
        #endregion
        #region ProbabilitySolver
        //[SerializeField] 
        private float result;
        //[SerializeField] 
        private float _showRarityCoef;
        //[SerializeField] 
        private int _count = 0;
        public int Count { get => _count; set => _count = value; }
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
        [SerializeField] private int _priority;
        public int Priority { get => _priority; set => _priority = value; }
        public float Probability { get; set; }
        //public float Probability { get => _probability; set => _probability = value; }
        //[SerializeField] private float _probability;
        #endregion
    }
    #endregion
    #region IShowsCounter
    public interface IShowsCounter
    {
        //public int maximumIterations { get; set; }//Включительно
        //public int minimumIterations { get; set; }//Включительно
        public bool? shouldShow(int planI, int characterId);
        public void setIterations(int minimum, int maximum);
        public void resetCounter();
    }
    #endregion
    #region IProbabilitySolver
    internal interface IProbabilitySolver
    {
        public int Count { get; set; }
        public float calculateProbability(int total, int listCount);
    }
    #endregion
    #region IPriority
    internal interface IPriority
    {
        public int Priority { get; set; }
        public float Probability { get; set; }
        public void setProbability(int total = 0) => Probability = total == 0 ? 0.0f : Priority / (float)total;
    }
    #endregion
}
