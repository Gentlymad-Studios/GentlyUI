using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    /// <summary>
    /// A toggle is a selectable that has an 'on' and 'off' state: checkbox, tab, toggle button, radio button etc.
    /// </summary>
    [AddComponentMenu("GentlyUI/Toggle", 2)]
    public class GMToggle : GMSelectableStyled, IPointerClickHandler, ISubmitHandler {
        /// <summary>
        /// A visual element that displays the on/off state of the toggle.
        /// </summary>
        [SerializeField] GMVisualElement indicator;

        [SerializeField] bool isOn = false;

        /// <summary>
        /// Defines whether the toggle is on or off.
        /// </summary>
        [Tooltip("Defines whether the toggle is on or off.")]
        public bool IsOn {
            get { return isOn; }
            set { Set(value); }
        }

        [SerializeField] private GMToggleGroup group;

        public GMToggleGroup Group {
            get { return group; }
            set {
                SetToggleGroup(value, true);
            }
        }

        [Serializable]
        public class ToggleEvent : UnityEvent<bool> { }

        [SerializeField] private ToggleEvent onValueChanged = new ToggleEvent();

        public ToggleEvent OnValueChanged {
            get { return onValueChanged; }
            set { onValueChanged = value; }
        }

        protected GMToggle() { }

        protected override void OnInitialize() {
            base.OnInitialize();

            indicator.Initialize(this);
        }

        protected override void Start() {
            base.Start();

            UpdateIndicator(true);
        }

        protected override void OnEnable() {
            base.OnEnable();

            //Register toggle in cached group
            SetToggleGroup(group, false);

            //Update visuals
            UpdateIndicator(true);
        }

        protected override void OnDisable() {
            //Remove disabled toggles from toggle group
            SetToggleGroup(null, false);

            base.OnDisable();
        }

        void SetToggleGroup(GMToggleGroup newGroup, bool updateGroupInternal) {
            GMToggleGroup oldGroup = group;

            //Remove from old group
            if (oldGroup != null) {
                //Unregister
                oldGroup.UnregisterToggle(this);
            }

            // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
            if (updateGroupInternal) {
                group = newGroup;
            }

            if (group != null && IsActive()) {
                //Register
                group.RegisterToggle(this);
            }

            //Notify group if this toggle is on
            if (newGroup != null && newGroup != oldGroup && isOn && IsActive()) {
                group.NotifyToggleOn(this);
            }
        }

        void Set(bool value) {
            Set(value, true);
        }

        void Set(bool value, bool sendCallback) {
            if (isOn == value)
                return;

            isOn = value;

            if (group != null && group.isActiveAndEnabled && IsActive()) {
                if (isOn || (!group.AnyToggleOn() && !group.allowSwitchOff)) {
                    isOn = true;
                    group.NotifyToggleOn(this);
                } else {
                    group.NotifyToggleOff(this);
                }
            }

            //Only send callback if value changed at this point
            //don't allowSwitchOff could have prevent the value change.
            if (isOn == value && sendCallback) {
                onValueChanged.Invoke(isOn);
                UpdateIndicator();
            }
        }

        /// <summary>
        /// Set isOn without invoking onValueChanged callback.
        /// </summary>
        /// <param name="value">New Value for isOn.</param>
        public void SetIsOnWithoutNotify(bool value) {
            Set(value, false);
        }

        /// <summary>
        /// Sets the value and updates the visuals immediately (without transitions). Also doesn't send the change callback.
        /// </summary>
        public void SetInitialValue(bool isOn) {
            Set(isOn, false);
            UpdateIndicator(true);
        }

        void UpdateIndicator(bool setImmediately = false) {
            if (isOn) {
                indicator.SetState(GMVisualElement.VisualState.Default, setImmediately);
            } else {
                indicator.SetState(GMVisualElement.VisualState.Inactive, setImmediately);
            }
        }

        public override void Tick(float unscaledDeltaTime) {
            base.Tick(unscaledDeltaTime);

            if (indicator.UpdateTweens) indicator.DoTweenUpdate();
        }

        void InternalToggle() {
            if (IsActive() && Interactable) {
                IsOn = !IsOn;
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
        }

        public void OnSubmit(BaseEventData eventData) {
            InternalToggle();
        }

        protected override void OnDestroy() {
            if (group != null) {
                group.EnsureValidState();
            }

            base.OnDestroy();
        }
    }
}
