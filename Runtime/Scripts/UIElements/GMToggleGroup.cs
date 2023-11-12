using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.UIElements {
    public class GMToggleGroup : GMContent {
        /// <summary>
        /// Defines whether all toggles can be switched off.
        /// </summary>
        [Tooltip("Defines whether all toggles can be switched off.")]
        public bool allowSwitchOff = false;

        private List<GMToggle> toggles = new List<GMToggle>();
        private GMToggle activeToggle;
        /// <summary>
        /// Optional buttons that can be used to toggle through the tabs.
        /// </summary>
        [SerializeField] private GMHoldInteractionButton[] buttons;

        [Serializable]
        public class ToggleGroupEvent : UnityEvent<GMToggle> { }

        [SerializeField] private ToggleGroupEvent onActiveToggleChanged = new ToggleGroupEvent();

        public ToggleGroupEvent OnActiveToggleChanged {
            get { return onActiveToggleChanged; }
            set { onActiveToggleChanged = value; }
        }

        protected GMToggleGroup() { }

        protected override void Awake() {
            base.Awake();

            //Buttons
            for (int i = 0, count = buttons.Length; i < count; ++i) {
                GMHoldInteractionButton button = buttons[i];
                button.OnClick.AddListener(() => {
                    SwitchActiveToggle(button.scrollDirection);
                });
            }
        }

        protected override void Start() {
            EnsureValidState();
            base.Start();
        }

        protected override void OnEnable() {
            EnsureValidState();
            base.OnEnable();
        }

        void SwitchActiveToggle(int direction) {
            if (toggles == null || toggles.Count < 2 || activeToggle == null) {
                return;
            }

            int currentIndex = toggles.IndexOf(activeToggle);
            currentIndex += direction;

            if (currentIndex >= toggles.Count) {
                currentIndex = 0;
            } else if (currentIndex < 0) {
                currentIndex = toggles.Count - 1;
            }

            GMToggle newToggle = toggles[currentIndex];
            newToggle.IsOn = true;
        }

        public void RegisterToggle(GMToggle toggle) {
            if (!toggles.Contains(toggle)) {
                toggles.Add(toggle);
            }
        }

        public void UnregisterToggle(GMToggle toggle) {
            if (toggles.Contains(toggle)) {
                toggles.Remove(toggle);

                if (toggle == activeToggle) {
                    activeToggle = null;
                }
            }
        }

        public void NotifyToggleOn(GMToggle toggle) {
            //Return if toggle is not part of the group
            if (!IsTogglePartOfGroup(toggle)) {
                return;
            }

            //Disable all other toggles in group
            //but keep corret toggle on
            bool _allowSwitchOff = allowSwitchOff;
            allowSwitchOff = true;

            for (int i = 0, count = toggles.Count; i < count; ++i) {
                GMToggle _toggle = toggles[i];
                _toggle.IsOn = _toggle == toggle;
            }

            allowSwitchOff = _allowSwitchOff;

            //Cache active toggle
            if (activeToggle != toggle) {
                activeToggle = toggle;
                onActiveToggleChanged.Invoke(activeToggle);
            }
        }

        public void NotifyToggleOff(GMToggle toggle) {
            if (activeToggle == toggle) {
                activeToggle = null;
                onActiveToggleChanged.Invoke(activeToggle);
            }
        }

        public GMToggle GetActiveToggle() {
            return activeToggle;
        }

        public bool AnyToggleOn() {
            //IsOn check is important so that allowSwitchOff false works for the active toggle being deactivated.
            return activeToggle != null && activeToggle.IsOn;
        }

        bool IsTogglePartOfGroup(GMToggle toggle) {
            return toggle != null && toggles.Contains(toggle);
        }

        public void SetAllTogglesOff(bool notifyChange = true) {
            bool _allowSwitchOff = allowSwitchOff;
            allowSwitchOff = true;

            for (int i = 0, count = toggles.Count; i < count; ++i) {
                if (notifyChange) {
                    toggles[i].IsOn = false;
                } else {
                    toggles[i].SetIsOnWithoutNotify(false);
                }                
            }

            allowSwitchOff = _allowSwitchOff;
        }

        public void EnsureValidState() {
            if (!allowSwitchOff && !AnyToggleOn() && toggles.Count != 0) {
                if (activeToggle != null && toggles.Contains(activeToggle)) {
                    activeToggle.IsOn = true;
                } else {
                    for (int i = 0, count = toggles.Count; i < count; ++i) {
                        GMToggle _toggle = toggles[i];
                        if (_toggle.IsOn) {
                            //Notify toggle on if a toggle is already active
                            NotifyToggleOn(_toggle);
                            return;
                        }
                    }

                    //Fallback. Activate first toggle
                    toggles[0].IsOn = true;
                }
            }
        }
    }
}
