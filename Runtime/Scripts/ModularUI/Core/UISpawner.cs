using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

namespace GentlyUI.ModularUI {
    /// <summary>
    /// Spawns UI in a pool for a specific type of ui behaviour (button, toggle etc.)
    /// </summary>
    /// <typeparam name="T">The ui behaviour type.</typeparam>
    public static class UISpawner<T> where T : Behaviour {
        private static Dictionary<T, string> uiObjects = new Dictionary<T, string>();
        private static Dictionary<string, Core.UIObjectPool<T>> pools = new Dictionary<string, Core.UIObjectPool<T>>();

        /// <summary>
        /// Immediately spawns an ui from path.
        /// Use carefully since using this method a lot on a single frame could have performance impact due to too many ui elements being instantiated.
        /// </summary>
        public static T SpawnUI(string path, RectTransform container) {
            return SpawnPrefabFromPath(path, container);

        }

        public static void ReturnUI(T ui) {
            string poolID = uiObjects[ui];
            pools[poolID].Return(ui.GetComponent<T>());
        }

        private static T SpawnPrefabFromPath(string path, RectTransform container) {
            if (!pools.ContainsKey(path)) {
                //Load prefab
                GameObject prefab = UIPrefabLoader.LoadFromPath(path);
                //Create pool
                Core.UIObjectPool<T> pool = new Core.UIObjectPool<T>(
                    //Spawn
                    () => {
                        GameObject ui = GameObject.Instantiate(prefab, container, false);
                        T component = ui.GetComponent<T>();
                        return component;
                    },
                    //Get
                    (T t) => {
                        t.transform.SetAsLastSibling();
                    }
                );

                pools.Add(path, pool);
            }

            //Spawn
            T ui = pools[path].Get(container);
            //Cache the ui and the pool (path) it belongs to.
            if (!uiObjects.ContainsKey(ui)) uiObjects.Add(ui, path);
            //Return
            return ui;
        }
    }
}
