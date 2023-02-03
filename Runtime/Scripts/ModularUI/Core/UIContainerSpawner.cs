using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GentlyUI.ModularUI {
    public static class UIContainerSpawner
    {
        private static RectTransform CreateContainer(string name) {
            GameObject containerGO = new GameObject(name);
            containerGO.hideFlags = HideFlags.DontSave;
            containerGO.layer = LayerMask.NameToLayer("UI");

            RectTransform container = containerGO.AddComponent<RectTransform>();
            container.hierarchyCapacity = UIManager.UISettings.MaxHierarchyCapacity;
            return container;
        }

        /// <summary>
        /// Creates a container ui object within a parent object.
        /// </summary>
        /// <returns>The newly created container ui object.</returns>
        public static RectTransform CreateContainerInParent(string name, RectTransform parent) {
            RectTransform container = CreateContainer(name);
            container.transform.SetParent(parent, false);
            return container;
        }

        private static RectTransform CreateContainerInCanvas(string name, string canvasIdentifier) {
            return CreateContainerInParent(name, UIManager.Instance.GetCanvas(canvasIdentifier).transform as RectTransform);
        }

        /// <summary>
        /// Creates a container that is anchored to a certain position in the UI.
        /// </summary>
        /// <param name="canvas">The canvas to create the container in.</param>
        /// <param name="name">The name of the container.</param>
        /// <param name="anchor">The anchored position.</param>
        /// <param name="width">The initial width of the container.</param>
        /// <param name="height">The initial height of the container.</param>
        /// <returns></returns>
        public static RectTransform CreateAnchoredRootUIContainer(
            string canvas,
            string name,
            Anchor anchor,
            float width,
            float height,
            Vector2 offsetToAnchor
        ) {
            RectTransform container = CreateContainerInCanvas(name, canvas);

            container.SetPivotAndAnchors(GetAnchorVector(anchor));
            container.anchoredPosition = offsetToAnchor;

            if (width >= 0) {
                container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }

            if (height >= 0) {
                container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }

            return container;
        }

        /// <summary>
        /// Creates a fully stretched root UI container in a canvas with margin to all screen edges.
        /// </summary>
        /// <param name="canvas">The canvas to spawn the root container into.</param>
        /// <param name="name">The name of the container</param>
        /// <param name="margin">The margin of the container to all screen edges.</param>
        /// <returns></returns>
        public static RectTransform CreateStretchedRootUIContainer(
            string canvas,
            string name,
            float margin
        ) {
            return CreateStretchedRootUIContainer(canvas, name, margin, margin, margin, margin);
        }

        /// <summary>
        /// Creates a fully stretched root UI container in a canvas with margin to all screen edges.
        /// </summary>
        /// <param name="canvas">The canvas to spawn the root container into.</param>
        /// <param name="name">The name of the container</param>
        public static RectTransform CreateStretchedRootUIContainer(
            string canvas,
            string name, 
            float leftMargin, 
            float rightMargin, 
            float topMargin, 
            float bottomMargin
        ) {
            RectTransform container = CreateContainerInCanvas(name, canvas);

            container.anchorMin = Vector2.zero;
            container.anchorMax = Vector2.one;
            container.SetOffset(leftMargin, rightMargin, topMargin, bottomMargin);

            return container;
        }

        /// <summary>
        /// Creates a root UI container that is only stretched verticall in a canvas.
        /// </summary>
        /// <param name="canvas">The canvas to spawn the root container into.</param>
        /// <param name="name">The name of the container</param>
        /// <param name="width">The horizontal width of the container.</param>
        public static RectTransform CreateVerticallyStretchedRootUIContainer (
            string canvas,
            string name,
            float topMargin,
            float bottomMargin,
            float width,
            Anchor anchor = Anchor.Center
        ) {
            RectTransform container = CreateContainerInCanvas(name, canvas);

            Vector2 anchorVector = GetAnchorVector(anchor);
            container.anchorMin = new Vector2(anchorVector.x, 0);
            container.anchorMax = new Vector2(anchorVector.x, 1);
            container.pivot = anchorVector;
            container.SetOffset(0, 0, topMargin, bottomMargin);
            container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            return container;
        }

        public static Vector2 GetAnchorVector(Anchor anchor) {
            return anchorLUT[anchor];
        }

        private static Dictionary<Anchor, Vector2> anchorLUT = new Dictionary<Anchor, Vector2>() {
            { Anchor.Top, new Vector2(0.5f, 1f) },
            { Anchor.Bottom, new Vector2(0.5f, 0f) },
            { Anchor.Left, new Vector2(0f, 0.5f) },
            { Anchor.Right, new Vector2(1f, 0.5f) },
            { Anchor.TopLeft, Vector2.up },
            { Anchor.TopRight, Vector2.one },
            { Anchor.BottomLeft, Vector2.zero },
            { Anchor.BottomRight, Vector2.right },
            { Anchor.Center, Vector2.one * 0.5f }
        };

        public enum Anchor {
            Top,
            Bottom,
            Left,
            Right,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center
        }
    }
}
