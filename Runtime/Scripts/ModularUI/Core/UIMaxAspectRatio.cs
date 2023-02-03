using UnityEngine;
using UnityEngine.UI;

namespace GentlyUI {
	public class UIMaxAspectRatio : MonoBehaviour {
        public Vector2Int aspectRatio = new Vector2Int(16, 9);

        private RectTransform rectTransform;
        private RectTransform RectTransform {
            get {
                if (rectTransform == null) rectTransform = transform as RectTransform;
                return rectTransform;
            }
        }

        private Canvas canvas;
        private float CanvasScale {
            get {
                if (canvas == null) canvas = GetComponentInParent<Canvas>(true);
                if (canvas != null) {
                    return canvas.scaleFactor;
                } else {
                    return 1f;
                }
            }
        }

        private void OnRectTransformDimensionsChange() {
            //Clamp width of hud to aspect ratio
            float _aspectRatio = Mathf.Min(aspectRatio.x / (float)aspectRatio.y, Screen.width / (float)Screen.height);
            float width = Screen.height * _aspectRatio / CanvasScale;

            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }
}
