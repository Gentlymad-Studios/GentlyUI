using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Uween {
    public class TweenCanvasGroupA : TweenVec1 {
        public static TweenCanvasGroupA Add(GameObject g, float duration) {
            return Add<TweenCanvasGroupA>(g, duration);
        }

        public static TweenCanvasGroupA Add(GameObject g, float duration, float to) {
            return Add<TweenCanvasGroupA>(g, duration, to);
        }

        private CanvasGroup CG;

        protected CanvasGroup GetCanvasGroup() {
            if (CG == null) {
                CG = GetComponent<CanvasGroup>();
            }

            return CG;
        }

        protected override float Value 
        {
            get => GetCanvasGroup().alpha;
            set => GetCanvasGroup().alpha = value; 
        }
    }
}
