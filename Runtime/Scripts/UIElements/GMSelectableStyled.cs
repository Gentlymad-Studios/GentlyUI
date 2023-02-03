using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Selectable Styled", 0)]
    public class GMSelectableStyled : GMSelectable
    {
        [SerializeField] private GMTextComponent labelOutput;

        [SerializeField] private GMImageComponent iconOutput;

        public void SetIcon(Sprite icon) {
            if (iconOutput != null) iconOutput.sprite = icon;
        }

        public void SetLabel(string label) {
            if (labelOutput != null) labelOutput.SetText(label);
        }
    }
}
