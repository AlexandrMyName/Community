using Newtonsoft.Json;

namespace Client
{
    public struct CallbackAnimation : IBaseCallback
    {
        public CallbackAnimation(int characterId, EventStatus status)
        {
            this.characterId = characterId;
            this.animationStatus = status;
            callbackKey = "CallbackAnimation";
        }
        public string callbackKey { get; set; }

        public int characterId { get; set; }
        public EventStatus animationStatus { get; set; }
        public string toJson() => JsonConvert.SerializeObject(this,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
    }

    public struct CallbackAudio : IBaseCallback
    {
        public CallbackAudio(EventStatus status)
        {
            this.audioStatus = status;
            callbackKey = "CallbackAudio";
        }
        public string callbackKey { get; set; }
        public EventStatus audioStatus { get; set; }
        public string toJson() => JsonConvert.SerializeObject(this,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
    }
    public struct CallbackLoader : IBaseCallback
    {
        public CallbackLoader(LoaderStatus status)
        {
            this.status = status;
            callbackKey = "CallbackLoader";
        }

        public LoaderStatus status { get; set; }
        public string callbackKey { get; set; }
        public enum LoaderStatus { loading, loaded, showWindow, unloadWindow, quitWindow }
        public string toJson() => JsonConvert.SerializeObject(this,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
    }
    public enum EventStatus { 
        end, playStart, playing, pause, stop
    }
    public interface IBaseCallback {
        public string callbackKey { get; set; }
    }
}
