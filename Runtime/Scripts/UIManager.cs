using System.Collections.Generic;
using UnityEngine;
using GentlyUI.Core;
using GentlyUI.ModularUI;
using UnityEngine.EventSystems;
using Uween;
using GentlyUI.UIElements;

namespace GentlyUI {
    public class UIManager : MonoBehaviour {
        /// <summary>
        /// The global ui settings file being used to style the whole UI.
        /// </summary>
        [Tooltip("The global ui settings file being used to style the whole UI.")]
        [SerializeField] private UISettings uiSettings;
        public static UISettings UISettings => instance.uiSettings;

        /// <summary>
        /// The ui camera that will render every root canvas.
        /// </summary>
        [Tooltip("The ui camera that will render every root canvas.")]
        [SerializeField] private Camera uiCamera;
        public static Camera UICamera => instance.uiCamera;

        private List<UIBase> uiBases = new List<UIBase>();
        private List<IUITickable> tickableUIs = new List<IUITickable>();

        private List<UIBase> uiBasesToAdd = new List<UIBase>();
        private List<UIBase> uiBasesToRemove = new List<UIBase>();

        private Dictionary<string, Canvas> canvasLUT = new Dictionary<string, Canvas>();

        PointerEventData pointerEventData;
        private bool wasPointerEventDataUpdatedThisFrame = false;

        private static UIManager instance;
        public static UIManager Instance => instance;

        private float updateTimer = 0f;

        private bool EDITOR_skipAllTransitions = false;
        public bool EDITOR_SkipAllTransitions {
            get => EDITOR_skipAllTransitions;
            set {
#if UNITY_EDITOR
                EDITOR_skipAllTransitions = value;
#else
                EDITOR_skipAllTransitions = false;
#endif
            }
        }

        private void Awake() {
            uiSettings.Initialize();
            instance = this;
            DontDestroyOnLoad(gameObject);

            //Spawn all canvasses that should be spawned on game start.
            foreach (KeyValuePair<string, CanvasData> cd in UISettings.canvasDataLUT) {
                if (cd.Value.spawnOnGameStart) {
                    SpawnCanvas(cd.Key);
                }
            }
        }

        private void OnEnable() {
#if UNITY_EDITOR
            //Make sure that on a domain reload the instance is still set.
            instance = this;
#endif
        }

        public void SpawnCanvas(string identifier) {
            if (!canvasLUT.ContainsKey(identifier)) {
                CanvasData cd = UISettings.GetCanvasData(identifier);
                Canvas canvas = UISpawner<Canvas>.SpawnUI(cd.pathToCanvas, null);
                OnCanvasSpawned(canvas, identifier);
            }
        }

        public void ActivateCanvas(string identifier) {
            if (canvasLUT.ContainsKey(identifier)) {
                Canvas canvas = canvasLUT[identifier];
                canvas.enabled = true;
                canvas.gameObject.SetActive(true);
            }
        }

        public void DeactivateCanvas(string identifier) {
            if (canvasLUT.ContainsKey(identifier)) {
                Canvas canvas = canvasLUT[identifier];
                canvas.enabled = false;
                canvas.gameObject.SetActive(false);
            }
        }

        void OnCanvasSpawned(Canvas canvas, string identifier) {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            canvasLUT.Add(identifier, canvas);
        }

        public Canvas GetCanvas(string name) {
            return canvasLUT[name];
        }

        private void LateUpdate() {
            if (!UISettings.UpdateManagerManually) {
                Tick();
            }
        }

