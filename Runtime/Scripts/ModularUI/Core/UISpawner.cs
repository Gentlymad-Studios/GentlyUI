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
        private static Dictionary<GameObject, string> uiObjects = new Dictionary<GameObject, string>();

        private static List<SpawnData<T>> spawnOrder = new List<SpawnData<T>>();

        private static List<GameObject> uiToReturn = new List<GameObject>();

        private static Dictionary<string, Core.UIObjectPool<T>> pools = new Dictionary<string, Core.UIObjectPool<T>>();

        private static Coroutine processingRoutine;

        /// <summary>
        /// How many UI elements should be processed (return/spawn) on a single frame?
        /// </summary>
        private const int maxProcessesPerFrame = 50;
        private static int processCounter = 0;

        private static bool doProcessing = false;
        private static bool initialized = false;

        public static void RegisterUIForSpawn(string path, RectTransform container, Action<T> onSpawn = null, int siblingIndex = -1) {
            SpawnData<T> spawnData = new SpawnData<T>(path, container, onSpawn, siblingIndex);
            spawnOrder.Add(spawnData);

            StartProcessingRoutine();
        }

        /// <summary>
        /// Cancels spawning of ui that is pending for spawn.
        /// This should mostly be used to cancel the spawn of a specific element from a UI definition in a certain container.
        /// </summary>
        /// <param name="container">The container for which all ui spawning should be canceled.</param>
        public static void CancelUISpawnForContainer(RectTransform container) {
            for (int i = spawnOrder.Count - 1; i >= 0; --i) {
                if (spawnOrder[i].Container == container) {
                    spawnOrder.RemoveAt(i);
                }
            }
        }

        static void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
            doProcessing = true;
        }

        static void OnPreRender(Camera cam) {
            if (IsLayerRendered(cam, LayerMask.NameToLayer("UI"))) {
                doProcessing = true;
            }
        }

        static bool IsLayerRendered(Camera cam, int layer) {
            return (cam.cullingMask & (1 << layer)) != 0;
        }

        public static void RegisterUIForReturn(T ui) {
            if (uiObjects.ContainsKey(ui.gameObject) && !uiToReturn.Contains(ui.gameObject)) {
                uiToReturn.Add(ui.gameObject);
            }

            StartProcessingRoutine();
        }

        public static IEnumerator Process() {
            //Wait for end of frame to make sure all UIs that should be spawned on this frame are registered.
            yield return new WaitUntil(() => doProcessing);

            //Return
            while (uiToReturn.Count > 0) {
                GameObject ui = uiToReturn[0];
                ReturnUI(ui);
                //Remove from ui that should be returned
                uiToReturn.RemoveAt(0);

                ++processCounter;

                if (processCounter >= maxProcessesPerFrame) {
                    processCounter = 0;
                    doProcessing = false;
                    yield return null;
                }
            }

            //Spawn
            while (spawnOrder.Count > 0) {
                //Get path from spawn order
                SpawnData<T> spawnData = spawnOrder[0];
                //Spawn UI
                T ui = SpawnPrefabFromPath(spawnData.Path, spawnData.Container, spawnData.SiblingIndex);
                //Trigger callback if we have one
                if (spawnData.Callback != null) spawnData.Callback.Invoke(ui);
                //Remove from spawn order and remove callbacks
                spawnOrder.RemoveAt(0);

                ++processCounter;

                if (processCounter >= maxProcessesPerFrame) {
                    processCounter = 0;
                    doProcessing = false;
                    yield return null;
                }
            }

            //Reset coroutine
            ResetProcessingRoutine();
        }

        static void StartProcessingRoutine() {
            if (!initialized) {
                RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
                Camera.onPreRender += OnPreRender;
                initialized = true;
            }

            //Start processing routine if not running
            if (processingRoutine == null) {
                processingRoutine = UIManager.Instance.StartCoroutine(Process());
            }
        }

        static void ResetProcessingRoutine() {
            processingRoutine = null;
            processCounter = 0;
        }

        /// <summary>
        /// Immediately spawns an ui from path.
        /// Use carefully since using this method a lot on a single frame could have performance impact due to too many ui elements being instantiated.
        /// </summary>
        public static T SpawnImmediately(string path, RectTransform container, int siblingIndex) {
            //Increase process counter 
            ++processCounter;

            return SpawnPrefabFromPath(path, container, siblingIndex);

        }

        public static void ReturnImmediately(T ui) {
            //Increase process counter 
            ++processCounter;

            ReturnUI(ui.gameObject);
        }

        private static void ReturnUI(GameObject ui) {
            string poolID = uiObjects[ui.gameObject];
            pools[poolID].Return(ui.GetComponent<T>());
        }

        private static T SpawnPrefabFromPath(string path, RectTransform container, int siblingIndex) {
            if (!pools.ContainsKey(path)) {
                //Load prefab
                GameObject prefab = UIPrefabLoader.LoadFromPath(path);
                //Create pool
                Core.UIObjectPool<T> pool = new Core.UIObjectPool<T>(
                    //Spawn
                    () => {
                        GameObject ui = GameObject.Instantiate(prefab);
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
            //Sibling index
            if (siblingIndex > -1) ui.transform.SetSiblingIndex(siblingIndex, true);
            //Cache the ui and the pool (path) it belongs to.
            if (!uiObjects.ContainsKey(ui.gameObject)) uiObjects.Add(ui.gameObject, path);
            //Return
            return ui;
        }

        public class SpawnData<DataT> where DataT : Behaviour {
            private string path;
            public string Path => path;

            private RectTransform container;
            public RectTransform Container => container;
            private Action<DataT> callback;
            public Action<DataT> Callback => callback;

            private int siblingIndex;
            public int SiblingIndex => siblingIndex;

            public SpawnData(string path, RectTransform container, Action<DataT> callback, int siblingIndex) {
                this.path = path;
                this.container = container;
                this.callback = callback;
                this.siblingIndex = siblingIndex;
            }
        }
    }
}
