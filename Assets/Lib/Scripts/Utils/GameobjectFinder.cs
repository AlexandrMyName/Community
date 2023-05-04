using UnityEngine;

namespace Utils
{
    class GameobjectFinder
    {
        public static Transform FindObject(Transform root, string nameKey)
        {
            Transform finded;
            for (int i = 0; i < root.childCount; i++) {
                finded = root.GetChild(i);
                if (finded.name == nameKey) return finded;
            }
            for (int i = 0; i < root.childCount; i++) {
                finded = root.GetChild(i);
                if (finded.childCount > 0) { 
                    finded = FindObject(finded, nameKey);
                    if (finded != null) return finded;
                }
            }
            return null;            
        }
    }
}