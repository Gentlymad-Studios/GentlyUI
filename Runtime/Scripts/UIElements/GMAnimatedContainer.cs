using GentlyUI.Core;
using Uween;
using UnityEngine;
using static GentlyUI.UIElements.GMAnimatable;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    public class GMAnimatedContainer : UIBase {
        /// <summary>
        /// The animation state for when the container is shown.
        /// </summary>
        [Tooltip("The animation state for when the container is shown.")]
        [SerializeField] private GMAnimatedContainerState showState;
        /// <summary>
        /// The animation state for when the container is hidden.
        /// </summary>
        [Tooltip("The animation state for when the container is hidden.")]
        [SerializeField] private GMAnimatedContainerState hideState;

        private Vector3 defaultPosition;
        private CanvasGroup canvasGroup;

        /// <summary>
        /// Triggers when the show state has completed.
        /// </summary>
        [Tooltip("Triggers when the show state has completed.")]
        public Callback OnShow;
        /// <summary>
        /// Triggers when the hide state has completed.
        /// </summary>
        [Tooltip("Triggers when the hide state has completed.")]
        public Callback OnHide;

        private bool transitionRunning;
        /// <summary>
        /// Returns true if the container is currently running a transition between show and hide state.
        /// </summary>
        [Tooltip("Returns true if the container is currently running a transition between show and hide state.")]
        public bool TransitionRunning => transitionRunning;

        private bool animatedContainerInitialized = false;

        public static List<GMAnimatedContainer> runningTransitions = new List<GMAnimatedContainer>();

        private LayoutElement layoutElement;
        private LayoutElement LayoutElement {
            get {
                if (layoutElement == null) {
                    layoutElement = gameObject.GetOrAddComponent<LayoutElement>();
                }
                return layoutElement;
            }
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            Initialize();
        }

        public override void Initialize() {
            base.Initialize();

            if (animatedContainerInitialized) {
                return;
            }

            defaultPosition = RectTransform.anchoredPosition;
            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            //Stop if component is not properly set up
            if (showState == null || hideState == null) {
                return;
            }

            //Initialize states
            showState.Initialize();
            hideState.Initialize();

            animatedContainerInitialized = true;
        }

        public enum ContainerState {
            Show = 0,
            Hide = 1
        }

        private ContainerState state = ContainerState.Hide;

        public void ShowContainer(bool setImmediately = false) {
            SetState(ContainerState.Show, setImmediately);
        }

        public void HideContainer(bool setImmediately = false) {
            SetState(ContainerState.Hide, setImmediately);
        }

        public void SkipTransition() {
            if (TransitionRunning) {
                SetState(state, true);
            }
        }

        public void ApplyStatePreset(UIContainerAnimationPreset preset, bool setImmediately = true) {
            ResetContainer();

            showState = preset.showState;
            hideState = preset.hideState;

            if (!animatedContainerInitialized) {
                Initialize();
            }

            //Initialize states
            showState.Initialize();
            hideState.Initialize();

            SetState(state, setImmediately);
        }

        void SetState(ContainerState state, bool setImmediately = false) {
#if UNITY_EDITOR
            if (UIManager.Instance.EDITOR_SkipAllTransitions) {
                setImmediately = true;
            }
#endif

            //If the state is already set: return!
            if (this.state == state && !setImmediately)
                return;

            //Set state now
            this.state = state;

            if (setImmediately) {
                SetFinalVisualState();
            } else {
                //Update tweens for animations
                UpdateTweens();
            }
        }

        GMAnimatedContainerState GetCurrentState() {
            return state == ContainerState.Show ? showState : hideState;
        }

        void SetFinalVisualState() {
            UIManager.Instance.StopCoroutine(DeactivateDelayed());

            //Get State
            GMAnimatedContainerState state = GetCurrentState();

            gameObject.PauseTweens();

            if (state == null) return;

            //Position
            VisualElementAnimationAttributes posAnimAttributes = state.GetAnimationAttributes(AnimationProperty.PositionOffset);
            if (posAnimAttributes != null) {
                Vector2 finalPosition = defaultPosition + state.PositionOffset;
                RectTransform.anchoredPosition = new Vector3(finalPosition.x, finalPosition.y, 0);
            }

            //Scale
            VisualElementAnimationAttributes scaleAnimAttributes = state.GetAnimationAttributes(AnimationProperty.Scale);
            if (scaleAnimAttributes != null) {
                transform.localScale = state.Scale;
            }

            //Alpha
            VisualElementAnimationAttributes alphaAnimAttributes = state.GetAnimationAttributes(AnimationProperty.Alpha);
            if (alphaAnimAttributes != null) {
                canvasGroup.alpha = state.Alpha;
            }

            //Layout Min Height
            VisualElementAnimationAttributes layoutAnimAttributes_Height = state.GetAnimationAttributes(AnimationProperty.LayoutMinHeight);
            if (layoutAnimAttributes_Height != null) {
                LayoutElement.minHeight = state.LayoutMinHeight;
            }

            //Layout Min Width
            VisualElementAnimationAttributes layoutAnimAttributes_Width = state.GetAnimationAttributes(AnimationProperty.LayoutMinWidth);
            if (layoutAnimAttributes_Width != null) {
                LayoutElement.minWidth = state.LayoutMinWidth;
            }

            //Active state
            if (gameObject.activeSelf != state.ShowContainer) ToggleUI(state.ShowContainer);

            //End of state callback
            TriggerEndOfStateCallback();
        }

        void UpdateTweens() {
            //Get State
            GMAnimatedContainerState state = GetCurrentState();

            if (state.ShowContainer) Enable();

            //Start Tweens
            Tween tween;
            //Cache tween with longest duration to disable the element when animation finished if the state is VisualState.Inactive
            Tween longestTween = null;

            //Position
            VisualElementAnimationAttributes posAnimAttributes = state.GetAnimationAttributes(AnimationProperty.PositionOffset);
            if (posAnimAttributes != null) {
                tween = TweenXY.Add(gameObject, posAnimAttributes.Duration, defaultPosition + state.PositionOffset);
                SetupTween(tween, posAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Scale
            VisualElementAnimationAttributes scaleAnimAttributes = state.GetAnimationAttributes(AnimationProperty.Scale);
            if (scaleAnimAttributes != null) {
                tween = TweenSXY.Add(gameObject, scaleAnimAttributes.Duration, state.Scale);
                SetupTween(tween, scaleAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Alpha
            VisualElementAnimationAttributes alphaAnimAttributes = state.GetAnimationAttributes(AnimationProperty.Alpha);
            if (alphaAnimAttributes != null) {
                tween = TweenCanvasGroupA.Add(gameObject, alphaAnimAttributes.Duration, state.Alpha);
                SetupTween(tween, alphaAnimAttributes);
                CacheLongestTween(tween, ref longestTween);
            }

            //Layout Min Height
            VisualElementAnimationAttributes layoutAnimAttributes_Height = state.GetAnimationAttributes(AnimationProperty.LayoutMinHeight);
            if (layoutAnimAttributes_Height != null) {
                tween = TweenLayoutElementMinHeight.Add(gameObject, layoutAnimAttributes_Height.Duration, state.LayoutMinHeight);
                SetupTween(tween, layoutAnimAttributes_Height);
                CacheLongestTween(tween, ref longestTween);
            }

            //Layout Min Width
            VisualElementAnimationAttributes layoutAnimAttributes_Width = state.GetAnimationAttributes(AnimationProperty.LayoutMinWidth);
            if (layoutAnimAttributes_Width != null) {
                tween = TweenLayoutElementMinHeight.Add(gameObject, layoutAnimAttributes_Width.Duration, state.LayoutMinWidth);
                SetupTween(tween, layoutAnimAttributes_Width);
                CacheLongestTween(tween, ref longestTween);
            }

            //Longest tween callback
            if (longestTween != null) {
                //Add the end of state callback first so that it is triggered before the object is deactivated
                longestTween.OnComplete += TriggerEndOfStateCallback;

                if (!state.ShowContainer) {
                    longestTween.OnComplete += () => UIManager.Instance.StartCoroutine(DeactivateDelayed());
                }
            } else {
                //End of state callback
                TriggerEndOfStateCallback();
            }

            transitionRunning = true;
            runningTransitions.Add(this);
        }

        IEnumerator DeactivateDelayed() {
            yield return new WaitForEndOfFrame();
            Disable();
        }

        void TriggerEndOfStateCallback() {
            if (state == ContainerState.Show && OnShow != null) {
                OnShow();
                OnShow = null;
            } else if (state == ContainerState.Hide && OnHide != null) {
                OnHide();
                OnHide = null;
            }

            transitionRunning = false;
            runningTransitions.Remove(this);
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

            if (animAttributes.Delay > 0) {
                tween.Delay(animAttributes.Delay);
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

        /// <summary>
        /// Resets the animated container to reasonable default values.
        /// This is mostly needed when we new animation types are applied to this container on runtime.
        /// E.g. things like scaling need to be reset if they are not manipulated by the new show/hide state.
        /// </summary>
        void ResetContainer() {
            RectTransform.localScale = Vector3.one;

            if (canvasGroup != null) {
                canvasGroup.alpha = 1;
            }
        }
    }

    [System.Serializable]
    public class GMAnimatedContainerState : GMAnimatable {
        /// <summary>
        /// Defines whether the container gameobject is enabled/disabled once the state is fully reached.
        /// </summary>
        [Tooltip("Defines whether the container gameobject is enabled/disabled once the state is fully reached.")]
        [SerializeField] private bool showContainer = true;
        public bool ShowContainer => showContainer;

        /// <summary>
        /// The position offset of the container for this state.
        /// </summary>
        [Tooltip("The position offset of the container for this state.")]
        [SerializeField] private Vector2 positionOffset = Vector2.zero;
        private Vector3 positionOffsetCache = Vector3.zero;
        public Vector3 PositionOffset {
            get {
                if (positionOffsetCache.x != positionOffset.x || positionOffsetCache.y != positionOffset.y) {
                    positionOffsetCache = new Vector3(positionOffset.x, positionOffset.y, 0);
                }
                return positionOffsetCache;
            }
        }

        /// <summary>
        /// The scale of the container for this state.
        /// </summary>
        [Tooltip("The scale of the container for this state.")]
        [SerializeField] private Vector2 scale = Vector2.one;
        public Vector2 Scale => scale;

        /// <summary>
        /// The alpha of the container for this state
        /// </summary>
        [Tooltip("The alpha of the container for this state")]
        [Range(0f, 1f)]
        [SerializeField] private float alpha = 1f;
        public float Alpha => alpha;

        /// <summary>
        /// The min height of the layout element of the container for this state.
        /// </summary>
        [Tooltip("The min height of the layout element of the container for this state.")]
        [SerializeField] private float layoutMinHeight = 0f;
        public float LayoutMinHeight => layoutMinHeight;

        /// <summary>
        /// The min width of the layout element of the container for this state.
        /// </summary>
        [Tooltip("The min width of the layout element of the container for this state.")]
        [SerializeField] private float layoutMinWidth = 0f;
        public float LayoutMinWidth => layoutMinWidth;
    }
}
