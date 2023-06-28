using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Selectable Styled", 0)]
    public class GMSelectableStyled : GMSelectable {
        [SerializeField] private GMTextComponent labelOutput;
        protected GMTextComponent LabelOutput => labelOutput;

        [SerializeField] private GMImageComponent iconOutput;
        protected GMImageComponent IconOutput => iconOutput;

        private GMVisualElement labelVisualElement;
        private GMVisualElement iconVisualElement;

        public GMSelectableStyled() {
            if (HasLabelOutput()) {
                labelVisualElement = labelOutput.GetComponent<GMVisualElement>();
            }

            if (HasIconOutput()) {
                iconVisualElement = iconOutput.GetComponent<GMVisualElement>();
            }
        }

        public void SetIcon(Sprite icon) {
            if (iconOutput != null) iconOutput.sprite = icon;
        }

        public void SetLabel(string label) {
            if (labelOutput != null) labelOutput.SetText(label);
        }

        protected bool HasIconOutput() {
            return iconOutput != null;
        }

        protected bool HasLabelOutput() {
            return labelOutput != null;
        }

        public void ToggleIconOutput(bool enable) {
            if (!HasIconOutput()) {
                return;
            }

            if (iconVisualElement != null) {
                iconVisualElement.EnableVisualElement(enable);
            } else {
                iconOutput.gameObject.SetActive(enable);
            }
        }

        public void ToggleLabelOutput(bool enable) {
            if (!HasLabelOutput()) {
                return;
            }

            if (labelVisualElement != null) {
                labelVisualElement.EnableVisualElement(enable);
            } else {
                labelOutput.gameObject.SetActive(enable);
            }
        }
    }
}
