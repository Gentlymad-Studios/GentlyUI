using GentlyUI.Core;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    [RequireComponent(typeof(CanvasGroup))]
    public class GMDraggable : UIBase, IBeginDragHandler, IDragHandler, IEndDragHandler, IUITickable, IPointerClickHandler {
        /// <summary>
        /// (Optional) Assign a UI prefab that will be spawned as drag visuals.
        /// </summary>
        [Tooltip("(Optional) Assign a UI prefab that will be spawned as drag visuals.")]
        [SerializeField] private GameObject dragDummyPrefab;
        /// <summary>
        /// Defines whether this draggable should be reparented if dropped on a valid dropzone.
        /// Disable this if you want to have custom update logic on the dropzone.
        /// </summary>
        [Tooltip("Defines whether this draggable should be reparented if dropped on a valid dropzone.\r\nDisable this if you want to have custom update logic on the dropzone.")]
        public bool reparentOnDrop = true;

        private readonly Timer mouseSingleClickTimer = new Timer();

        /// <summary>
        /// Global access to currently dragged element
        /// </summary>
        public static GMDraggable currentDraggedElement;

        private RectTransform currentPlaceholder;
        private GMDraggedObject currentDragObject;
        public GMDraggedObject CurrentDragObject => currentDragObject;

        private CanvasGroup canvasGroup;
        private CanvasGroup CanvasGroup {
            get {
                if (canvasGroup == null) {
                    canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
                }
                return canvasGroup;
            }
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            mouseSingleClickTimer.Interval = 400;
            mouseSingleClickTimer.Elapsed += SingleClick;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {
            OnDragStarted();
        }

        public void OnDrag(PointerEventData eventData) {}

        public virtual void OnEndDrag(PointerEventData eventData) {
            //Set state to returning
            currentDragObject.SetDragState(GMDraggedObject.DragState.Returning);
        }

        public virtual void Tick(float unscaledDeltaTime) {
            CanvasGroup.blocksRaycasts = currentDraggedElement == null;
        }

        void CreateDragObject() {
            if (dragDummyPrefab != null) {
                currentDragObject = Instantiate(dragDummyPrefab, transform.root, false).GetOrAddComponent<GMDraggedObject>();
                currentDragObject.transform.position = RectTransform.transform.position;
                currentDragObject.Setup(RectTransform as RectTransform, this);
            } else {
                CreatePlaceholder();
                currentDragObject = gameObject.GetOrAddComponent<GMDraggedObject>();
                currentDragObject.Setup(transform.parent as RectTransform, this);
                currentDragObject.transform.SetParent(transform.root, true);
            }
        }

        void CreatePlaceholder() {
            //Spawn empty placeholder to keep layout at original position (e.g. in a grid layout)
            currentPlaceholder = new GameObject("drag-placeholder", typeof(RectTransform)).transform as RectTransform;
            //Parent
            currentPlaceholder.transform.SetParent(transform.parent, false);
            currentPlaceholder.transform.SetSiblingIndex(transform.GetSiblingIndex());
            LayoutElement layoutElement = RectTransform.GetComponent<LayoutElement>();

            if (layoutElement != null) {
                LayoutElement _layout = currentPlaceholder.gameObject.AddComponent<LayoutElement>();
                _layout.preferredHeight = layoutElement.preferredHeight;
                _layout.preferredWidth = layoutElement.preferredWidth;
                _layout.minHeight = layoutElement.minHeight;
                _layout.minWidth = layoutElement.minWidth;
                _layout.flexibleHeight = layoutElement.flexibleHeight;
                _layout.flexibleWidth = layoutElement.flexibleWidth;
            } else {
                currentPlaceholder.SetSize(RectTransform.GetSize());
            }
        }

        void OnDragStarted() {
            currentDraggedElement = this;

            CreateDragObject();

            //Canvas group
            CanvasGroup.alpha = 0.5f;
            CanvasGroup.blocksRaycasts = false;
        }

        void DestroyPlaceholder() {
            if (currentPlaceholder != null) {
                Destroy(currentPlaceholder.gameObject);
                currentPlaceholder = null;
            }
        }

        void ResetDragObject() {
            bool returnToPlaceholder = currentPlaceholder != null && currentPlaceholder.parent == currentDragObject.Origin;
            bool reparentOriginalElement = reparentOnDrop || returnToPlaceholder;

            if (reparentOriginalElement) {
                //Parent
                RectTransform.SetParent(currentDragObject.Origin, false);
                RectTransform.transform.localScale = Vector3.one;
                //Check if we should use the last sibling index: If we have a placeholder in the same parent we have to!
                if (returnToPlaceholder) {
                    RectTransform.SetSiblingIndex(currentPlaceholder.GetSiblingIndex());
                }
            }

            if (currentDragObject.gameObject != gameObject) {
                //Destroy drag object
                Destroy(currentDragObject.gameObject);
            } else {
                //Remove component
                Destroy(currentDragObject);
                //Destroy placeholder
                DestroyPlaceholder();
            }

            currentDragObject = null;
        }

        public void OnDragFinished() {
            //Canvas group
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;
            //Reset current drag target
            ResetDragObject();
            //Reset global cache
            currentDraggedElement = null;
        }

        /// <summary>
        /// Allows to set a prefab that is displayed while dragging.
        /// </summary>
        /// <param name="dragDummy">The prefab.</param>
        public void SetDragDummy(GameObject dragDummy) {
            dragDummyPrefab = dragDummy;
        }

        /* --- Click logic ------*/
        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (eventData.clickCount > 1) {
                    //Double click
                    OnDoubleClick();

                    if (mouseSingleClickTimer.Enabled) {
                        mouseSingleClickTimer.Stop();
                    }
                } else {
                    //Single click
                    if (!mouseSingleClickTimer.Enabled) {
                        mouseSingleClickTimer.Start();
                    }
                }
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                OnRightClick();
            }
        }

        void SingleClick(object o, System.EventArgs e) {
            mouseSingleClickTimer.Stop();
            OnClick();
        }

        public virtual void OnClick() {}

        public virtual void OnDoubleClick() {}

        public virtual void OnRightClick() {}
    }
}
