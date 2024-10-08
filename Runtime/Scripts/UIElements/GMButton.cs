using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.EventSystems.PointerEventData;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Button", 1)]
    public class GMButton : GMSelectableStyled, IPointerClickHandler, ISubmitHandler {
        [Serializable]
        public class ButtonClickedEvent : GentlyUIEvent { }

        [SerializeField] private ButtonClickedEvent onClick = new ButtonClickedEvent();
        [SerializeField] private ButtonClickedEvent onRightClick = new ButtonClickedEvent();

        private bool isClickAllowed;

        protected GMButton() { }

        public ButtonClickedEvent OnClick {
            get { return onClick; }
            set { onClick = value; }
        }

        public ButtonClickedEvent OnRightClick {
            get { return onRightClick; }
            set { onRightClick = value; }
        }

        protected void Press(InputButton buttonType) {
            if (!IsActive() || !Interactable)
                return;

            if (buttonType == InputButton.Right) {
                onRightClick.Invoke();
            } else if (buttonType == InputButton.Left) {
                onClick.Invoke();
            }
        }

        protected override void SetPressedState() {
            if (onClick.ListenerCount > 0) {
                base.SetPressedState();
            }
        }

        // Trigger all registered callbacks.
        public virtual void OnPointerClick(PointerEventData eventData) {
            if (!isClickAllowed)
                return;

            Press(eventData.button);
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
            isClickAllowed = Interactable && eventData.button != InputButton.Middle;
        }

        public virtual void OnSubmit(BaseEventData eventData) {
            Press(InputButton.Left);

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !Interactable)
                return;

            SetVisualState(GMVisualElement.VisualState.Pressed, false);
        }

        //POOLED CALLBACK
        UnityAction currentClickCallback;

        /// <summary>
        /// Sets a callback that is automatically removed when this item is returned by an object pool.
        /// </summary>
        /// <param name="callback"></param>
        public void SetClickCallback(UnityAction callback) {
            RemoveCurrentClickCallback();
            currentClickCallback = callback;

            OnClick.AddListener(currentClickCallback);
        }

        public override void ResetPooledUI() {
            base.ResetPooledUI();

            RemoveCurrentClickCallback();
        }

        /// <summary>
        /// Removes the current set click callback. This also happens automatically if the button is returned by an object pool.
        /// </summary>
        public void RemoveCurrentClickCallback() {
            if (currentClickCallback != null) {
                OnClick.RemoveListener(currentClickCallback);
                currentClickCallback = null;
            }
        }
    }
}
