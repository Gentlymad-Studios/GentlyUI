using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    public class GMHoldInteractionButton : GMButton {
        /// <summary>
        /// The initial delay. On pointer down the button will trigger once and wait for initial delay before triggering further.
        /// </summary>
        [Tooltip("The initial delay. On pointer down the button will trigger once and wait for initial delay before triggering further.")]
        public float initialDelay;
        /// <summary>
        /// The overall trigger delay. After initial delay this is the delay that defines in which intervals the button will trigger.
        /// </summary>
        [Tooltip("The overall trigger delay. After initial delay this is the delay that defines in which intervals the button will trigger.")]
        public float triggerDelay;
        /// <summary>
        /// The scroll direction. 1 = down/right, -1 = up/left.
        /// </summary>
        [Tooltip("The scroll direction. 1 = down/right, -1 = up/left.")]
        public int scrollDirection = 1;

        float heldTimer;

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);

            OnPointerClick(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData) {
            //Avoid the default click action!
            //base.OnPointerClick(eventData);
        }

        public override void Tick(float unscaledDeltaTime) {
            base.Tick(unscaledDeltaTime);

            if (IsPressed()) {
                if (heldTimer == 0f) {
                    Press(PointerEventData.InputButton.Left);
                } else if (heldTimer > initialDelay) {
                    Press(PointerEventData.InputButton.Left);
                    heldTimer -= triggerDelay;
                }

                heldTimer += unscaledDeltaTime;
            } else if (heldTimer > 0f) {
                heldTimer = 0f;
            }
        }
    }
}
