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
    }
}
