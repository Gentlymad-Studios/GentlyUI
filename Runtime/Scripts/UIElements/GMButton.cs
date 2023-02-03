using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Button", 1)]
    public class GMButton : GMSelectableStyled, IPointerClickHandler, ISubmitHandler, IPooledUIResetter {
        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }

        [SerializeField] private ButtonClickedEvent onClick = new ButtonClickedEvent();

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

        // Trigger all registered callbacks.
        public virtual void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData) {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !Interactable)
                return;

            SetVisualState(GMVisualElement.VisualState.Pressed, false);
        }

        public void CreatePooledUICache() {}

        public void ResetPooledUI() {
            ToggleWarning(false);
        }
    }
}
