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
    public class GMSelectable : UIBase, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IUITickable {
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
        [UnityEngine.Tooltip("A list of child visual elements that get visually manipulated for different states of this ui element.")]
        public List<GMVisualElement> VisualElements => visualElements;

        protected bool isPointerInside;
        private bool isPointerDown;

        private VisualState currentVisualState;
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
                    currentVisualState = VisualState.Default;
                }
            } else {
                currentVisualState = VisualState.Disabled;
            }

            UpdateVisualElementStates(setImmediately);
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
    }

    /// <summary>
    /// A child of an ui element that gets visually manipulated for different ui states of its parent.
    /// </summary>
    [Serializable]
    public class GMVisualElement : Namable {
        public bool foldOut = false;

        public enum VisualState {
            Default = 0,
            Hovered = 1,
            Pressed = 2,
            Selected = 3,
            Disabled = 4,
            Inactive = 5
        }

        /// <summary>
        /// The graphic to manipulate for different states of its parent ui element.
        /// </summary>
        [Tooltip("The graphic to manipulate for different states of its parent ui element.")]
        [SerializeField] private Graphic graphic;

        /// <summary>
        /// Defines whether the raycastTarget bool on the graphic should be overwritten or not.
        /// </summary>
        [Tooltip("Defines whether the raycastTarget bool on the graphic should be overwritten or not.")]
        [SerializeField] private bool keepRaycastable = false;

        public VisualStateData defaultState;
        public VisualStateData hoveredState;
        public VisualStateData pressedState;
        public VisualStateData selectedState;
        public VisualStateData disabledState;
        public VisualStateData inactiveState;

        /// <summary>
        /// The root selectable this visual element belongs to.
        /// </summary>
        private GMSelectable rootSelectable;
        private GMSelectable RootSelectable {
            get {
                if (rootSelectable == null) rootSelectable = graphic.GetComponentInParent<GMSelectable>(true);
                return rootSelectable;
            }
        }

        /// <summary>
        /// The current state of the visual element.
        /// </summary>
        private VisualState currentState = VisualState.Default;

        private Vector2 defaultPosition;

        private bool initialized = false;
        private VisualStateData stateData;

        /// <summary>
        /// Updates this visual element to the passed state.
        /// </summary>
        /// <param name="state">The state!</param>
        /// <param name="setImmediately">Set to true if we want to go to the end values immediately. Only works if no repeatable animation is configured for this state.</param>
        public void SetState(VisualState state, bool setImmediately = false) {
            if (!initialized) {
                Initialize(RootSelectable);
            }

            if (state == currentState && !setImmediately)
                return;

            stateData = GetStateData(state);

            //Return if we don't have a state or no animation attributes are set.
            if (stateData == null || stateData.AnimationAttributes.Count == 0)
                return;

            currentState = state;

            if (setImmediately) {
                SetFinalVisualState();
            } else {
                //Start Tween or set properties immediately under certain circumstances
                updateTweens = true;
            }
        }

        VisualStateData GetStateData(VisualState state) {
            switch (state) {
                case VisualState.Default:
                    return defaultState;
                case VisualState.Hovered:
                    return hoveredState;
                case VisualState.Pressed:
                    return pressedState;
                case VisualState.Disabled:
                    return disabledState;
                case VisualState.Selected:
                    return selectedState;
                case VisualState.Inactive:
                    return inactiveState;
                default:
                    return null;
            }
        }

        bool updateTweens = false;
        public bool UpdateTweens => updateTweens;

        public void SetFinalVisualState() {
            graphic.gameObject.PauseTweens();
            updateTweens = false;

            //Position
            VisualElementAnimationAttributes posAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.PositionOffset);
            if (posAnimAttributes != null) {
                Vector2 finalPosition = defaultPosition + stateData.PositionOffset;
                graphic.rectTransform.anchoredPosition = new Vector3(finalPosition.x, finalPosition.y, 0);
            }

            //Scale
            VisualElementAnimationAttributes scaleAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Scale);
            if (scaleAnimAttributes != null) {
                Vector3 scale = stateData.Scale;
                scale.z = 1;
                graphic.transform.localScale = scale;
            }

            //Rotation
            VisualElementAnimationAttributes rotAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Rotation);
            if (rotAnimAttributes != null) {
                graphic.transform.localRotation = Quaternion.Euler(Vector3.forward * stateData.Rotation);
            }

            //Color
            VisualElementAnimationAttributes colAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Color);
            if (colAnimAttributes != null) {
                Color color = RootSelectable.WarningActive ? UIManager.UISettings.GetColor(stateData.WarningGlobalColor) : UIManager.UISettings.GetColor(stateData.GlobalColor);
                graphic.color = color;
            }

            //Alpha
            VisualElementAnimationAttributes alphaAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Alpha);
            if (alphaAnimAttributes != null) {
                graphic.canvasRenderer.SetAlpha(stateData.Alpha);
            }

            //Active state
            if (graphic.gameObject.activeSelf != stateData.ShowElement) graphic.gameObject.SetActive(stateData.ShowElement);
        }

        public void DoTweenUpdate() {
            updateTweens = false;

            //Check if the visual element should simply be disabled
            if (currentState != VisualState.Inactive) {
                graphic.gameObject.SetActive(stateData.ShowElement);

                //We don't need to do anything else if the visual element is disabled
                if (!graphic.gameObject.activeSelf) {
                    return;
                }
            }

            //Start Tweens
            Tween tween;
            //Cache tween with longest duration to disable the element when animation finished if the state is VisualState.Inactive
            Tween longestTween = null;

            //Position
            VisualElementAnimationAttributes posAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.PositionOffset);
            if (posAnimAttributes != null) {
                tween = TweenXY.Add(graphic.gameObject, posAnimAttributes.Duration, defaultPosition + stateData.PositionOffset);
                SetupTween(tween, posAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Scale
            VisualElementAnimationAttributes scaleAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Scale);
            if (scaleAnimAttributes != null) {
                tween = TweenSXY.Add(graphic.gameObject, scaleAnimAttributes.Duration, stateData.Scale);
                SetupTween(tween, scaleAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Rotation
            VisualElementAnimationAttributes rotAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Rotation);
            if (rotAnimAttributes != null) {
                tween = TweenRZ.Add(graphic.gameObject, rotAnimAttributes.Duration, stateData.Rotation);
                SetupTween(tween, rotAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Color
            VisualElementAnimationAttributes colAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Color);
            if (colAnimAttributes != null) {
                Color color = RootSelectable.WarningActive ? UIManager.UISettings.GetColor(stateData.WarningGlobalColor) : UIManager.UISettings.GetColor(stateData.GlobalColor);
                tween = TweenC.Add(graphic.gameObject, colAnimAttributes.Duration, color);
                SetupTween(tween, colAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Alpha
            VisualElementAnimationAttributes alphaAnimAttributes = stateData.GetAnimationAttributes(AnimationProperty.Alpha);
            if (alphaAnimAttributes != null) {
                tween = TweenA.Add(graphic.gameObject, alphaAnimAttributes.Duration, stateData.Alpha);
                SetupTween(tween, alphaAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Longest tween callback
            if (longestTween != null) {
                if (currentState == VisualState.Inactive && !stateData.ShowElement) {
                    longestTween.OnComplete += () => graphic.gameObject.SetActive(false);
                }
            } else if (currentState == VisualState.Inactive && !stateData.ShowElement) {
                graphic.gameObject.SetActive(false);
            }
        }

        void CacheLongestTween(Tween tween, ref Tween longestTweenCache) {
            if (longestTweenCache == null) {
                longestTweenCache = tween;
            } else {
                float currentLongestTweenDuration = longestTweenCache.Duration + longestTweenCache.DelayTime;
                float tweenDuration = tween.Duration + tween.DelayTime;

                if (tweenDuration > currentLongestTweenDuration) longestTweenCache = tween;
            }
        }

        void SetupTween(Tween tween, VisualElementAnimationAttributes animAttributes) {
            if (animAttributes.Easing == EasingType.CustomCurve) {
                bool useDiff =
                    animAttributes.PropertyTypeToAnimate.HasFlag(AnimationProperty.PositionOffset) ||
                    animAttributes.PropertyTypeToAnimate.HasFlag(AnimationProperty.Rotation) ||
                    animAttributes.PropertyTypeToAnimate.HasFlag(AnimationProperty.Alpha);
                tween.EaseCustomCurve(animAttributes.AnimationCurve, useDiff);
            } else if (animAttributes.Easing != EasingType.None) {
                EasingHelper.ApplySimpleEasing(animAttributes.Easing, tween);
            }
            if (animAttributes.Delay > 0) tween.Delay(animAttributes.Delay);
            if (animAttributes.ReflectAnimation) tween.Reflect();
            tween.Repeat(animAttributes.RepeatCount);
        }

        public void Initialize(GMSelectable rootSelectable) {
            if (!initialized) {
                this.rootSelectable = rootSelectable;
                if (!keepRaycastable) graphic.raycastTarget = rootSelectable.gameObject == graphic.gameObject;

                //Defaults
                defaultPosition = graphic.rectTransform.anchoredPosition;

                //Initialize states
                defaultState.Initialize();
                hoveredState.Initialize();
                pressedState.Initialize();
                disabledState.Initialize();
                selectedState.Initialize();
                inactiveState.Initialize();

                initialized = true;

                SetState(VisualState.Default, true);
            }
        }

        public override void UpdateName() {
            if (graphic != null) name = graphic.name;
            else name = "<missing graphic>";
        }

        [Serializable]
        public class VisualStateData : GMAnimatable {
            /// <summary>
            /// Whether or not the visual element should be enabled in this state.
            /// </summary>
            [Tooltip("Whether or not the visual element should be enabled in this state.")]
            [SerializeField] private bool showElement = true;
            public bool ShowElement => showElement;
            /// <summary>
            /// The position offset to the default position of the ui element for this state.
            /// </summary>
            [Tooltip("The position offset to the default position of the ui element for this state.")]
            [SerializeField] private Vector2 positionOffset = Vector3.zero;
            public Vector2 PositionOffset => positionOffset;
            /// <summary>
            /// The scale for this state.
            /// </summary>
            [Tooltip("The scale for this state.")]
            [SerializeField] private Vector2 scale = Vector3.one;
            public Vector2 Scale => scale;
            /// <summary>
            /// The z-rotation for this state.
            /// </summary>
            [SerializeField] float rotation = 0f;
            public float Rotation => rotation;
            /// <summary>
            /// What color should this visual element have in this state?
            /// </summary>
            [GlobalUIColorProperty]
            [SerializeField] string globalColor;
            public string GlobalColor => globalColor;

            /// <summary>
            /// The warning color this visual element has in this state.
            /// Warning is set for UI elements that need to highlight under certain conditions (e.g. a value that reaches 0).
            /// </summary>
            [GlobalUIColorProperty]
            [SerializeField] string warningGlobalColor;
            public string WarningGlobalColor => warningGlobalColor;
            /// <summary>
            /// What alpha should the visual element have in this state?
            /// This is NOT the alpha of the color, but of the graphic canvas.
            /// </summary>
            [SerializeField] float alpha = 1f;
            public float Alpha => alpha;
        }
    }
}
