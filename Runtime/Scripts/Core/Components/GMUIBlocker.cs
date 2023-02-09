using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.Core {
    public class GMUIBlocker : UIBase, IUITickable {
        /// <summary>
        /// A RectTransform that the player is allowed to click into without the blocker disappearing.
        /// </summary>
        private RectTransform target;


        System.Action onClickedOnBlocker;

        public void Setup(RectTransform target, System.Action onClickedOnBlocker) {
            this.target = target;
            this.onClickedOnBlocker = onClickedOnBlocker;
        }

        public void Tick(float unscaledDeltaTime) {
            PointerEventData inputState = UIManager.Instance.GetCurrentPointerEventData();

            if (inputState.clickCount > 0 &&
                (inputState.button == PointerEventData.InputButton.Left || inputState.button == PointerEventData.InputButton.Right)) {

                if (!RectTransformUtility.RectangleContainsScreenPoint(target, inputState.position, UIManager.UICamera)) {
                    OnClickedOnBlocker();
                }
            }
        }

        void OnClickedOnBlocker() {
            onClickedOnBlocker.Invoke();
            GMUIBlockerCreator.DestroyBlocker();
        }
    }
}
