using UnityEngine;
using UnityEngine.UI;

namespace Uween {
    public class TweenLayoutElementMinHeight : TweenVec1 {
        public static TweenLayoutElementMinHeight Add(GameObject g, float duration) {
            return Add<TweenLayoutElementMinHeight>(g, duration);
        }

        public static TweenLayoutElementMinHeight Add(GameObject g, float duration, float to) {
            return Add<TweenLayoutElementMinHeight>(g, duration, to);
        }

        private LayoutElement L;

        protected LayoutElement GetLayoutElement() {
            if (L == null) {
                L = GetComponent<LayoutElement>();
            }

            return L;
        }

        protected override float Value {
            get => GetLayoutElement().minHeight;
            set => GetLayoutElement().minHeight = value;
        }
    }
}
