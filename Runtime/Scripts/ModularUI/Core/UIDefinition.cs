using GentlyUI.UIElements;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        /// <summary>
        /// The root container in which the UI Definition will be spawned.
        /// </summary>
        protected UIDefinitionContainer rootContainer;
        /// <summary>
        /// The current container elements are spawned into. This could change e.g. if a content ui was spawned.
        /// </summary>
        protected  RectTransform currentContainer;
        /// <summary>
        /// If the current container is a toggle group it will be cached here.
        /// </summary>
        protected GMToggleGroup currentToggleGroup;
        /// <summary>
        /// The current hierarchy order in the current container at which the next ui element will be spawned
        /// </summary>
        protected int currentHierarchyOrder = 0;
        /// <summary>
        /// Cache for objects and their return actions once the definition is disposed.
        /// </summary>
        private Dictionary<GameObject, Action> uiObjects = new Dictionary<GameObject, Action>();
        /// <summary>
        /// A list of dynamic layouts that will all be destroyed when the definition is dispoed.
        /// </summary>
        private List<GameObject> dynamicLayouts = new List<GameObject>();

        public UIDefinition(UIDefinitionContainer container, object data = null) {
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
        public virtual void PostCreateUI(object dada = null) { }

        /// <summary>
        /// Creates the UI within the passed container.
        /// </summary>
        public abstract void CreateUI(object data = null);

        public void DisposeUI() { 
            foreach(KeyValuePair<GameObject, Action> ui in uiObjects) {
                ui.Value();
            }

            uiObjects.Clear();

            for (int i = 0, count = dynamicLayouts.Count; i < count; ++i) {
                GameObject layout = dynamicLayouts[i];
                layout.transform.DetachChildren();
                GameObject.Destroy(layout.gameObject);
            }

            dynamicLayouts.Clear();

            OnDispose();
        }

        /// <summary>
        /// Called after the UI Definition was disposed (removed) from its container.
        /// </summary>
        public virtual void OnDispose() { }

        private Dictionary<RectTransform, int> hierarchyOrderLUT = new Dictionary<RectTransform, int>();

        protected void SetCurrentContainer(RectTransform container) {
            //Cache hierarchy order of current container
            if (currentContainer != null) {
                hierarchyOrderLUT[currentContainer] = currentHierarchyOrder;
            }

            currentContainer = container;

            //Set current toggle group if the container has one.
            //It is allowed to set currentToggleGroup to null if no toggle group component can be found on the container.
            currentToggleGroup = currentContainer.GetComponent<GMToggleGroup>();

            if (hierarchyOrderLUT.ContainsKey(container)) {
                currentHierarchyOrder = hierarchyOrderLUT[container];    
            } else {
                hierarchyOrderLUT.Add(container, 0);
                currentHierarchyOrder = 0;
            }
        }

        protected  void LeaveCurrentContainer() {
            currentToggleGroup = null;

            if (currentContainer != rootContainer) {
                hierarchyOrderLUT.Remove(currentContainer);
                SetCurrentContainer(currentContainer.transform.parent as RectTransform);
            }
        }

        protected void IncrementCurrentHierarchyOrder() {
            ++currentHierarchyOrder;
        }

        /// <summary>
        /// Caches the ui object and a return action that is invoked once the definition is diposed.
        /// </summary>
        protected void CacheUIObject(GameObject go, Action returnAction) {
            uiObjects.Add(go, returnAction);
        }

        protected void CacheDynamicLayout(GameObject go) {
            dynamicLayouts.Add(go);
        }
    }
}
