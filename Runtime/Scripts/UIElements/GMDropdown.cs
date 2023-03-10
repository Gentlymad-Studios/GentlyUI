using GentlyUI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    public class GMDropdown : UIBase, IUITickable {
        [SerializeField] private GMToggle toggle;
        [SerializeField] private GMPooledScrollView scrollView;
        [SerializeField] private MonoBehaviour scrollViewItemPrefab;
        [SerializeField] private GMToggleGroup toggleGroup;

        [SerializeField] private int value;
        public int Value => value;

        public bool IsFocused => toggle.IsOn;

        [Serializable]
        public class DropdownEvent : UnityEvent<int> {}

        private DropdownEvent onValueChanged = new DropdownEvent();
        public DropdownEvent OnValueChanged => onValueChanged;

        private GMAnimatedContainer animatedContainer;

        [SerializeField] private List<DropdownOptionData> options = new List<DropdownOptionData>();

        [Serializable]
        public class DropdownOptionData {
            [SerializeField] private Sprite icon;
            public Sprite Icon => icon;

            [SerializeField] private string label;
            public string Label => label;

            public DropdownOptionData(string label, Sprite icon) {
                this.label = label;
                this.icon = icon;
            }

            public DropdownOptionData(string label) {
                this.label = label;
                this.icon = null;
            }

            public DropdownOptionData(Sprite icon) {
                this.label = null;
                this.icon = icon;
            }
        }

        public void SetOptions(List<DropdownOptionData> options) {
            this.options = options;
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            animatedContainer = scrollView.GetComponent<GMAnimatedContainer>();
            animatedContainer.Initialize();
            //allow switch off must be true, as we are pooling our scroll view and sometimes there are only items visible that are not active.
            toggleGroup.allowSwitchOff = true;
            RefreshShownValue();
        }
        protected override void OnEnable() {
            base.OnEnable();

            toggle.IsOn = false;
            toggle.OnValueChanged.AddListener(OnToggleValueChanged);
            ToggleScrollView(toggle.IsOn, true);

            SetValueInternal(value, false);
        }

        protected override void OnDisable() {
            toggle.IsOn = false;

            base.OnDisable();

            toggle.OnValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void SetValueInternal(int value, bool notifyChange = true) {
            this.value = value;
            RefreshShownValue();
            //Close scroll view
            toggle.IsOn = false;
            //Callback
            if (notifyChange) OnValueChanged.Invoke(value);
        }

        public void RefreshShownValue() {
            //Get option
            DropdownOptionData optionData = options[Value];
            //Output
            toggle.SetIcon(optionData.Icon);
            toggle.SetLabel(optionData.Label);
        }

        public void SetValue(int value, bool notifyChange = true) {
            if (this.value == value)
                return;

            SetValueInternal(value);
        }

        public void SetDefaultValue(int value) {
            SetValueInternal(value, false);
        }

        void OnToggleValueChanged(bool isOn) {
            ToggleScrollView(isOn);
        }

        public void ToggleScrollView(bool showScrollview, bool updateImmediately = false) {
            if (showScrollview) {
                Show(updateImmediately);
            } else {
                Hide(updateImmediately);
            }
        }

        private void Show(bool updateImmediately) {
            if (animatedContainer != null) {
                animatedContainer.ShowContainer(updateImmediately);
            } else {
                scrollView.gameObject.SetActive(true);
            }
            UpdateOptions();
            GMUIBlockerCreator.CreateBlocker(scrollView.transform as RectTransform, () => { toggle.IsOn = false; });
        }


        void Hide(bool updateImmediately) {
            if (animatedContainer != null) {
                animatedContainer.HideContainer(updateImmediately);
            } else {
                scrollView.gameObject.SetActive(false);
            }
            GMUIBlockerCreator.DestroyBlocker();
        }

        void UpdateOptions() {
            scrollView.Initialize<GMDropdownItem>(scrollViewItemPrefab, options.Count, OnUpdateItem);
            scrollView.SnapToElement(Value);
        }

        void OnUpdateItem(Behaviour item, int dataIndex) {
            GMDropdownItem dItem = (GMDropdownItem)item;
            DropdownOptionData data = options[dataIndex];
            dItem.SetIcon(data.Icon);
            dItem.SetLabel(data.Label);
            dItem.SetIndex(dataIndex);
            dItem.Group = toggleGroup;
            if (dataIndex == value) {
                dItem.SetInitialValue(true);
            } else {
                dItem.SetInitialValue(false);
            }
        }

        public void Tick(float unscaledDeltaTime) {
            if (!toggle.IsOn) {
                return;
            }

            PointerEventData inputState = UIManager.Instance.GetCurrentPointerEventData();

            if (inputState.clickCount > 0 &&
                (inputState.button == PointerEventData.InputButton.Left || inputState.button == PointerEventData.InputButton.Right)) {
                bool isPointerInsideScrollView = RectTransformUtility.RectangleContainsScreenPoint(scrollView.transform as RectTransform, inputState.position, UIManager.UICamera);

                if (!isPointerInsideScrollView)
                {
                    toggle.IsOn = false;
                }
            }
        }
    }
}
