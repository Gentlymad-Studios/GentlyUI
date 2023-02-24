using GentlyUI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.UIElements {
    public class GMDropzone : UIBase, IDropHandler {
        [SerializeField] private RectTransform dropContainer;
        [SerializeField] private GMAnimatedContainer highlight;
        /// <summary>
        /// The maximum number of elements that can be dropped into this zone. Set to 0 if unlimited items are possible.
        /// </summary>
        [Tooltip("The maximum number of elements that can be dropped into this zone. Set to 0 if unlimited items are possible.")]
        [SerializeField] private int maxElements = 0;
        public RectTransform DropContainer => dropContainer;
        private GMDraggable currentDragTarget;

        public static List<GMDropzone> activeDropzones = new List<GMDropzone>();
        protected bool isDropAllowed;

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
        }

        protected override void OnEnable() {
            base.OnEnable();

            activeDropzones.Add(this);
        }

        protected override void OnDisable() {
            base.OnDisable();

            activeDropzones.Remove(this);
        }

        public void OnDragStarted() {
            isDropAllowed = CheckIfDropIsAllowed();

            if (isDropAllowed) {
                highlight.ShowContainer();
            } else {
                highlight.HideContainer();
            }
        }

        public void OnDragEnded() {
            isDropAllowed = false;
            highlight.HideContainer();
        }

        protected virtual bool CheckIfDropIsAllowed() {
            if (GMDraggable.currentDraggedElement == null) {
                return false;
            }

            if (GMDraggable.currentDraggedElement.CurrentDragObject.Origin == DropContainer ||
                GMDraggable.currentDraggedElement.transform.IsChildOf(DropContainer)) {
                return false;
            }

            if (maxElements > 0 && ChildCount >= maxElements) {
                return false;
            }

            return true;
        }

        public void OnDrop(PointerEventData eventData) {
            if (isDropAllowed) {
                OnDrop(GMDraggable.currentDraggedElement.CurrentDragObject);
            }
        }

        public virtual void OnDrop(GMDraggedObject droppedElement) {
            droppedElement.SetOrigin(DropContainer);
        }
    }
}
