using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.RectTransform;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Flexible Grid Layout", 50)]
    public class FlexibleGridLayout : LayoutGroup {
        public enum LayoutConstraints {
            Uniform = 0,
            FixedRows = 1,
            FixedColumns = 2
        }

        /// <summary>
        /// Defines whether columns and rows should be distributed evenly or with fixed counts.
        /// </summary>
        [Tooltip("Defines whether columns and rows should be distributed evenly or with fixed counts.")]
        public LayoutConstraints layoutConstraints = LayoutConstraints.Uniform;
        /// <summary>
        /// Defines if children will try to align horizontal or vertical for uneven counts.
        /// </summary>
        [Tooltip("Defines if children will try to align horizontal or vertical for uneven counts.")]
        [SerializeField] protected Axis startAxis = Axis.Horizontal;
        public Axis StartAxis { get { return startAxis; } set { SetProperty(ref startAxis, value); } }
        /// <summary>
        /// Defines whether all elements should adjust their width to fit into their parent.
        /// </summary>
        [Tooltip("Defines whether all elements should adjust their width to fit into their parent.")]
        public bool fillWidth;
        /// <summary>
        /// Defines whether all elements should adjust their height to fit into their parent.
        /// </summary>
        [Tooltip("Defines whether all elements should adjust their height to fit into their parent.")]
        public bool fillHeight;

        public Vector2 spacing;
        public int rows;
        public int columns;
        [Header("Cell Size")]
        public float cellWidth;
        public float cellHeight;

        public UnityEvent onLayoutUpdate;
        LayoutGroup parentLayout;

        public FlexibleGridLayout() {
            onLayoutUpdate = new UnityEvent();
        }

        public override void CalculateLayoutInputHorizontal() {
            base.CalculateLayoutInputHorizontal();

            if (transform.parent != null) {
                parentLayout = transform.parent.GetComponent<LayoutGroup>();
            }

            if (fillWidth) {
                RectTransform parent = transform.parent as RectTransform;
                float _parentWidth = parent.GetSize().x;
                if (parentLayout != null) _parentWidth -= parentLayout.padding.left + parentLayout.padding.right;
                SetLayoutInputForAxis(_parentWidth, _parentWidth, -1, 0);
            } else {
                int minColumns = columns;
                int preferredColumns = columns;

                SetLayoutInputForAxis(
                    padding.horizontal + (cellWidth + spacing.x) * minColumns - spacing.x,
                    padding.horizontal + (cellWidth + spacing.x) * preferredColumns - spacing.x,
                    -1, 0);
            }

            if (layoutConstraints == LayoutConstraints.Uniform) {
                float sqrRt = Mathf.Sqrt(rectChildren.Count);
                rows = Mathf.RoundToInt(sqrRt);
                columns = Mathf.CeilToInt(sqrRt);
            } else if (layoutConstraints == LayoutConstraints.FixedColumns) {
                rows = Mathf.CeilToInt(rectChildren.Count / (float)columns);
            } else if (layoutConstraints == LayoutConstraints.FixedRows) {
                columns = Mathf.CeilToInt(rectChildren.Count / (float)rows);
            }

            //Make sure columns and rows is at least 1
            rows = Mathf.Max(1, rows);
            columns = Mathf.Max(1, columns);

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float _cellWidth = (parentWidth - (spacing.x * ((float)columns - 1)) - padding.left - padding.right) / columns;
            float _cellHeight = (parentHeight - (spacing.y * ((float)rows - 1)) - padding.top - padding.bottom) / rows;

            cellWidth = fillWidth ? _cellWidth : cellWidth;
            cellHeight = fillHeight ? _cellHeight : cellHeight;

            int columnCount = 0;
            int rowCount = 0;

            int cornerX = ((int)childAlignment + 1) % 3;
            int cornerY = Mathf.FloorToInt((int)childAlignment / 6);

            float requiredSpaceX = columns * cellWidth + (columns - 1) * spacing.x;
            float requiredSpaceY = rows * cellHeight + (rows - 1) * spacing.y;

            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpaceX),
                GetStartOffset(1, requiredSpaceY)
            );

            for (int i = 0, count = rectChildren.Count; i < count; ++i) {
                if (startAxis == Axis.Horizontal) {
                    rowCount = i / columns;
                    columnCount = i % columns;
                } else {
                    rowCount = i % columns;
                    columnCount = i / columns;
                }

                RectTransform item = rectChildren[i];
                float xPos, yPos;

                if (cornerX == 0) {
                    //Right aligned so switch pos
                    xPos = requiredSpaceX - (cellWidth * (columnCount + 1)) - (spacing.x * (columnCount + 1));
                } else {
                    xPos = (cellWidth * columnCount) + (spacing.x * (columnCount - 1));
                }

                if (cornerY == 1) {
                    //Lower aligned so switch pos
                    yPos = requiredSpaceY - (cellHeight * (rowCount + 1)) - (spacing.y * (rowCount + 1));
                } else {
                    yPos = (cellHeight * rowCount) + (spacing.y * (rowCount - 1));
                }

                SetChildAlongAxis(item, 0, startOffset.x + (xPos + spacing.x), cellWidth);
                SetChildAlongAxis(item, 1, startOffset.y + (yPos + spacing.y), cellHeight);
            }

            onLayoutUpdate.Invoke();
        }

        public override void CalculateLayoutInputVertical() {
            if (fillHeight) {
                RectTransform parent = transform.parent as RectTransform;
                float parentHeight = parent.GetSize().y;
                if (parentLayout != null) parentHeight -= parentLayout.padding.top + parentLayout.padding.bottom;
                SetLayoutInputForAxis(parentHeight, parentHeight, -1, 1);
            } else {
                int minRows = rows;
                float minSpace = padding.vertical + (cellHeight + spacing.y) * minRows - spacing.y;
                SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
            }
        }

        public override void SetLayoutVertical() {}
        public override void SetLayoutHorizontal() {}

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();

            CalculateLayoutInputHorizontal();
        }
    }
}
