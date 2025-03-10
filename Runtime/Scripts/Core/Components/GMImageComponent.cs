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

            SetGlobalUIColor(globalUIColor);

            if (HasNoMaterial() && UIManager.Instance != null && UIManager.UISettings.defaultUIMaterial != null) {
                material = UIManager.UISettings.defaultUIMaterial;
            }
        }

        bool HasNoMaterial() {
            return material == null || material == Graphic.defaultGraphicMaterial;
        }

        public void SetGlobalUIColor(string globalUIColor) {
            this.globalUIColor = globalUIColor;

            if (useGlobalUIColor) {
                color = UIManager.UISettings.GetColor(globalUIColor);
            }
        }
    }
}
