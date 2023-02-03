using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.Core {
    public class UIBase : UIBehaviour {
        bool uiInitialized = false;

        private RectTransform rectTransform;
        protected RectTransform RectTransform {
            get {
                if (rectTransform == null) rectTransform = transform as RectTransform;
                return rectTransform;
            }
        }

        protected override void Awake() {
            base.Awake();

            uiInitialized = true;
            OnInitialize();
        }

        protected override void Start() {
            base.Start();

            AfterInitialize();
        }

        /// <summary>
        /// Called immediately the first time this UIBase is ever active in a scene.
        /// </summary>
        protected virtual void OnInitialize() {}

        /// <summary>
        /// Called the first time this UIBase is ever active in a scene but after other scripts have executed.
        /// </summary>
        protected virtual void AfterInitialize() {}


        protected override void OnEnable() {
            if (uiInitialized) UIManager.Instance.RegisterUI(this);
        }

        protected override void OnDisable() {
            if (uiInitialized) UIManager.Instance.UnregisterUI(this);
        }

        protected override void OnDestroy() {
            if (uiInitialized) UIManager.Instance.UnregisterUI(this);
        }

        /// <summary>
        /// Called when the UI was registered in the UIManager.
        /// </summary>
        public virtual void OnRegisterUI() { }

        /// <summary>
        /// Sets the gameobject of this ui to active = true.
        /// </summary>
        public void Enable() {
            gameObject.SetActive(true);
        }

        public void Disable() {
            gameObject.SetActive(false);
        }
    }
}
