using System.IO;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public static class UIPrefabLoader
    {
        public static GameObject LoadUIElement(string elementName) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, elementName);
            return LoadFromPath(path);
        }

        public static GameObject LoadUILayout(string layoutName) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.LayoutPath, layoutName);
            return LoadFromPath(path);
        }

        public static GameObject LoadFromPath(string path) {
            //Load prefab
            GameObject prefab = Resources.Load(path) as GameObject;
            return prefab;
        }
    }
}
