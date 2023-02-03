using GentlyUI.UIElements;
using System.IO;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected FlexibleGridLayout AddContent_GridFixedColumns(int columnCount, Vector2 spacing, RectOffset padding = null) {
            FlexibleGridLayout flexibleGridLayout = SpawnGrid(spacing, padding);
            flexibleGridLayout.layoutConstraints = FlexibleGridLayout.LayoutConstraints.FixedColumns;
            flexibleGridLayout.columns = columnCount;
            flexibleGridLayout.fillWidth = true;
            flexibleGridLayout.fillHeight = true;
            return flexibleGridLayout;
        }

        protected FlexibleGridLayout AddContent_GridFixedColumns(int columnCount, float cellHeight, Vector2 spacing, RectOffset padding = null) {
            FlexibleGridLayout grid = AddContent_GridFixedColumns(columnCount, spacing, padding);

            grid.fillHeight = false;
            grid.cellHeight = cellHeight;

            return grid;
        }

        protected FlexibleGridLayout AddContent_GridFixedColumns(int columnCount, float cellHeight, float cellWidth, Vector2 spacing, RectOffset padding = null) {
            FlexibleGridLayout grid = AddContent_GridFixedColumns(columnCount, cellHeight, spacing, padding);

            grid.fillWidth = false;
            grid.cellWidth = cellWidth;

            return grid;
        }

        protected FlexibleGridLayout AddContent_GridFixedRows(int rowCount, Vector2 spacing, RectOffset padding = null) {
            FlexibleGridLayout flexibleGridLayout = SpawnGrid(spacing, padding);
            flexibleGridLayout.layoutConstraints = FlexibleGridLayout.LayoutConstraints.FixedRows;
            flexibleGridLayout.rows = rowCount;
            flexibleGridLayout.fillWidth = true;
            flexibleGridLayout.fillHeight = true;
            return flexibleGridLayout;
        }

        protected FlexibleGridLayout AddContent_GridFixedRows(int rowCount, float cellWidth, Vector2 spacing, RectOffset padding = null) {
            FlexibleGridLayout grid = AddContent_GridFixedRows(rowCount, spacing, padding);

            grid.fillWidth = false;
            grid.cellWidth = cellWidth;

            return grid;
        }

        protected FlexibleGridLayout AddContent_GridFixedRows(int rowCount, float cellWidth, float cellHeight, Vector2 spacing, RectOffset padding = null) {
            FlexibleGridLayout grid = AddContent_GridFixedRows(rowCount, cellWidth, spacing, padding);

            grid.fillHeight = false;
            grid.cellHeight = cellHeight;

            return grid;
        }

        FlexibleGridLayout SpawnGrid(Vector2 spacing, RectOffset padding = null) {
            GameObject gridGO = new GameObject("grid", typeof(RectTransform));
            gridGO.transform.SetParent(currentContainer, false);
            gridGO.transform.SetSiblingIndex(currentHierarchyOrder);

            FlexibleGridLayout flexibleGridLayout = gridGO.AddComponent<FlexibleGridLayout>();
            if (padding != null) {
                flexibleGridLayout.padding = padding;
            }
            flexibleGridLayout.spacing = spacing;

            RectTransform flexibleGridRectT = flexibleGridLayout.transform as RectTransform;
            flexibleGridRectT.anchorMin = Vector2.zero;
            flexibleGridRectT.anchorMax = Vector2.one;
            flexibleGridRectT.SetOffset(0, 0, 0, 0);

            CacheDynamicLayout(gridGO);

            IncrementCurrentHierarchyOrder();

            SetCurrentContainer(flexibleGridRectT);

            return flexibleGridLayout;
        }
    }
}
