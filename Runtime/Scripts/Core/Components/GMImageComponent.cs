using UnityEngine;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Image Component", 100)]
    public class GMImageComponent : Image {
        [SerializeField] private bool useGlobalUIColor = false;
        [GlobalUIColorProperty]
        [SerializeField] private string globalUIColor;

        protected override void Awake() {
            base.Awake();

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            if (useGlobalUIColor) {
                color = UIManager.UISettings.GetColor(globalUIColor);
            }
        }
    }
}
