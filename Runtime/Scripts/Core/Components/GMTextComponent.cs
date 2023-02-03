using TMPro;
using UnityEngine;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Text Component", 101)]
    public class GMTextComponent : TextMeshProUGUI {
        private float scale = 1f;

        public void UpdateAnimation() {
            transform.localScale = Vector3.one * scale;
        }
    }
}
