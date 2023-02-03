using GentlyUI.UIElements;
using UnityEngine;
using UnityEngine.UI;

namespace GentlyUI.ModularUI {
    public static class ContainerExtensions {
        public enum Layout {
            Vertical,
            Horizontal
        }

        public static RectTransform ApplyLayout(this RectTransform container, Layout layout, TextAnchor childAlignment, float spacing, RectOffset padding) {
            HorizontalOrVerticalLayoutGroup layoutGroup;

            if (layout == Layout.Horizontal) {
                layoutGroup = container.gameObject.GetOrAddComponent<HorizontalLayoutGroup>();
            } else {
                layoutGroup = container.gameObject.GetOrAddComponent<VerticalLayoutGroup>();
            }

            layoutGroup.spacing = spacing;
            layoutGroup.padding = padding;
            layoutGroup.childAlignment = childAlignment;

            return container;
        }

        public static RectTransform ApplyContentSizeFitter(this RectTransform container, ContentSizeFitter.FitMode horizontalFit, ContentSizeFitter.FitMode verticalFit) {
            ContentSizeFitter contentSizeFitter = container.gameObject.GetOrAddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = horizontalFit;
            contentSizeFitter.verticalFit = verticalFit;
            return container;
        }

        public static RectTransform ApplyMaxAspectRatio(this RectTransform container, Vector2Int aspectRatio) {
            container.gameObject.GetOrAddComponent<UIMaxAspectRatio>().aspectRatio = aspectRatio;
            return container;
        }

        /// <summary>
        /// Adds a layout element to the gameobject or uses an existing one with the given properties.
        /// Set parameter to -1 if it should not be set on the layout element.
        /// </summary>
        /// <param name="container">The rect transform.</param>
        /// <returns>The layout element.</returns>
        public static RectTransform AddOrSetLayoutElement(
            this RectTransform container,
            float minWidth = -1,
            float preferredWidth = -1,
            float flexibleWidth = -1,
            float minHeight = -1,
            float preferredHeight = -1,
            float flexibleHeight = -1
        ) {
            LayoutElement le = container.gameObject.GetOrAddComponent<LayoutElement>();
            //Width
            le.minWidth = minWidth;
            le.preferredWidth = preferredWidth;
            le.flexibleWidth = flexibleWidth;
            //Height
            le.minHeight = minHeight;
            le.preferredHeight = preferredHeight;
            le.flexibleHeight = flexibleHeight;

            return container;
        }

        public static RectTransform AddAnimationPresetForAnchor(
            this RectTransform container,
            UIContainerSpawner.Anchor anchor
        ) {
            GMAnimatedContainer animatedContainer = container.gameObject.GetOrAddComponent<GMAnimatedContainer>();
            UIContainerAnimationPreset animationPresetForAnchor = UIManager.UISettings.GetContainerAnimationPresetForAnchor(anchor);

            if (animationPresetForAnchor != null) {
                animatedContainer.ApplyStatePreset(animationPresetForAnchor);
            }

            return container;
        }
    }
}
