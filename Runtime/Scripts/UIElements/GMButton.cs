using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Button", 1)]
    public class GMButton : GMSelectableStyled, IPointerClickHandler, ISubmitHandler {
        [Serializable]
        public class ButtonClickedEvent : GentlyUIEvent { }

        [SerializeField] private ButtonClickedEvent onClick = new ButtonClickedEvent();

        private bool isClickAllowed;

        protected GMButton() { }

        public ButtonClickedEvent OnClick {
            get { return onClick; }
            set { onClick = value; }
        }

        private void Press() {
            if (!IsActive() || !Interactable)
                return;

                onClick.Invoke();
        }

        protected override void SetPressedState() {
            if (onClick.ListenerCount > 0) {
                base.SetPressedState();
            }
        }

        // Trigger all registered callbacks.
        public virtual void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left || !isClickAllowed)
                return;

            Press();
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
            isClickAllowed = Interactable;
        }

        public virtual void OnSubmit(BaseEventData eventData) {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !Interactable)
                return;

            SetVisualState(GMVisualElement.VisualState.Pressed, false);
        }
    }
}
