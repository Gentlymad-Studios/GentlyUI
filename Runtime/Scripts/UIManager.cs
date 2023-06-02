using System.Collections.Generic;
using UnityEngine;
using GentlyUI.Core;
using GentlyUI.ModularUI;
using UnityEngine.EventSystems;
using Uween;
using GentlyUI.UIElements;

namespace GentlyUI {
    /// <summary>
    /// The heart of the GentlyUI
    /// </summary>
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

        private List<IUITickable> uiTickablesToAdd = new List<IUITickable>();
        private List<IUITickable> uiTickablesToRemove = new List<IUITickable>();

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
            if (canvasLUT.ContainsKey(name)) {
                return canvasLUT[name];
            } else {
                return null;
            }
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

            //Remove ui bases
            for (int i = 0, count = uiBasesToRemove.Count; i < count; ++i) {
                RemoveRegisteredUI(uiBasesToRemove[i]);
            }

            uiBasesToRemove.Clear();

            //Remove tickables
            for (int i = 0, count = uiTickablesToRemove.Count; i < count; ++i) {
                RemoveRegisteredUITickable(uiTickablesToRemove[i]);
            }

            uiTickablesToRemove.Clear();

            //Add ui bases
            for (int i = 0, count = uiBasesToAdd.Count; i < count; ++i) {
                UIBase uiBase = uiBasesToAdd[i];
                AddUIToRegister(uiBase);
                uiBase.OnRegisterUI();
            }

            uiBasesToAdd.Clear();

            //Add tickables
            for (int i = 0, count = uiTickablesToAdd.Count; i < count; ++i) {
                AddUITickableToRegister(uiTickablesToAdd[i]);
            }

            uiTickablesToAdd.Clear();

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


                //Reduce updateTimer to a steady frame rate
                updateTimer = updateTimer % UISettings.UIUpdateRate;
            }
        }

        public void RegisterUI(UIBase ui) {
            if (!uiBases.Contains(ui)) {
                uiBasesToAdd.Add(ui);
            }

            if (uiBasesToRemove.Contains(ui)) uiBasesToRemove.Remove(ui);
        }

        public void UnregisterUI(UIBase ui) {
            if (uiBases.Contains(ui)) {
                uiBasesToRemove.Add(ui);
            }

            if (uiBasesToAdd.Contains(ui)) uiBasesToAdd.Remove(ui);
        }

        public void RegisterUITickable(IUITickable uiTickable) {
            if (!tickableUIs.Contains(uiTickable)) {
                uiTickablesToAdd.Add(uiTickable);
            }

            if (uiTickablesToRemove.Contains(uiTickable)) uiTickablesToRemove.Remove(uiTickable);
        }

        public void UnregisterUITickable(IUITickable uiTickable) {
            if (tickableUIs.Contains(uiTickable)) {
                uiTickablesToRemove.Add(uiTickable);
            }

            if (uiTickablesToAdd.Contains(uiTickable)) uiTickablesToAdd.Remove(uiTickable);
        }

        void AddUIToRegister(UIBase ui) {
            uiBases.Add(ui);

            IUITickable tickable = ui as IUITickable;
            if (tickable != null) {
                AddUITickableToRegister(tickable);
            }
        }

        void RemoveRegisteredUI(UIBase ui) {
            uiBases.Remove(ui);

            if (ui is IUITickable tickable) {
                RemoveRegisteredUITickable(tickable);
            }
        }

        void AddUITickableToRegister(IUITickable uiTickable) {
            tickableUIs.Add(uiTickable);
        }

        void RemoveRegisteredUITickable(IUITickable uiTickable) {
            tickableUIs.Remove(uiTickable);
        }

        private GMSelectable currentHoveredSelectable;
        private GMDraggable currentHoveredDraggable;
        private bool leftMouseButtonPressed;
        private List<RaycastResult> hoveredElements;
        public void ProcessPointerEventData() {
            currentHoveredSelectable = null;
            currentHoveredDraggable = null;

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
                hoveredElements = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerEventData, hoveredElements);

                for (int i = 0, count = hoveredElements.Count; i < count; ++i) {
                    GMSelectable selectable = hoveredElements[i].gameObject.GetComponent<GMSelectable>();
                    GMDraggable draggable = hoveredElements[i].gameObject.GetComponent<GMDraggable>();

                    if (currentHoveredSelectable == null && selectable != null) {
                        currentHoveredSelectable = selectable;

                        if (leftMouseButtonPressed) {
                            pointerEventData.pointerPress = selectable.gameObject;
                        }
                    }

                    if (currentHoveredDraggable == null && draggable != null) {
                        currentHoveredDraggable = draggable;
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

        public GMDraggable GetCurrentHoveredDraggable() {
            return currentHoveredDraggable;
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
