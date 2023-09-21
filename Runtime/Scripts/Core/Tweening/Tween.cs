using System.Collections.Generic;
using UnityEngine;

namespace Uween {
    /// <summary>
    /// A base class for Uween's tweens.
    /// </summary>
    public abstract class Tween : MonoBehaviour {
        //Holds a reference to all running tweens
        public static List<Tween> allRunningTweens = new List<Tween>();

        protected static T Get<T>(GameObject g, float duration) where T : Tween {
            T tween = g.GetComponent<T>();
            if (tween == null) {
                tween = g.AddComponent<T>();
            }

            tween.Reset();
            tween.duration = duration;
            tween.enabled = true;
            return tween;
        }

        protected float duration;
        protected float delayTime;
        protected float elapsedTime;
        protected Easings easing;

        protected bool reflect;
        public bool Reflect {
            get { return reflect; }
            set { reflect = value; }
        }


        protected int repeatCount;

        public int RepeatCount {
            get { return repeatCount; }
            set { repeatCount = value; }
        }

        /// <summary>
        /// Total duration of this tween (sec).
        /// </summary>
        /// <value>The duration.</value>
        public float Duration {
            get { return Mathf.Max(0f, duration); }
        }

        /// <summary>
        /// Current playing position (sec).
        /// </summary>
        /// <value>The position.</value>
        public float Position {
            get { return Mathf.Max(0f, elapsedTime - DelayTime); }
        }

        /// <summary>
        /// Delay for starting tween (sec).
        /// </summary>
        /// <value>The delay time.</value>
        public float DelayTime {
            get { return Mathf.Max(0f, delayTime); }
            set { delayTime = value; }
        }

        /// <summary>
        /// Easing that be used for calculating tweening value.
        /// </summary>
        /// <value>The easing.</value>
        public Easings Easing {
            get { return easing ?? Linear.EaseNone; }
            set { easing = value; }
        }

        /// <summary>
        /// Whether tween has been completed or not.
        /// </summary>
        /// <value><c>true</c> if this tween is complete; otherwise, <c>false</c>.</value>
        public bool IsComplete {
            get { return Position >= Duration; }
        }

        /// <summary>
        /// Occurs when on tween complete.
        /// </summary>
        public event Callback OnComplete;
        private int playbackDirection = 1;

        public void Skip() {
            elapsedTime = DelayTime + Duration;
            repeatCount = 0;
            Update();
        }

        protected virtual void Reset() {
            duration = 0f;
            delayTime = 0f;
            elapsedTime = 0f;
            repeatCount = 0;
            playbackDirection = 1;
            reflect = false;
            //Don't reset if we have a custom ease curve set to avoid gc allocations.
            //custom ease curve will be reused and updated by the UI instead.
            easing = easing is CustomEase.EaseCurve ? easing : null;
            OnComplete = null;
        }

        public virtual void Update() {
            if (!GentlyUI.UIManager.UISettings.UpdateTweensByManager) {
                Update(elapsedTime + Time.deltaTime);
            }
        }

        public void Tick(float deltaTime) {
            Update(elapsedTime + deltaTime);
        }

        public virtual void Update(float elapsed) {
            float delay = DelayTime;
            float duration = Duration;

            elapsedTime = elapsed;

            if (elapsedTime < delay) {
                return;
            }

            float currentTime = elapsedTime - delay;

            if (currentTime >= duration) {
                if (RepeatCount != 0) {
                    //Restart
                    elapsedTime = (currentTime + delay) % duration;
                    currentTime = currentTime % duration;

                    //Reduce repeat count if we are not endless looping (repeat count would be -1 then)
                    if (RepeatCount > 0) {
                        repeatCount -= 1;
                    }

                    //Callback
                    if (OnComplete != null) OnComplete();

                    //Reflection
                    if (Reflect) playbackDirection *= -1;
                } else {
                    if (duration == 0f) {
                        currentTime = duration = 1f;
                    } else {
                        currentTime = duration;
                    }

                    elapsedTime = delay + duration;
                    enabled = false;
                    allRunningTweens.Remove(this);
                }
            }

            if (playbackDirection > 0) {
                UpdateValue(Easing, currentTime, duration);
            } else {
                UpdateValue(Easing, duration - currentTime, duration);
            }

            if (!enabled) {
                if (OnComplete != null) {
                    var callback = OnComplete;
                    OnComplete = null;
                    callback();
                }
            }
        }

        protected abstract void UpdateValue(Easings e, float t, float d);
    }
}