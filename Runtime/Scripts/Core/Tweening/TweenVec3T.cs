using UnityEngine;

namespace Uween
{
    public abstract class TweenVec3T : TweenVec3
    {
        protected abstract Vector3 Vector { get; set; }

        protected override Vector3 Value
        {
            get { return Vector; }
            set { Vector = value; }
        }


        private RectTransform rectTransform;

        protected RectTransform GetTransform() {
            if (rectTransform == null) {
                rectTransform = transform as RectTransform;
            }

            return rectTransform;
        }
    }

    public abstract class TweenVec3P : TweenVec3T
    {
        protected override Vector3 Vector
        {
            get { return GetTransform().anchoredPosition; }
            set { GetTransform().anchoredPosition = value; }
        }
    }

    public abstract class TweenVec3R : TweenVec3T
    {
        protected override Vector3 Vector
        {
            get { return GetTransform().localRotation.eulerAngles; }
            set { GetTransform().localRotation = Quaternion.Euler(value); }
        }
    }

    public abstract class TweenVec3S : TweenVec3T
    {
        protected override Vector3 Vector
        {
            get { return GetTransform().localScale; }
            set { GetTransform().localScale = value; }
        }
    }
}