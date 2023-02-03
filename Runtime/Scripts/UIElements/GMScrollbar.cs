using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Scrollbar", 4)]
    public class GMScrollbar : GMSelectable, ICanvasElement {
       [Serializable]
       public class ScrollEvent : UnityEvent<float> { }


        [SerializeField] private RectTransform handle;
        public RectTransform Handle => handle;
        
        [Range(0f, 1f)]
        [SerializeField] private float value = 1f;
        /// <summary>
        /// The current value of the scroll bar in 0 to 1 range.
        /// </summary>
        [UnityEngine.Tooltip("The current value of the scroll bar in 0 to 1 range.")]
        public float Value => value;

        private float size = 0.2f;
        /// <summary>
        /// The current size of the scrollbar's handle in 0 to 1 range.
        /// </summary>
        [UnityEngine.Tooltip("The current size of the scrollbar's handle in 0 to 1 range.")]
        public float Size => size;

        /// <summary>
        /// The minimum size of the handle. The handle should not get shorter than this.
        /// </summary>
        [UnityEngine.Tooltip("The minimum size of the handle. The handle should not get shorter than this.")]
        [SerializeField] private float minSize = 0.1f;
        public float MinSize => minSize;

        [SerializeField]
        private ScrollEvent onValueChanged = new ScrollEvent();
        public ScrollEvent OnValueChanged => onValueChanged;

        private RectTransform container;

        private DrivenRectTransformTracker tracker;
        /// <summary>
        /// Whether or not the player is currently interacting with the scrollbar.
        /// </summary>
        private bool isInteractionActive = false;

        protected GMScrollbar() { }
        public void GraphicUpdateComplete() {}
        public void LayoutComplete() {}

        PointerEventData eventData;

        private float startValue;
        private bool movedByHandle = false;
        private Vector2 startPosition;

        public override void OnPointerDown(PointerEventData eventData) {
            if (!IsInteractionAllowed(eventData))
                return;

            base.OnPointerDown(eventData);

            StartScrolling(eventData);
        }

        void StartScrolling(PointerEventData eventData) {
            this.eventData = eventData;

            startValue = Value;
            movedByHandle = false;
            //Get local start position of pointer
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, eventData.position, UIManager.UICamera, out startPosition);
            //Check if the scrollbar is moved by handle
            movedByHandle = RectTransformUtility.RectangleContainsScreenPoint(handle, eventData.position, UIManager.UICamera);
            //This starts the tick
            isInteractionActive = true;
        }

        public override void Tick(float unscaledDeltaTime) {
            base.Tick(unscaledDeltaTime);

            if (isInteractionActive && Application.isFocused) {
                //Current local mouse position
                Vector2 localMousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(container, eventData.position, UIManager.UICamera, out localMousePosition);

                if (movedByHandle) {
                    //Move distance
                    float yDeltaMovement = startPosition.y - localMousePosition.y;
                    //Get new value from position offset
                    float maxScrollDistance = container.GetHeight() - handle.GetHeight();
                    if (maxScrollDistance > 0) {
                        float newValue = startValue + yDeltaMovement / maxScrollDistance;
                        SetValue(newValue);
                    }
                } else {
                    float newValue = Mathf.InverseLerp(0, container.GetHeight() - handle.GetHeight(), Mathf.Abs(startPosition.y) - handle.GetHeight() * 0.5f);
                    SetValue(newValue);

                    movedByHandle = true;
                    startValue = Value;
                }
            }
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);

            this.eventData = null;
            isInteractionActive = false;
        }

        bool IsInteractionAllowed(PointerEventData eventData) {
            return IsActive() && Interactable && eventData.button == PointerEventData.InputButton.Left;
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            //TODO: This can also be done if we write a custom inspector for the gm scrollbar
            UnityEditor.EditorApplication.delayCall += EDITOR_UpdateScrollbar;
        }

        void EDITOR_UpdateScrollbar() {
            UpdateCache();
            UpdateVisuals();
        }
#endif

        public void SetValue(float value) {
            SetValue(value, true);
        }

        public void SetValue(float value, bool sendCallback) {
            this.value = Mathf.Clamp01(value);

            UpdateVisuals();

            if (sendCallback) {
                onValueChanged.Invoke(value);
            }
        }

        void UpdateVisuals() {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCache();
#endif
            tracker.Clear();

            if (container != null) {
                tracker.Add(this, handle, DrivenTransformProperties.Pivot);
                tracker.Add(this, handle, DrivenTransformProperties.Anchors);

                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                float yAnchorPos = Mathf.Lerp(1f- size, 0f, value);

                anchorMin[1] = yAnchorPos;
                anchorMax[1] = yAnchorPos + size;

                handle.anchorMin = anchorMin;
                handle.anchorMax = anchorMax;

                handle.offsetMin = handle.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// Sets the size of the handle but not smaller than minimum size.
        /// </summary>
        /// <param name="size">Value from 0 to 1.</param>
        public void SetHandleSize(float size) {
            this.size = Mathf.Clamp(size, minSize, 1f);
            UpdateVisuals();
        }

        protected override void OnEnable() {
            base.OnEnable();

            UpdateCache();
        }

        protected override void OnDisable() {
            tracker.Clear();
            base.OnDisable();
        }

        void SetPivots() {
            if (container != null) {
                container.pivot = new Vector2(container.pivot.x, 1);
                handle.pivot = new Vector2(0.5f, 1);
            }
        }

        void UpdateCache() {
            if (handle != null && handle.parent != null) {
                container = handle.parent.GetComponent<RectTransform>();
            } else {
                container = null;
            }

            SetPivots();
        }

        public void Rebuild(CanvasUpdate executing) {
        }
    }
}
