using GentlyUI.Core;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    public class GMDropzone : UIBase, IDropHandler, IUITickable {
        [SerializeField] private RectTransform dropContainer;
        [SerializeField] private GMAnimatedContainer highlight;
        /// <summary>
        /// The maximum number of elements that can be dropped into this zone. Set to 0 if unlimited items are possible.
        /// </summary>
        [Tooltip("The maximum number of elements that can be dropped into this zone. Set to 0 if unlimited items are possible.")]
        [SerializeField] private int maxElements = 0;
        public RectTransform DropContainer => dropContainer;

        private Type acceptedType;
        private GMDraggable currentDragTarget;

        private int ChildCount {
            get {
                int childCount = dropContainer.childCount;

                if (highlight.transform.IsChildOf(dropContainer.transform)) {
                    childCount -= 1;
                }

                return childCount;
            }
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            highlight.HideContainer(true);
            acceptedType = typeof(GMDraggable);
        }

        public virtual void Setup<T>() where T : GMDraggable {
            acceptedType = typeof(T);
        }

        public void Tick(float unscaledDeltaTime) {
            if (currentDragTarget != GMDraggable.currentDraggedElement) {
                currentDragTarget = GMDraggable.currentDraggedElement;

                if (ShouldHighlight()) {
                    highlight.ShowContainer();
                } else {
                    highlight.HideContainer();
                }
            }
        }

        bool ShouldHighlight() {
            return currentDragTarget != null && acceptedType != null && currentDragTarget.GetComponent(acceptedType) != null;
        }

        public void OnDrop(PointerEventData eventData) {
            if (acceptedType == null || (maxElements > 0 && ChildCount == maxElements)) {
                return;
            }

            GMDraggable droppedElement;

            //Check if we have a drag dummy
            GMDragDummy dragDummy = eventData.pointerDrag.GetComponent<GMDragDummy>();
            if (dragDummy != null) {
                droppedElement = dragDummy.Origin;
            } else {
                droppedElement = eventData.pointerDrag.GetComponent(acceptedType) as GMDraggable;
            }

            if (droppedElement != null) {
                OnDrop(droppedElement);
            }            
        }

        public virtual void OnDrop(GMDraggable droppedElement) {
            droppedElement.SetReturnParent(dropContainer);
        }
    }
}
