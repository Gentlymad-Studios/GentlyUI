using GentlyUI.Core;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    public class GMDropzone : UIBase, IDropHandler {
        [SerializeField] private RectTransform dropContainer;
        /// <summary>
        /// The maximum number of elements that can be dropped into this zone. Set to 0 if unlimited items are possible.
        /// </summary>
        [Tooltip("The maximum number of elements that can be dropped into this zone. Set to 0 if unlimited items are possible.")]
        [SerializeField] private int maxElements = 0;
        public RectTransform DropContainer => dropContainer;

        private Type acceptedType;

        public virtual void Setup<T>() where T : GMDraggable {
            acceptedType = typeof(T);
        }

        public void OnDrop(PointerEventData eventData) {
            if (acceptedType == null || (maxElements > 0 && dropContainer.childCount == maxElements)) {
                return;
            }

            GMDraggable droppedElement = eventData.pointerDrag.GetComponent(acceptedType) as GMDraggable;

            if (droppedElement != null) {
                droppedElement.SetReturnParent(dropContainer);

                OnDrop(droppedElement);
            }            
        }

        public virtual void OnDrop(GMDraggable droppedElement) {}
    }
}
