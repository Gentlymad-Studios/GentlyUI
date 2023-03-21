using GentlyUI.Core;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UnityEngine.RectTransform;

namespace GentlyUI.UIElements {
    public class GMSlider : GMSelectableStyled, IPointerDownHandler, IPointerUpHandler
    {
        public enum Direction {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom
        }

        [SerializeField] private Direction direction;
        [SerializeField] private GMSlicedFilledImage fillImage;
        [SerializeField] private RectTransform handle;
        [Space]
        [SerializeField] private bool wholeNumbers;
        [Space]
        [SerializeField] private float minValue = 0;
        protected float MinValue => minValue;
        [SerializeField] private float maxValue = 1;
        protected float MaxValue => maxValue;
        [Space]
        [SerializeField] private float value;
        [Space]
        [SerializeField] private GMTextComponent valueOutput;
        [SerializeField] private GameObject valueOutputContainer;
        [SerializeField] private bool onlyShowValueOnHover = false;

        public float Value {
            get => value;
            set {
                SetValue(value);
            }
        }

        /// <summary>
        /// The normalized value of the slider.
        /// </summary>
        public float normalizedValue {
            get {
                if (Mathf.Approximately(minValue, maxValue)) {
                    return 0;
                }

                return Mathf.InverseLerp(minValue, maxValue, Value);
            }
            set {
                Value = Mathf.Lerp(minValue, maxValue, value);
            }
        }

        public override bool Interactable {
            get {
                return base.Interactable;
            }
            set
            {
                base.Interactable = value;
                TryToggleValueOutput();
            }
        }

        [Serializable]
        public class SliderEvent : UnityEvent<float> { }

        [SerializeField] private SliderEvent onValueChanged = new SliderEvent();

        public SliderEvent OnValueChanged {
            get { return onValueChanged; }
            set { onValueChanged = value; }
        }


        private RectTransform container;
        PointerEventData eventData;

        private float startValue;
        private bool movedByHandle = false;
        private Vector2 startPosition;

        private DrivenRectTransformTracker tracker;
        /// <summary>
        /// Whether or not the player is currently interacting with the slider.
        /// </summary>
        private bool isInteractionActive = false;

#if UNITY_EDITOR
        public void EDITOR_UpdateSlider() {
            if (wholeNumbers) {
                if (maxValue < minValue) maxValue = minValue;

                minValue = Mathf.Round(minValue);
                maxValue = Mathf.Round(maxValue);
            }

            SetValue(value, false);

            UpdateCache();
            UpdateVisuals();
        }
#endif

        protected override void OnEnable() {
            base.OnEnable();

            TryToggleValueOutput();

            SetValue(value, false);
            UpdateCache();
            UpdateVisuals();

            if (!onlyShowValueOnHover) {
                OnValueChanged.AddListener(OutputValue);
            }
        }

        protected override void OnDisable() {
            tracker.Clear();

            TryToggleValueOutput();

            if (!onlyShowValueOnHover) {
                OnValueChanged.RemoveListener(OutputValue);
            }

            base.OnDisable();
        }

        void TryToggleValueOutput() {
            if (valueOutputContainer == null) {
                return;
            }

            bool isValueOutputAllowed = Interactable;

            if (!onlyShowValueOnHover) {
                if (!valueOutput.gameObject.activeSelf) {
                    valueOutput.gameObject.SetActive(isValueOutputAllowed);
                }
                return;
            }

            bool showValeOutput = (isPointerInside || isInteractionActive) && isActiveAndEnabled;

            if (showValeOutput && !valueOutputContainer.gameObject.activeSelf) {
                valueOutputContainer.gameObject.SetActive(isValueOutputAllowed);
                OnValueChanged.AddListener(OutputValue);
            } else if (!showValeOutput && valueOutputContainer.gameObject.activeSelf) {
                valueOutputContainer.gameObject.SetActive(false);
                OnValueChanged.RemoveListener(OutputValue);
            }
        }

        private void SetNormalizedValue(float normalizedValue) {
            this.normalizedValue = normalizedValue;
        }

        private void SetValue(float value) {
            SetValue(value, true);
        }

        void SetValue(float input, bool sendCallback) {
            //Clamp between min and max
            float newValue = Mathf.Clamp(input, minValue, maxValue);
            
            //Round if we only allow whole numbers
            if (wholeNumbers) {
                newValue = Mathf.Round(newValue);
            }

            //Only do continue if value is not already set
            if (value != newValue) {
                value = newValue;
                UpdateVisuals();
                if (sendCallback) {
                    onValueChanged.Invoke(newValue);
                }
            }
        }
        public void SetInitialValue(float value) {
            SetValue(value, false);
            OutputValue(value);
        }

        public void SetInitialValue(float value, float minValue, float maxValue, bool wholeNumbers = false) {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.wholeNumbers = wholeNumbers;

            SetInitialValue(value);

            UpdateVisuals();
            OutputValue(value);
        }

