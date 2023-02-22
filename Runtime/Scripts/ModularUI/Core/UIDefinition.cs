using GentlyUI.UIElements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        /// <summary>
        /// The root container in which the UI Definition will be spawned.
        /// </summary>
        protected UIDefinitionContainer rootContainer;
        /// <summary>
        /// The current container elements are spawned into. This could change e.g. if a content ui was spawned.
        /// </summary>
        protected RectTransform currentContainer;
        /// <summary>
        /// If the current container is a toggle group it will be cached here.
        /// </summary>
        protected GMToggleGroup currentToggleGroup;
        /// <summary>
        /// Cache for objects and their return actions once the definition is disposed.
        /// </summary>
        private Dictionary<GameObject, Action> uiObjects;
        /// <summary>
        /// A list of dynamic layouts that will all be destroyed when the definition is dispoed.
        /// </summary>
        private List<GameObject> dynamicLayouts;

        public UIDefinition(UIDefinitionContainer container, object data = null) {
            dynamicLayouts = ListPool<GameObject>.Get();
            uiObjects = DictionaryPool<GameObject, Action>.Get();

            rootContainer = container;
            SetCurrentContainer(rootContainer.RectTransform);
            //Call Pre Create UI
            PreCreateUI(data);
            //Create UI
            CreateUI(data);
            //Post Create UI
            PostCreateUI(data);
        }

        /// <summary>
        /// Called before the UI is created.
        /// </summary>
        public virtual void PreCreateUI(object data = null) { }
        /// <summary>
        /// Called after the UI was created.
        /// </summary>
        public virtual void PostCreateUI(object data = null) { }

        /// <summary>
        /// Send a data object to this ui definition e.g. to update it without respawning.
        /// </summary>
        /// <param name="data"></param>
        public virtual void BroadcastData(object data) { }

        /// <summary>
        /// Creates the UI within the passed container.
        /// </summary>
        public abstract void CreateUI(object data = null);

        public void DisposeUI() {
            foreach (KeyValuePair<GameObject, Action> ui in uiObjects) {
                ui.Value();
            }

            for (int i = 0, count = dynamicLayouts.Count; i < count; ++i) {
                GameObject layout = dynamicLayouts[i];
                layout.transform.DetachChildren();
                GameObject.Destroy(layout.gameObject);
            }

            ListPool<GameObject>.Release(dynamicLayouts);
            DictionaryPool<GameObject, Action>.Release(uiObjects);

            OnDispose();
        }

        /// <summary>
        /// Called after the UI Definition was disposed (removed) from its container.
        /// </summary>
        public virtual void OnDispose() { }

        protected void SetCurrentContainer(RectTransform container) {

            currentContainer = container;

            //Set current toggle group if the container has one.
            //It is allowed to set currentToggleGroup to null if no toggle group component can be found on the container.
            currentToggleGroup = currentContainer.GetComponent<GMToggleGroup>();
        }

        protected void LeaveCurrentContainer() {
            currentToggleGroup = null;

            if (currentContainer != rootContainer) {
                SetCurrentContainer(currentContainer.transform.parent as RectTransform);
            }
        }

        /// <summary>
        /// Caches the ui object and a return action that is invoked once the definition is diposed.
        /// </summary>
        protected void CacheUIObject(GameObject go, Action returnAction) {
            uiObjects.Add(go, returnAction);
        }

        /// <summary>
        /// Removes a ui element that was previously spawned in this definition.
        /// </summary>
        /// <param name="go"></param>
        protected void RemoveUIElement(GameObject go) {
            if (uiObjects.ContainsKey(go)) {
                uiObjects[go].Invoke();
                uiObjects.Remove(go);
            }
        }

        protected void CacheDynamicLayout(GameObject go) {
            dynamicLayouts.Add(go);
        }
    }
}
