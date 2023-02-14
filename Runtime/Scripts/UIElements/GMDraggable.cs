using GentlyUI.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    [RequireComponent(typeof(CanvasGroup))]
    public class GMDraggable : UIBase, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IUITickable {
        /// <summary>
        /// Defines whether we drag a clone of the ui element or if we drag the actual ui element.
        /// </summary>
        [Tooltip("Defines whether we drag a clone of the ui element or if we drag the actual ui element.")]
        [SerializeField] private bool keepOriginal = true;
        /// <summary>
        /// The scale of the element while being dragged.
        /// </summary>
        [Tooltip("The scale of the element while being dragged.")]
        [SerializeField] private float dragScale = 1.1f;
        /// <summary>
        /// How long in seconds it should take for a ui element to snap to its return position (either the new position in the drop zone or its original position)
        /// </summary>
        [Tooltip("How long in seconds it should take for a ui element to snap to its return position (either the new position in the drop zone or its original position)")]
        [SerializeField] private float returnDuration = 0.1f;
        /// <summary>
        /// (Optional) Assign a UI prefab that will be spawned as a placeholder at the original position of the ui element while being dragged.
        /// </summary>
        [Tooltip("(Optional) Assign a UI prefab that will be spawned as a placeholder at the original position of the ui element while being dragged.")]
        [SerializeField] private GameObject placeholder;
        /// <summary>
        /// (Optional) Assign a UI prefab that will be spawned as drag visuals.
        /// </summary>
        [Tooltip("(Optional) Assign a UI prefab that will be spawned as drag visuals.")]
        [SerializeField] private GameObject dragDummy;

        private RectTransform currentPlaceholder;
        private RectTransform currentDragTarget;
        private GMDropzone targetDropZone;

        private CanvasGroup canvasGroup;
        private CanvasGroup CanvasGroup {
            get {
                if (canvasGroup == null) {
                    canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
                }
                return canvasGroup;
            }
        }

        //Canvas
        private Canvas canvas;
        private Canvas Canvas {
            get {
                if (canvas == null) canvas = RectTransform.GetComponentInParent<Canvas>();
                return canvas;
            }
        }

        public enum DragState {
            Idle = 0,
            Dragging = 1,
            Returning = 2
        }

        private DragState dragState;

        /// <summary>
        /// The parent this element will snap to when drag ends.
        /// </summary>
        private RectTransform returnParent;
        private Vector3 returnPosition;
        private float returnSpeed;

        public static GMDraggable currentDraggedElement;

        private UnityEvent<GameObject> onDragDummySpawned = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnDragDummySpawned => onDragDummySpawned;


        protected override void OnInitialize() {
            base.OnInitialize();

            RectTransform.pivot = Vector2.one * 0.5f;
            SetReturnParent(transform.parent as RectTransform, true);
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {
            //Create placeholder first
            CreatePlaceholder();
            //Unparent to enable dragging
            SetupDragObject();
            //Initial update position to pointer
            UpdateDragPosition();
            //Set state
            SetDragState(DragState.Dragging);
        }

        public void OnDrag(PointerEventData eventData) {
            UpdateDragPosition();
        }

        public virtual void OnEndDrag(PointerEventData eventData) {
            //If we are already in idle mode dragging was ended from outside
            if(dragState == DragState.Idle) {
                return;
            }

            //Set state to returning
            SetDragState(DragState.Returning);
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (targetDropZone == null) {
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2) {
                SetReturnParent(targetDropZone.DropContainer);
                SetDragState(DragState.Returning);
                targetDropZone.OnDrop(this);
            }
        }

        void SetDragState(DragState state) {
            dragState = state;

            switch(dragState) {
                case DragState.Idle:
                    RectTransform.SetParent(returnParent, false);
                    ResetDragData();
                    break;
                case DragState.Returning:
                    //Set return parent if there is none
                    if (returnParent == null) {
                        returnPosition = currentPlaceholder.TransformPoint(currentPlaceholder.rect.center);
                        SetReturnParent(currentPlaceholder.parent as RectTransform);
                    } else {
                        returnPosition = returnParent.TransformPoint(returnParent.rect.center);
                    }
                    //Calculate return speed
                    float returnDistance = Vector3.Distance(currentDragTarget.position, returnPosition);
                    returnSpeed = returnDistance / returnDuration;
                    break;
                default:
                    break;
            }
        }

        public virtual void Tick(float unscaledDeltaTime) {
            if (dragState == DragState.Returning) {
                //If we are close enough to our return position we have arrived.
                if (Vector3.Distance(currentDragTarget.position, returnPosition) <= 0.1f) {
                    SetDragState(DragState.Idle);
                } else {
                    currentDragTarget.position = Vector3.MoveTowards(currentDragTarget.position, returnPosition, unscaledDeltaTime * returnSpeed);
                }
            } else if (dragState == DragState.Dragging) {
                UpdateDragPosition();
            }

            if (dragState != DragState.Idle) {
                UpdateScale();
            }
        }

        void CreatePlaceholder() {
            if (keepOriginal) {
                CanvasGroup canvasGroup;
                currentPlaceholder = RectTransform;
                
                if (dragDummy == null) {
                    //Spawn a copy of the original ui element as placeholder
                    currentPlaceholder = Instantiate(gameObject, RectTransform.parent, true).transform as RectTransform;
                }
                
                canvasGroup = currentPlaceholder.gameObject.GetOrAddComponent<CanvasGroup>();
                canvasGroup.alpha = 0.5f;
                canvasGroup.blocksRaycasts = false;
            } else if (placeholder != null) {
                //Spawn placeholder
                currentPlaceholder = (RectTransform)Instantiate(placeholder).transform;
            } else {
                //Spawn empty placeholder to keep layout at original position (e.g. in a grid layout)
                currentPlaceholder = new GameObject("drag-placeholder", typeof(RectTransform)).transform as RectTransform;
            }

            //Parent
            currentPlaceholder.transform.SetParent(transform.parent, false);
            currentPlaceholder.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }

        void CreateDragDummy() {
            currentDragTarget = Instantiate(dragDummy, Canvas.transform, false).transform as RectTransform;
            CanvasGroup canvasGroup = currentDragTarget.gameObject.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.5f;
            canvasGroup.blocksRaycasts = false;
        }

        void SetupDragObject() {
            if (dragDummy != null) {
                CreateDragDummy();

                if (!keepOriginal) {
                    RectTransform.gameObject.SetActive(false);
                }
            } else {
                RectTransform.SetParent(Canvas.transform, true);
                //Canvas group
                CanvasGroup.alpha = 0.5f;
                CanvasGroup.blocksRaycasts = false;
                //Set as drag target
                currentDragTarget = RectTransform;
            }

            currentDraggedElement = this;
            onDragDummySpawned.Invoke(currentDraggedElement.gameObject);
        }

        void UpdateDragPosition() {
            Vector2 localPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(currentDragTarget, UIManager.Instance.GetCurrentPointerEventData().position, UIManager.UICamera, out localPosition);
            currentDragTarget.position = currentDragTarget.TransformPoint(localPosition);
        }

        void UpdateScale() {
            Vector3 newScale;

            if (dragState == DragState.Dragging) {
                newScale = Vector3.one * Mathf.MoveTowards(currentDragTarget.localScale.x, dragScale, Time.unscaledTime * 5f);
            } else {
                newScale = Vector3.one * Mathf.MoveTowards(currentDragTarget.localScale.x, 1f, Time.unscaledTime * 5f);
            }

            if (currentDragTarget.localScale != newScale) {
                currentDragTarget.localScale = newScale;
            }
        }

        void DestroyPlaceholder() {
            if (currentPlaceholder != null && currentPlaceholder != RectTransform) {
                Destroy(currentPlaceholder.gameObject);
            }
        }

        void DestroyDragDummy() {
            if (currentDragTarget != null && currentDragTarget != RectTransform) {
                Destroy(currentDragTarget.gameObject);
            }
        }

        void ResetDragData() {
            if (!RectTransform.gameObject.activeSelf) {
                RectTransform.gameObject.SetActive(true);
            }

            if (currentDragTarget == RectTransform) {
                RectTransform.localPosition = returnParent.rect.center;
            }
            RectTransform.localScale = Vector3.one;
            //Reset return parent
            returnParent = null;
            //Destroy the current placeholder (will check if there even is one)
            DestroyPlaceholder();
            //Canvas group
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;
            //Reset current drag target
            DestroyDragDummy();
            currentDragTarget = null;
            //Reset global cache
            currentDraggedElement = null;
        }

        public void SetReturnParent(RectTransform parent, bool returnImmediately = false) {
            returnParent = parent;
            if (returnImmediately) {
                SetDragState(DragState.Idle);
            }
        }

        /// <summary>
        /// Use this method to set a target dropzone.
        /// If a target dropzone is set the item can also be "dropped" by double clicking it.
        /// </summary>
        /// <param name="dropzone">The target dropzone.</param>
        public void SetTargetDropzone(GMDropzone dropzone) {
            targetDropZone = dropzone;
        }
    }
}
