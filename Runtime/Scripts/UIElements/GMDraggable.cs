using GentlyUI.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    [RequireComponent(typeof(CanvasGroup))]
    public class GMDraggable : GMSelectableStyled, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler {
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

        private RectTransform currentPlaceholder;
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

        public virtual void OnBeginDrag(PointerEventData eventData) {
            //Create placeholder first
            CreatePlaceholder();
            //Unparent to enable dragging
            UnparentOriginalUI();
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
                    float returnDistance = Vector3.Distance(RectTransform.position, returnPosition);
                    returnSpeed = returnDistance / returnDuration;
                    break;
                default:
                    break;
            }
        }

        public override void Tick(float unscaledDeltaTime) {
            base.Tick(unscaledDeltaTime);

            if (dragState == DragState.Returning) {
                //If we are close enough to our return position we have arrived.
                if (Vector3.Distance(RectTransform.position, returnPosition) <= 0.1f) {
                    SetDragState(DragState.Idle);
                } else {
                    RectTransform.position = Vector3.MoveTowards(RectTransform.position, returnPosition, unscaledDeltaTime * returnSpeed);
                }
            } else if (dragState == DragState.Dragging) {
                UpdateDragPosition();
            }

            UpdateScale();
        }

        void CreatePlaceholder() {
            if (keepOriginal) {
                currentPlaceholder = Instantiate(gameObject, RectTransform.parent, true).transform as RectTransform;
                CanvasGroup canvasGroup = currentPlaceholder.gameObject.GetOrAddComponent<CanvasGroup>();
                canvasGroup.alpha = 0.5f;
                canvasGroup.blocksRaycasts = false;
            } else if (placeholder != null) {
                currentPlaceholder = (RectTransform)Instantiate(placeholder).transform;
            } else {
                currentPlaceholder = new GameObject("drag-placeholder", typeof(RectTransform)).transform as RectTransform;
            }

            //Parent
            currentPlaceholder.transform.SetParent(transform.parent, false);
            currentPlaceholder.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }

        void UnparentOriginalUI() {
            RectTransform.SetParent(Canvas.transform, true);
            //Canvas group
            CanvasGroup.alpha = 0.5f;
            CanvasGroup.blocksRaycasts = false;
        }

        void UpdateDragPosition() {
            Vector2 localPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, UIManager.Instance.GetCurrentPointerEventData().position, UIManager.UICamera, out localPosition);
            RectTransform.position = RectTransform.TransformPoint(localPosition);
        }

        void UpdateScale() {
            Vector3 newScale;

            if (dragState == DragState.Dragging) {
                newScale = Vector3.one * Mathf.MoveTowards(RectTransform.localScale.x, dragScale, Time.unscaledTime * 5f);
            } else {
                newScale = Vector3.one * Mathf.MoveTowards(RectTransform.localScale.x, 1f, Time.unscaledTime * 5f);
            }

            if (RectTransform.localScale != newScale) {
                RectTransform.localScale = newScale;
            }
        }

        void DestroyPlaceholder() {
            if (currentPlaceholder != null) {
                Destroy(currentPlaceholder.gameObject);
            }
        }

        void ResetDragData() {
            RectTransform.localPosition = returnParent.rect.center;
            //Reset return parent
            returnParent = null;
            //Destroy the current placeholder (will check if there even is one)
            DestroyPlaceholder();
            //Canvas group
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;
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
