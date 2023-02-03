using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    public class GMDropdownItem : GMToggle, ICancelHandler {
        private GMDropdown dropdown;
        private GMDropdown Dropdown {
            get {
                if (dropdown == null) dropdown = GetComponentInParent<GMDropdown>();
                return dropdown;
            }
        }

        private int index;

        public void SetIndex(int index) {
            this.index = index;
        }

        protected override void OnEnable() {
            base.OnEnable();

            OnValueChanged.AddListener(OnValueChange);
        }

        protected override void OnDisable() {
            base.OnDisable();

            OnValueChanged.RemoveListener(OnValueChange);
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);

            if (IsOn) {
                //If we click the current active item we just want to close the dropdown by setting the same value again (don't send callback though).
                Dropdown.SetDefaultValue(Dropdown.Value);
                return;
            }
        }

        void OnValueChange(bool isOn) {
            if (isOn) {
                Dropdown.SetValue(index);
            }
        }

        public void OnCancel(BaseEventData eventData) {
            Dropdown.ToggleScrollView(false);
        }
    }
}
