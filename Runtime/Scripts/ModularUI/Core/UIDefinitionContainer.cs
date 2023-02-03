using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public class UIDefinitionContainer : MonoBehaviour {
        public class ContainerEnabledEvent : UnityEvent { }

        private ContainerEnabledEvent onEnable = new ContainerEnabledEvent();
        public ContainerEnabledEvent OnContainerEnabled => onEnable;


        public class ContainerDisabledEvent : UnityEvent { }
        private ContainerDisabledEvent onDisable = new ContainerDisabledEvent();
        public ContainerDisabledEvent OnContainerDisabled => onDisable;

        private RectTransform rectTransform;
        public RectTransform RectTransform {
            get {
                if (rectTransform == null) rectTransform = (RectTransform)transform;
                return rectTransform;
            }
        }

        private void OnEnable() {
            onEnable.Invoke();
        }

        private void OnDisable() {
            onDisable.Invoke();
        }
    }
}