        public void Tick() {
            ProcessPointerEventData();

            //Tick tickables (not bound to ui update rate)
            for (int i = 0, count = tickableUIs.Count; i < count; ++i) {
                tickableUIs[i].Tick(Time.unscaledDeltaTime);
            }

            wasPointerEventDataUpdatedThisFrame = false;

            for (int i = 0, count = uiBasesToRemove.Count; i < count; ++i) {
                RemoveRegisteredUI(uiBasesToRemove[i]);
            }

            uiBasesToRemove.Clear();

            for (int i = 0, count = uiBasesToAdd.Count; i < count; ++i) {
                UIBase uiBase = uiBasesToAdd[i];
                AddUIToRegister(uiBase);
                uiBase.OnRegisterUI();
            }

            uiBasesToAdd.Clear();

            //Update timer dependencies
            updateTimer += Time.unscaledDeltaTime;

            if (updateTimer >= UISettings.UIUpdateRate) {
                //Update Tweens
                if (UISettings.UpdateTweensByManager) {
                    for (int i = Tween.allRunningTweens.Count - 1; i >= 0; --i) {
                        Tween tween = Tween.allRunningTweens[i];
                        //Tween can sometimes become null if it ends in a bad spot.
                        if (tween != null) {
                            Tween.allRunningTweens[i].Tick(updateTimer);
                        }
                    }
                }

                updateTimer -= UISettings.UIUpdateRate;
            }
        }

        public void RegisterUI(UIBase ui) {
            if (!uiBases.Contains(ui)) {
                if (!uiBasesToAdd.Contains(ui)) uiBasesToAdd.Add(ui);
            }

            if (uiBasesToRemove.Contains(ui)) uiBasesToRemove.Remove(ui);
        }

        public void UnregisterUI(UIBase ui) {
            if (uiBases.Contains(ui)) {
                if (!uiBasesToRemove.Contains(ui)) uiBasesToRemove.Add(ui);
            }

            if (uiBasesToAdd.Contains(ui)) uiBasesToAdd.Remove(ui);
        }

        void AddUIToRegister(UIBase ui) {
            uiBases.Add(ui);

            IUITickable tickable = ui as IUITickable;
            if (tickable != null && !tickableUIs.Contains(tickable)) {
                tickableUIs.Add(tickable);
            }
        }

        void RemoveRegisteredUI(UIBase ui) {
            uiBases.Remove(ui);

            if (ui is IUITickable tickable && tickableUIs.Contains(tickable)) {
                tickableUIs.Remove(tickable);
            }
        }

        private GMSelectable currentHoveredSelectable;
        private bool leftMouseButtonPressed;

        private void ProcessPointerEventData() {
            if (!wasPointerEventDataUpdatedThisFrame || pointerEventData == null) {
                pointerEventData = new PointerEventData(EventSystem.current);

#if ENABLE_INPUT_SYSTEM
                pointerEventData.position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();

                if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) {
                    pointerEventData.button = PointerEventData.InputButton.Left;
                    pointerEventData.clickCount = 1;
                } else if (UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame) {
                    pointerEventData.button = PointerEventData.InputButton.Right;
                    pointerEventData.clickCount = 1;
                }

                leftMouseButtonPressed = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
                pointerEventData.position = Input.mousePosition;

                if (Input.GetMouseButtonDown(0)) {
                    pointerEventData.button = PointerEventData.InputButton.Left;
                    pointerEventData.clickCount = 1;
                } else if (Input.GetMouseButtonDown(1)) {
                    pointerEventData.button = PointerEventData.InputButton.Right;
                    pointerEventData.clickCount = 1;
                }

				leftMouseButtonPressed = Input.GetMouseButton(0);
#endif
                //Update hovered selectable
                List<RaycastResult> hoveredElements = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerEventData, hoveredElements);

                for (int i = 0, count = hoveredElements.Count; i < count; ++i) {
                    GMSelectable selectable = hoveredElements[i].gameObject.GetComponent<GMSelectable>();

                    if (selectable != null) {
                        currentHoveredSelectable = selectable;

                        if (leftMouseButtonPressed) {
                            pointerEventData.pointerPress = selectable.gameObject;
                        }
                        break;
                    }
                }

                wasPointerEventDataUpdatedThisFrame = true;
            }
        }

        public PointerEventData GetCurrentPointerEventData() {
            return pointerEventData;
        }

        public GMSelectable GetCurrentHoveredSelectable() {
            return currentHoveredSelectable;
        }

        public void SelectUI(GameObject uiObject) {
            if (EventSystem.current.alreadySelecting)
                return;

            EventSystem.current.SetSelectedGameObject(uiObject);
        }
    }

    public interface IUITickable {
        void Tick(float unscaledDeltaTime);
    }
}
