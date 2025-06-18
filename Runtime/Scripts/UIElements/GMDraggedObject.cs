using GentlyUI.Core;
using UnityEngine;

namespace GentlyUI.UIElements {
    public class GMDraggedObject : UIBase, IUITickable {
        public enum DragState {
            Idle = 0,
            Dragging = 1,
            Returning = 2
        }

        private DragState dragState;
        public DragState CurrentDragState => dragState;

        private RectTransform origin;
        public RectTransform Origin => origin;

        private Vector3 returnPosition;
        private GMDraggable draggable;

        /// <summary>
        /// Setups the drag object.
        /// </summary>
        /// <param name="origin">The origin to return to when drag was started.</param>
        public void Setup(RectTransform origin, GMDraggable draggable) {
            this.draggable = draggable;

            //Update origin
            SetOrigin(origin);
            //Start drag
            SetDragState(DragState.Dragging);
            //Inform all dropzones
            for (int i = 0, count = GMDropzone.activeDropzones.Count; i < count; ++i) {
                GMDropzone.activeDropzones[i].OnDragStarted();
            }
        }

        public void SetOrigin(RectTransform origin) {
            this.origin = origin;
            returnPosition = UIManager.UICamera.WorldToViewportPoint(Origin.TransformPoint(Origin.rect.center));
        }

        public void SetDragState(DragState state) {
            dragState = state;

            switch (dragState) {
                case DragState.Idle:
                    draggable.OnDragFinished();
                    break;
                case DragState.Returning:
                    //Inform all dropzones
                    for (int i = 0, count = GMDropzone.activeDropzones.Count; i < count; ++i) {
                        GMDropzone.activeDropzones[i].OnDragEnded();
                    }
                    //Set anchored position
                    RectTransform.anchorMin = RectTransform.anchorMax = UIManager.UICamera.WorldToViewportPoint(RectTransform.position);
                    RectTransform.anchoredPosition = Vector2.zero;

                    break;
                case DragState.Dragging:
                    CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
                    canvasGroup.alpha = 0.5f;
                    canvasGroup.blocksRaycasts = false;
                    break;
                default:
                    break;
            }
        }

        public virtual void Tick(float unscaledDeltaTime) {
            UpdateScale();

            if (dragState == DragState.Returning) {
                //If we are close enough to our return position we have arrived.
                if (Vector2.Distance(RectTransform.anchorMin, returnPosition) <= 0.01f) {
                    SetDragState(DragState.Idle);
                } else {
                    RectTransform.anchorMin = RectTransform.anchorMax = Vector2.MoveTowards(RectTransform.anchorMin, returnPosition, unscaledDeltaTime * UIManager.UISettings.DragReturnSpeed);
                    RectTransform.anchoredPosition = Vector2.zero;
                }
            } else if (dragState == DragState.Dragging) {
                UpdateDragPosition();
            }
        }

        void UpdateDragPosition() {
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, UIManager.Instance.GetCurrentPointerEventData().position, UIManager.UICamera, out localPosition);
            Vector3 currentLocalPosition = RectTransform.InverseTransformPoint(transform.position);

            if (draggable.DragMode == GMDraggable.DrageModeEnum.OnlyHorizontal) {
                localPosition.y = currentLocalPosition.y;
            } else if (draggable.DragMode == GMDraggable.DrageModeEnum.OnlyVertical) {
                localPosition.x = currentLocalPosition.x;
            }

            if (draggable.ReorderableElement) {
                //Restrict to container
                Vector3[] localCornersOfOrigin = new Vector3[4];
                Origin.GetLocalCorners(localCornersOfOrigin);

                //Transform local position into local space of Origin
                Vector3 worldPosition = RectTransform.TransformPoint(localPosition);
                localPosition = Origin.InverseTransformPoint(worldPosition);

                localPosition.x = Mathf.Clamp(localPosition.x, localCornersOfOrigin[0].x, localCornersOfOrigin[2].x);
                localPosition.y = Mathf.Clamp(localPosition.y, localCornersOfOrigin[0].y, localCornersOfOrigin[2].y);

                //Update position before updating sibling index
                UpdatePositionByLocalPosition(localPosition, Origin);
                //Update siblindg index after position
                UpdateSiblingIndex();
            } else {
                UpdatePositionByLocalPosition(localPosition, transform);
            }
        }

        void UpdatePositionByLocalPosition(Vector3 localPosition, Transform objectSpace) {
            Vector3 newPosition = objectSpace.TransformPoint(localPosition);
            transform.position = newPosition;
        }

        void UpdateSiblingIndex() {
            GMDraggable hoveredDraggable = UIManager.Instance.GetCurrentHoveredDraggable();

            if (hoveredDraggable != null && hoveredDraggable != draggable && hoveredDraggable.transform.IsChildOf(Origin)) {
                draggable.UpdateSiblingIndexOfPlaceholder(hoveredDraggable.transform.GetSiblingIndex());
            }
        }

        void UpdateScale() {
            Vector3 newScale;

            if (dragState == DragState.Dragging) {
                newScale = Vector3.one * Mathf.MoveTowards(transform.localScale.x, UIManager.UISettings.DragObjectScale, Time.unscaledTime * 5f);
            } else {
                newScale = Vector3.one * Mathf.MoveTowards(transform.localScale.x, 1f, Time.unscaledTime * 5f);
            }

            if (transform.localScale != newScale) {
                transform.localScale = newScale;
            }
        }
    }
}