        protected virtual void UpdateVisuals() {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCache();
#endif
            tracker.Clear();

            if (container != null) {
                if (axis == Axis.Horizontal) {
                    tracker.Add(this, handle, DrivenTransformProperties.AnchoredPositionX);
                    tracker.Add(this, handle, DrivenTransformProperties.AnchorMaxX);
                    tracker.Add(this, handle, DrivenTransformProperties.AnchorMinX);

                    handle.anchorMin = new Vector2(reverseValue ? 1f - normalizedValue : normalizedValue, handle.anchorMin.y);
                    handle.anchorMax = new Vector2(reverseValue ? 1f - normalizedValue : normalizedValue, handle.anchorMax.y);
                } else if (axis == Axis.Vertical) {
                    tracker.Add(this, handle, DrivenTransformProperties.AnchoredPositionY);
                    tracker.Add(this, handle, DrivenTransformProperties.AnchorMaxY);
                    tracker.Add(this, handle, DrivenTransformProperties.AnchorMinY);

                    handle.anchorMin = new Vector2(handle.anchorMin.x, reverseValue ? 1f - normalizedValue : normalizedValue);
                    handle.anchorMax = new Vector2(handle.anchorMax.x, reverseValue ? 1f - normalizedValue : normalizedValue);
                }

                //Fill
                fillImage.fillAmount = normalizedValue;
                //Handle
                handle.anchoredPosition = Vector2.zero;
            }
        }

        //INPUT
        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            TryToggleValueOutput();
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            TryToggleValueOutput();
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (!IsInteractionAllowed(eventData))
                return;

            base.OnPointerDown(eventData);

            StartScrolling(eventData);
        }

        void StartScrolling(PointerEventData eventData) {
            this.eventData = eventData;

            startValue = normalizedValue;
            movedByHandle = false;
            //Get local start position of pointer
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, eventData.position, UIManager.UICamera, out startPosition);
            AdjustToAbsolutePosition(ref startPosition);
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
                AdjustToAbsolutePosition(ref localMousePosition);

                if (movedByHandle) {
                    float deltaMovement;
                    float maxScrollDistance;

                    if (direction == Direction.BottomToTop || direction == Direction.TopToBottom) {
                        //Move distance
                        deltaMovement = localMousePosition.y - startPosition.y;
                        maxScrollDistance = container.GetHeight();
                    } else {
                        //Move distance
                        deltaMovement = localMousePosition.x - startPosition.x;
                        maxScrollDistance = container.GetWidth();
                    }

                    if (maxScrollDistance > 0) {
                        if (reverseValue) deltaMovement *= -1;
                        float change = (deltaMovement / maxScrollDistance);
                        float newValue = startValue + change;
                        SetNormalizedValue(newValue);
                    }
                } else {
                    float newValue;

                    if (direction == Direction.BottomToTop || direction == Direction.TopToBottom) {
                        newValue = 1f - Mathf.InverseLerp(container.GetHeight(), 0, startPosition.y);
                    } else {
                        newValue = Mathf.InverseLerp(0, container.GetWidth(), startPosition.x);
                    }

                    if (reverseValue) newValue = 1f - newValue;

                    SetNormalizedValue(newValue);

                    movedByHandle = true;
                    startValue = normalizedValue;
                }
            }
        }

        void AdjustToAbsolutePosition(ref Vector2 localPosition) {
            float x = Mathf.Clamp(container.GetWidth() * container.pivot.x, 0, container.GetWidth());
            float y = Mathf.Clamp(container.GetHeight() * container.pivot.y, 0, container.GetHeight());
            localPosition += new Vector2(x, y);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);

            this.eventData = null;
            isInteractionActive = false;

            TryToggleValueOutput();
        }

        bool IsInteractionAllowed(PointerEventData eventData) {
            return IsActive() && Interactable && eventData.button == PointerEventData.InputButton.Left;
        }

        //LAYOUT
        Axis axis { get { return (direction == Direction.LeftToRight || direction == Direction.RightToLeft) ? Axis.Horizontal : Axis.Vertical; } }
        bool reverseValue { get { return direction == Direction.RightToLeft || direction == Direction.TopToBottom; } }

        public void SetDirection(Direction direction, bool includeRectLayouts) {
            Axis oldAxis = axis;
            bool oldReverse = reverseValue;
            this.direction = direction;

            if (!includeRectLayouts)
                return;

            if (axis != oldAxis)
                RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);

            if (reverseValue != oldReverse)
                RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, (int)axis, true, true);

            switch(direction) {
                case Direction.BottomToTop:
                    fillImage.fillDirection = GMSlicedFilledImage.FillDirection.Up;
                    break;
                case Direction.TopToBottom:
                    fillImage.fillDirection = GMSlicedFilledImage.FillDirection.Down;
                    break;
                case Direction.LeftToRight:
                    fillImage.fillDirection = GMSlicedFilledImage.FillDirection.Right;
                    break;
                case Direction.RightToLeft:
                    fillImage.fillDirection = GMSlicedFilledImage.FillDirection.Left;
                    break;
            }
        }

        void UpdateCache() {
            if (handle != null && handle.parent != null) {
                container = handle.parent.GetComponent<RectTransform>();
            } else {
                container = null;
            }
        }

        void OutputValue(float value) {
            if (valueOutputContainer != null && valueOutput != null) {
                valueOutput.SetText(GlobalTextFormatter.RoundWithDecimalsToString(value));
            }
        }
    }
}
