using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using static GentlyUI.UIElements.GMVisualElement;
using System;
using GentlyUI.Core;
using Uween;
using static GentlyUI.UIElements.GMAnimatable;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Selectable", 0)]
    public class GMSelectable : UIBase, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IUITickable, IPooledUIResetter {
        /// <summary>
        /// List of all the selectable objects currently active in the scene.
        /// <para>Example: Use this list for handling navigation events instead of getting all objects with component Selectable in the scene.</para>
        /// </summary>
        private static List<GMSelectable> allSelectables = new List<GMSelectable>();
        public static List<GMSelectable> AllSelectables => allSelectables;

        [SerializeField]
        private Navigation navigation = new Navigation() {
            mode = Navigation.Mode.None
        };

        [SerializeField]
        [Tooltip("Whether or not this Selectable should be interactable.")]
        private bool interactable = true;
        /// <summary>
        /// Defines whether interaction is not allowed due to any parent canvas group having interactable set to false.
        /// </summary>
        private bool groupsAllowInteraction = true;

        public virtual bool Interactable {
            get { return interactable && groupsAllowInteraction; }
            set {
                SetInteractable(value);
            }
        }

        [SerializeField]
        [Tooltip("A list of child visual elements that get visually manipulated for different states of this ui element.")]
        private List<GMVisualElement> visualElements;
        /// <summary>
        /// A list of child visual elements that get visually manipulated for different states of this ui element.
        /// </summary>
        [Tooltip("A list of child visual elements that get visually manipulated for different states of this ui element.")]
        public List<GMVisualElement> VisualElements => visualElements;

        protected bool isPointerInside;
        private bool isPointerDown;

        protected VisualState currentVisualState;

        /// <summary>
        /// Defines whether warning color is used.
        /// </summary>
        private bool warningActive = false;
        public bool WarningActive => warningActive;

        private GMPooledScrollView parentScrollView;

        protected override void OnInitialize() {
            base.OnInitialize();

            if (visualElements != null) {
                //Initialize visual elements
                for (int i = 0, count = visualElements.Count; i < count; ++i) {
                    visualElements[i].Initialize(this);
                }
            }
        }

        protected override void OnEnable() {
            base.OnEnable();

            //Add to global list of selectables
            allSelectables.Add(this);
            //Check if this selectbale is part of a pooled scroll view
            parentScrollView = GetComponentInParent<GMPooledScrollView>();
            //Update visual state
            UpdateVisualState(true);
        }

        protected override void OnDisable() {
            //Remove from global list of selectables
            allSelectables.Remove(this);

            OnPointerExit(UIManager.Instance.GetCurrentPointerEventData());

            base.OnDisable();
        }

        public void SetInteractable(bool isInteractable, bool forceUpdate = false) {
            bool valueChanged = !interactable.Equals(isInteractable);
            interactable = isInteractable;

            if (valueChanged || forceUpdate) {
                UpdateVisualState(forceUpdate);
            }
        }

        public virtual void Tick(float unscaledDeltaTime) {
            if (visualElements == null)
                return;

            for (int i = 0, count = visualElements.Count; i < count; ++i) {
                GMVisualElement visualElement = visualElements[i];
                if (visualElement.UpdateTweens) visualElement.DoTweenUpdate();
            }
        }

        /// <summary>
        /// Sets this ui element as the currently selected one in the whole UI.
        /// </summary>
        public virtual void Select() {
            UIManager.Instance.SelectUI(gameObject);
        }

        private readonly List<CanvasGroup> canvasGroupCache = new List<CanvasGroup>();

        protected override void OnCanvasGroupChanged() {
            //Check if parent canvas groups allow interaction.
            bool interactionAllowed = true;
            Transform t = transform;
            //Loop through all parents and check if any canvas group doesn't allow interaction.
            //Stop looping through parents if any canvas group tells us to ignore parent groups.
            while (t != null) {
                t.GetComponents(canvasGroupCache);

                for (int i = 0, count = canvasGroupCache.Count; i < count; ++i) {
                    CanvasGroup currentGroup = canvasGroupCache[i];
                    interactionAllowed = currentGroup.interactable;

                    if (currentGroup.ignoreParentGroups || !interactionAllowed) break;
                }

                t = t.parent;
            }

            if (groupsAllowInteraction != interactionAllowed) {
                groupsAllowInteraction = interactionAllowed;
                OnSetProperty();
            }
        }

        public void UpdateVisualState(bool setImmediately = false) {
            if (visualElements == null || visualElements.Count == 0)
                return;

            if (Interactable) {
                if (IsPressed()) {
                    SetPressedState();
                } else if (IsHovered()) {
                    currentVisualState = VisualState.Hovered;
                } else {
                    SetDefaultState();
                }
            } else {
                currentVisualState = VisualState.Disabled;
            }

            UpdateVisualElementStates(setImmediately);
        }

        protected virtual void SetDefaultState() {
            currentVisualState = VisualState.Default;
        }

        private bool IsScrolling() {
            return parentScrollView != null && parentScrollView.IsScrolling;
        }

        protected virtual void SetPressedState() {
            currentVisualState = VisualState.Pressed;
        }

        protected void SetVisualState(VisualState state, bool setImmediately = false) {
            currentVisualState = state;

            UpdateVisualElementStates(setImmediately);
        }

        void UpdateVisualElementStates(bool setImmediately = false) {
            for (int i = 0, count = visualElements.Count; i < count; ++i) {
                GMVisualElement e = visualElements[i];
                e.SetState(currentVisualState, setImmediately);
            }
        }

        /// <summary>
        /// Callback that should be called for every property change that should trigger a visual change to the selectable.
        /// </summary>
        void OnSetProperty() {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateVisualState(true);
            else
#endif

                UpdateVisualState();
        }

        public virtual void OnPointerDown(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (Interactable && navigation.mode != Navigation.Mode.None) {
                //TODO: Replace with global method in service later
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);
            }

            isPointerDown = true;
            UpdateVisualState();
        }

        public virtual void OnPointerUp(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            isPointerDown = false;

            UpdateVisualState();
        }

        public virtual void OnPointerEnter(PointerEventData eventData) {
            isPointerInside = true;
            UpdateVisualState();
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            isPointerInside = false;
            UpdateVisualState();
        }

        protected bool IsPressed() {
            if (!IsActive() || IsScrolling())
                return false;

            return isPointerInside && isPointerDown;
        }

        protected bool IsHovered() {
            if (!IsActive() || IsScrolling())
                return false;

            return isPointerInside;
        }

        public void ToggleWarning(bool activateWarning) {
            warningActive = activateWarning;

            if (gameObject.activeInHierarchy) {
                SetVisualState(currentVisualState, true);
            }
        }

        private List<Type> addedComponents;


        /// <summary>
        /// Adds a component to this selectable that is automatically destroyed if this element is returned by an object pool.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>The component.</returns>
        public T AddReturnableComponent<T>() where T : MonoBehaviour {
            if (addedComponents == null) {
                addedComponents = new List<Type>();
            } else if (addedComponents.Contains(typeof(T))) {
                return gameObject.GetComponent<T>();
            }

            T c = gameObject.AddComponent<T>();
            addedComponents.Add(typeof(T));
            return c;
        }

        public virtual void CreatePooledUICache() { }

        public virtual void ResetPooledUI() {
            ToggleWarning(false);
            SetInteractable(true);

            //Remove dynamically added components on return
            if (addedComponents != null) {
                for (int i = 0, count = addedComponents.Count; i < count; ++i) {
                    Type c = addedComponents[i];
                    Destroy(GetComponent(c));
                }

                addedComponents.Clear();
                addedComponents = null;
            }
        }
    }
}
