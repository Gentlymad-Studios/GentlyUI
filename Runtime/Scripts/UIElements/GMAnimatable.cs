using System;
using UnityEngine;
using GentlyUI.Core;
using System.Collections.Generic;
using Uween;

namespace GentlyUI.UIElements {
    public class GMAnimatable : Namable {
        public override void UpdateName() { }

        [SerializeField] List<VisualElementAnimationAttributes> animationAttributes;
        public List<VisualElementAnimationAttributes> AnimationAttributes => animationAttributes;

        private Dictionary<AnimationProperty, VisualElementAnimationAttributes> animationAttributesLUT;

        private static Array animationProperties;
        private static Array AnimationProperties {
            get {
                if (animationProperties == null) {
                    animationProperties = Enum.GetValues(typeof(AnimationProperty));
                }

                return animationProperties;
            }
        }


        public virtual void Initialize() {
            //Check if already initialized first
            if (animationAttributesLUT != null)
                return;

            animationAttributesLUT = new Dictionary<AnimationProperty, VisualElementAnimationAttributes>();

            foreach (AnimationProperty property in AnimationProperties) {
                for (int i = 0, count = animationAttributes.Count; i < count; ++i) {
                    VisualElementAnimationAttributes attribute = animationAttributes[i];
                    if (attribute.PropertyTypeToAnimate.HasFlag(property)) {
                        animationAttributesLUT.Add(property, attribute);
                    }
                }
            }
        }

        public VisualElementAnimationAttributes GetAnimationAttributes(AnimationProperty animationProperty) {
            if (animationAttributesLUT.ContainsKey(animationProperty)) {
                return animationAttributesLUT[animationProperty];
            }

            return null;
        }

        [Serializable]
        public class VisualElementAnimationAttributes : Namable {
            [SerializeField] AnimationProperty propertyToAnimate;
            public AnimationProperty PropertyTypeToAnimate => propertyToAnimate;

            [Header("Animation Attributes")]
            /// <summary>
            /// The duration of the animation.
            /// </summary>
            [SerializeField] float duration = 0.5f;
            public float Duration => duration;

            /// <summary>
            /// Should the start of the animation be delayed?
            /// </summary>
            [SerializeField] float delay = 0f;
            public float Delay => delay;

            /// <summary>
            /// How often should the animation repeat? (-1 = endless loop, 0 = no repeat, 1 = 1 repeat etc.)
            /// </summary>
            [SerializeField] int repeatCount = 0;
            public int RepeatCount => repeatCount;

            /// <summary>
            /// Whether the animation should reverse each time it's repeated (ping-pong).
            /// </summary>
            [SerializeField] bool reflectAnimation = false;
            public bool ReflectAnimation => reflectAnimation;

            /// <summary>
            /// Whether the animation curved should be used to interpolate the animation values.
            /// Otherwise the animation will be interpolated linear.
            /// </summary>
            [SerializeField] EasingType easing = EasingType.None;
            public EasingType Easing => easing;

            /// <summary>
            /// The animation curve for this state. Only used if useAnimationCurve is set to true.
            /// </summary>
            [SerializeField] AnimationCurve animationCurve;
            public AnimationCurve AnimationCurve => animationCurve;

            public override void UpdateName() {
                if (propertyToAnimate == 0) {
                    name = "Nothing";
                } else if (propertyToAnimate < 0) {
                    name = "Everything";
                } else {
                    name = propertyToAnimate.ToString();
                }
            }
        }
    }

    [Flags]
    public enum AnimationProperty {
        PositionOffset = 1,     // 0000001
        Scale = 2,              // 0000010
        Rotation = 4,           // 0000100
        Color = 8,              // 0001000
        Alpha = 16,             // 0010000
        LayoutMinHeight = 32,   // 0100000
        LayoutMinWidth = 64     // 1000000
    }
}
