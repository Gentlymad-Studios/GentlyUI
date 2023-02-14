using GentlyUI.Core;
using System;
using System.Collections.Generic;
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
        private GMDraggable currentDragTarget;

        public static List<GMDropzone> activeDropzones = new List<GMDropzone>();
        bool isDropAllowed;

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

        public void Tick(float unscaledDeltaTime) {
            //Current drag target changed
            if (currentDragTarget != GMDraggable.currentDraggedElement) {
                currentDragTarget = GMDraggable.currentDraggedElement;

                if (CheckIfDropIsAllowed()) {
                    highlight.ShowContainer();
                } else {
                    highlight.HideContainer();
                }
            }
        }

        protected virtual bool CheckIfDropIsAllowed() {
            if (currentDragTarget != null) {
                isDropAllowed = true;
            } else {
                isDropAllowed = false;
            }

            return isDropAllowed;
        }

        public void OnDrop(PointerEventData eventData) {
            if (maxElements > 0 && ChildCount == maxElements) {
                return;
            }

            OnDrop(currentDragTarget);
        }

        public virtual void OnDrop(GMDraggable droppedElement) {
            droppedElement.SetReturnParent(dropContainer);
        }
    }
}
