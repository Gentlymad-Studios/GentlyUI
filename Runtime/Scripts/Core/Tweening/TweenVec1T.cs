using UnityEngine;

namespace Uween
{
    public abstract class TweenVec1T : TweenVec1
    {
        protected abstract Vector3 Vector { get; set; }


        private RectTransform rectTransform;

        protected RectTransform GetTransform() {
            if (rectTransform == null) {
                rectTransform = transform as RectTransform;
            }

            return rectTransform;
        }
    }

    public abstract class TweenVec1P : TweenVec1T
    {
        protected override Vector3 Vector
        {
            get { return GetTransform().anchoredPosition; }
            set { GetTransform().anchoredPosition = value; }
        }
    }

    public abstract class TweenVec1R : TweenVec1T
    {
        protected override Vector3 Vector
        {
            get { return GetTransform().localRotation.eulerAngles; }
            set { GetTransform().localRotation = Quaternion.Euler(value); }
        }
    }

    public abstract class TweenVec1S : TweenVec1T
    {
        protected override Vector3 Vector
        {
            get { return GetTransform().localScale; }
            set { GetTransform().localScale = value; }
        }
    }
}