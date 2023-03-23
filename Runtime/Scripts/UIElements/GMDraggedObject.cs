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
            returnPosition = Origin.TransformPoint(Origin.rect.center);
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
                if (Vector3.Distance(transform.position, returnPosition) <= 0.1f) {
                    SetDragState(DragState.Idle);
                } else {
                    transform.position = Vector3.MoveTowards(transform.position, returnPosition, unscaledDeltaTime * UIManager.UISettings.DragReturnSpeed);
                }
            } else if (dragState == DragState.Dragging) {
                UpdateDragPosition();
            }
        }

        void UpdateDragPosition() {
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, UIManager.Instance.GetCurrentPointerEventData().position, UIManager.UICamera, out localPosition);

            Vector3 newPosition = transform.TransformPoint(localPosition);

            if (draggable.DragMode == GMDraggable.DrageModeEnum.OnlyHorizontal) {
                newPosition.y = transform.position.y;
            } else if (draggable.DragMode == GMDraggable.DrageModeEnum.OnlyVertical) {
                newPosition.x = transform.position.x;
            }
            
            transform.position = newPosition;
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
