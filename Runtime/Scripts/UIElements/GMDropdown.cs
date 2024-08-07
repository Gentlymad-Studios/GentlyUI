using GentlyUI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    public class GMDropdown : UIBase, IUITickable, IPooledUIResetter {
        [SerializeField] private GMToggle toggle;
        [SerializeField] private GMPooledScrollView scrollView;
        [SerializeField] private MonoBehaviour scrollViewItemPrefab;
        [SerializeField] private GMToggleGroup toggleGroup;
        [SerializeField] private int maxElementsToDisplay = 5;

        [SerializeField] private int value;
        public int Value => value;

        public bool IsFocused => toggle.IsOn;

        public bool IsInteractable => toggle.Interactable;

        [Serializable]
        public class DropdownEvent : UnityEvent<int> { }

        private DropdownEvent onValueChanged = new DropdownEvent();
        public DropdownEvent OnValueChanged => onValueChanged;

        private GMAnimatedContainer animatedContainer;

        [SerializeField] private List<DropdownOptionData> options = new List<DropdownOptionData>();
        public List<DropdownOptionData> Options => options;

        [Serializable]
        public class DropdownOptionData {
            [SerializeField] private Sprite icon;
            public Sprite Icon => icon;

            [SerializeField] private string label;
            public string Label => label;

            private object data;
            /// <summary>
            /// Custom data that can be attached for easier use of dropdown selection.
            /// </summary>
            [Tooltip("Custom data that can be attached for easier use of dropdown selection.")]
            public object Data => data;

            public DropdownOptionData(string label, Sprite icon, object data = null) {
                this.label = label;
                this.icon = icon;
                this.data = data;
            }

            public DropdownOptionData(string label, object data = null) {
                this.label = label;
                this.icon = null;
                this.data = data;
            }

            public DropdownOptionData(Sprite icon, object data = null) {
                this.label = null;
                this.icon = icon;
                this.data = data;
            }

            public void SetLabel(string label) {
                this.label = label;
            }

            public void SetIcon(Sprite icon) {
                this.icon = icon;
            }

            public void SetData(object data) {
                this.data = data;
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

            SetValueInternal(value, notifyChange);
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

        /// <summary>
        /// Sets the toggle of the dropdown interactable/not interactable.
        /// </summary>
        /// <param name="interactable">The target value.</param>
        /// <param name="forceUpdate">Use force update to update the toggle's visuals immediately (without transitions).</param>
        public void SetInteractable(bool interactable, bool forceUpdate = false) {
            toggle.SetInteractable(interactable, forceUpdate);
        }


        void Hide(bool updateImmediately) {
            if (animatedContainer != null) {
                animatedContainer.HideContainer(updateImmediately);
            } else {
                scrollView.gameObject.SetActive(false);
            }
            GMUIBlockerCreator.DestroyBlocker(scrollView.transform as RectTransform);
        }

        void UpdateOptions() {
            int displayCount = Mathf.Min(maxElementsToDisplay, options.Count);
            FlexibleGridLayout grid = toggleGroup.GetComponent<FlexibleGridLayout>();

            RectTransform scrollViewRect = (RectTransform)scrollView.transform;
            scrollViewRect.SetHeight(displayCount * grid.cellHeight + grid.padding.top + grid.padding.bottom + grid.spacing.y * maxElementsToDisplay);

            scrollView.Initialize<GMDropdownItem>(scrollViewItemPrefab, options.Count, OnUpdateItem);
            scrollView.SnapToElement(Value);
        }

        void OnUpdateItem(Behaviour item, int dataIndex) {
            GMDropdownItem dItem = (GMDropdownItem)item;

            if (dataIndex >= options.Count) {
                item.gameObject.SetActive(false);
                return;
            }

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

                if (!isPointerInsideScrollView) {
                    toggle.IsOn = false;
                }
            }
        }

        public void CreatePooledUICache() { }

        public void ResetPooledUI() {
            SetInteractable(true);
        }
    }
}
