using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GentlyUI.Core {
    public class UIPaddingSetter : MonoBehaviour {
        [SerializeField] private string globalPadding;
        [SerializeField] private PaddingTarget paddingTarget;

        public enum PaddingTarget {
            PositionOffset = 0,
            LayoutGroupPadding = 1,
            LayoutGroupPaddingAndSpacing = 2
        }

        private HorizontalOrVerticalLayoutGroup layoutGroup;
        private RectTransform rectTransform;

        //Was the padding value cached from the global lut?
        private bool cached = false;

        private int padding = -1;
        private int Padding {
            get {
                if (!cached) {
                    padding = UIManager.UISettings.GetPadding(globalPadding);
                    cached = true;
                }
                return padding;
            }
        }

        private void Awake() {
            rectTransform = (RectTransform)transform;

            if (paddingTarget > 0) {
                layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
                
                if (layoutGroup != null) {
                    layoutGroup.padding = new RectOffset(Padding, Padding, Padding, Padding);

                    if (paddingTarget == PaddingTarget.LayoutGroupPaddingAndSpacing) {
                        layoutGroup.spacing = Padding;
                    }
                }
            } else {
                //Check if we are stretched
                if (rectTransform.anchorMin.x != rectTransform.anchorMax.x) {
                    //x stretch
                    rectTransform.offsetMin = new Vector2(Padding, rectTransform.offsetMin.y);
                    rectTransform.offsetMax = new Vector2(Padding, rectTransform.offsetMax.y);
                } else {
                    //x anchored
                    if (rectTransform.pivot.x == 0) {
                        rectTransform.anchoredPosition = new Vector2(Padding, rectTransform.anchoredPosition.y);
                    } else if (rectTransform.pivot.x == 1) {
                        rectTransform.anchoredPosition = new Vector2(-Padding, rectTransform.anchoredPosition.y);
                    }
                }

                if (rectTransform.anchorMin.y != rectTransform.anchorMax.y) {
                    //y stretch
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, Padding);
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.y, Padding);
                } else {
                    //y anchored
                    if (rectTransform.pivot.y == 0) {
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, Padding);
                    } else if (rectTransform.pivot.y == 1) {
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -Padding);
                    }
                }
            }
        }
    }
}
