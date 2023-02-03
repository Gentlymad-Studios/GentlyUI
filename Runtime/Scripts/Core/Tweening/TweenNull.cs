using UnityEngine;

namespace Uween
{
    public class TweenNull : Tween
    {
        public static TweenNull Add(GameObject g, float duration)
        {
            TweenNull tween = Tween.Get<TweenNull>(g, duration);
            if (!allRunningTweens.Contains(tween)) {
                allRunningTweens.Add(tween);
            }
            return tween;
        }

        protected override void UpdateValue(Easings e, float t, float d)
        {
        }
    }
}