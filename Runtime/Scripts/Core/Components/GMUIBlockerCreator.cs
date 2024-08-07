using GentlyUI.UIElements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GentlyUI.Core {
    public static class GMUIBlockerCreator {

        private static Dictionary<RectTransform, GMUIBlocker> blockers = new Dictionary<RectTransform, GMUIBlocker>();

        /// <summary>
        /// Create a blocker that blocks clicks to other controls while the dropdown list is open.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain a blocker GameObject.
        /// </remarks>
        public static void CreateBlocker(RectTransform target, Action onClickedOnBlocker, Color? color = null) {
            DestroyBlocker(target);

            // Create blocker GameObject.
            GameObject blocker = new GameObject("Blocker");
            blocker.layer = LayerMask.NameToLayer("UI");

            // Setup blocker RectTransform to cover entire root canvas area.
            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(target.root, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            Canvas blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            blockerCanvas.sortingOrder = target.gameObject.GetOrAddComponent<Canvas>().sortingOrder;

            // Find the Canvas that this dropdown is a part of
            Canvas parentCanvas = target.GetComponentInParent<Canvas>(true);

            // If we have a parent canvas, apply the same raycasters as the parent for consistency.
            if (parentCanvas != null) {
                Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++) {
                    Type raycasterType = components[i].GetType();
                    if (blocker.GetComponent(raycasterType) == null) {
                        blocker.AddComponent(raycasterType);
                    }
                }
            } else {
                // Add raycaster since it's needed to block.
                blocker.GetOrAddComponent<GraphicRaycaster>();
            }


            // Add image since it's needed to block, but make it clear or colorize it if a color was passed.
            GMImageComponent blockerImage = blocker.AddComponent<GMImageComponent>();
            blockerImage.color = color.HasValue ? color.Value : Color.clear;

            //Finally put the blocker component on top
            GMUIBlocker uiBlocker = blocker.GetOrAddComponent<GMUIBlocker>();
            uiBlocker.Setup(target, onClickedOnBlocker);

            blockers.Add(target, uiBlocker);
        }

        public static void DestroyBlocker(RectTransform target) {
            if (blockers.TryGetValue(target, out GMUIBlocker blocker)) {
                GameObject.Destroy(blocker.gameObject);
                blockers.Remove(target);
            }
        }
    }
}
