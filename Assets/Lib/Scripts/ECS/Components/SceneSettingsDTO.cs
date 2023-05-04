using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Client
{
    public struct SceneSettings
    {
        public static SceneSettings DeserializeJson(string json) => JsonConvert.DeserializeObject<SceneSettings>(json, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        public void Copy(SceneSettings settings)
        {
            sceneName = settings.sceneName;
            cameras = new CamerasSettings();
            cameras.Copy(settings.cameras);
        }

        public string sceneName { get; set; }
        public CamerasSettings cameras { get; set; }
    }

    public struct CamerasSettings
    {
        public PlanSettings settingsForAllGeneralPlans { get; set; }
        public PlanSettings settingsForAllCharactersPlans { get; set; }
        public List<PlanSettings> generalPlans { get; set; }
        public List<PlanSettings> charactersPlans { get; set; }

        public void Copy(CamerasSettings cameras)
        {
            settingsForAllCharactersPlans = cameras.settingsForAllCharactersPlans;
            settingsForAllGeneralPlans = cameras.settingsForAllGeneralPlans;
            generalPlans = cameras.generalPlans;
            charactersPlans = cameras.charactersPlans;
        }
    }
    public class PlanSettings
    {
        public int index { get; set; }
        public int priority { get; set; }
        public int maximumIterations { get; set; }
        public int minimumIterations { get; set; }
    }
}
