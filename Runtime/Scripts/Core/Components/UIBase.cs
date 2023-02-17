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

            if (!uiInitialized) {
                InitializeUI();
            }
        }

        /// <summary>
        /// Should only be called if you need an ui to be initialized before being active in hierarchy.
        /// Normally that shouldn't be a problem.
        /// </summary>
        public void InitializeUI() {
            OnInitialize();
            uiInitialized = true;
        }

        protected override void Start() {
            base.Start();

            AfterInitialize();
        }

        /// <summary>
        /// Called immediately the first time this UIBase is ever active in a scene.
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Called the first time this UIBase is ever active in a scene but after other scripts have executed.
        /// </summary>
        protected virtual void AfterInitialize() { }


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
        public virtual void Enable() {
            gameObject.SetActive(true);
        }

        public virtual void Disable() {
            gameObject.SetActive(false);
        }
    }
}
