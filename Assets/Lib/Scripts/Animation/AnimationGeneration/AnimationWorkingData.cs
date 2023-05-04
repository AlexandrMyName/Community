using System;
using System.Collections.Generic;

namespace AnimationsSystem
{
    [Serializable]
    public class AnimationWorkingData
    {
        public List<AnimationData> oldAnimations;
        public static AnimationData FindMissing(List<AnimationData> lesser, List<AnimationData> larger) {
            for (int i = 0; i < larger.Count; i++)
            {
                if (FindIndex(lesser, larger[i].realKey) == -1) 
                { 
                    return larger[i];
                }
            }
            return larger[larger.Count - 1];
        }
        public static int FindIndex(List<AnimationData> other, string dataRealKey) {
            for (int i = 0; i < other.Count; i++)
            {
                if (other[i].realKey == dataRealKey) return i;
            }
            return -1;
        }

        internal void CopyAnimations(List<AnimationData> idleAnimations)
        {
            oldAnimations.Clear();
            idleAnimations.ForEach(animation =>
            {
                oldAnimations.Add(animation.Copy());
            });
        }
    }
}