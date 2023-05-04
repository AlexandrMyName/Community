using UnityEngine;

namespace Client
{
    public class MessageAnimationInfo
    {
        public string animationKeyName { get; set; }
        public bool sayAfterTransition { get; set; }
        public System.Collections.Generic.List<TransitionInfo> transitionsInfo { get; set; }
    }

    public class TransitionInfo
    {
        public Vect3 vect3 { get; set; }
        public Quat quat { get; set; }
        public double transitionTime { get; set; }
        public bool withNormalSpeed { get; set; }
    }

    public struct Vect3
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public Vector3 toVector3() => new Vector3((float)x, (float)y, (float)z);
    }
    public struct Quat
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public double w { get; set; }
        public Quaternion toVector3() => new Quaternion((float)x, (float)y, (float)z, (float)w);
    }
}
