using UnityEngine;
using UnityEngine.UI;

namespace Uween {
    public class TweenLayoutElementMinWidth: TweenVec1 {
        public static TweenLayoutElementMinWidth Add(GameObject g, float duration) {
            return Add<TweenLayoutElementMinWidth>(g, duration);
        }

        public static TweenLayoutElementMinWidth Add(GameObject g, float duration, float to) {
            return Add<TweenLayoutElementMinWidth>(g, duration, to);
        }

        private LayoutElement L;

        protected LayoutElement GetLayoutElement() {
            if (L == null) {
                L = GetComponent<LayoutElement>();
            }

            return L;
        }

        protected override float Value {
            get => GetLayoutElement().minWidth;
            set => GetLayoutElement().minWidth = value;
        }
    }
}
