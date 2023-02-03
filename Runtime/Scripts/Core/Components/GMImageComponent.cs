using UnityEngine;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Image Component", 100)]
    public class GMImageComponent : Image {
        public bool useGlobalUIColor = false;
        [GlobalUIColorProperty]
        [SerializeField] private string globalUIColor;

        protected override void Awake() {
            base.Awake();

            if (useGlobalUIColor) {
                color = UIManager.UISettings.GetColor(globalUIColor);
            }
        }
    }
}
