using System;
using UnityEngine;

public static class RectTransformExtensions {
    public static void AnchorToCorners(this RectTransform transform) {
        if (transform == null)
            throw new ArgumentNullException("transform");

        if (transform.parent == null)
            return;

        var parent = transform.parent.GetComponent<RectTransform>();

        Vector2 newAnchorsMin = new Vector2(transform.anchorMin.x + transform.offsetMin.x / parent.rect.width,
                          transform.anchorMin.y + transform.offsetMin.y / parent.rect.height);

        Vector2 newAnchorsMax = new Vector2(transform.anchorMax.x + transform.offsetMax.x / parent.rect.width,
                          transform.anchorMax.y + transform.offsetMax.y / parent.rect.height);

        transform.anchorMin = newAnchorsMin;
        transform.anchorMax = newAnchorsMax;
        transform.offsetMin = transform.offsetMax = new Vector2(0, 0);
    }

    public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec) {
        trans.pivot = aVec;
        trans.anchorMin = aVec;
        trans.anchorMax = aVec;
    }

    public static Vector2 GetSize(this RectTransform trans) {
        return trans.rect.size;
    }

    public static float GetWidth(this RectTransform trans) {
        return trans.rect.width;
    }

    public static float GetHeight(this RectTransform trans) {
        return trans.rect.height;
    }

    public static void SetSize(this RectTransform trans, Vector2 newSize) {
        Vector2 oldSize = trans.rect.size;
        Vector2 deltaSize = newSize - oldSize;
        trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
        trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
    }

    public static void SetWidth(this RectTransform trans, float newSize) {
        SetSize(trans, new Vector2(newSize, trans.rect.size.y));
    }

    public static void SetHeight(this RectTransform trans, float newSize) {
        SetSize(trans, new Vector2(trans.rect.size.x, newSize));
    }

    public static void SetBottomLeftPosition(this RectTransform trans, Vector2 newPos) {
        trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
    }

    public static void SetTopLeftPosition(this RectTransform trans, Vector2 newPos) {
        trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
    }

    public static void SetBottomRightPosition(this RectTransform trans, Vector2 newPos) {
        trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
    }

    public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos) {
        trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
    }

    public static void SetOffset(this RectTransform trans, float left, float right, float top, float bottom) {
        trans.offsetMin = new Vector2(left, bottom);
        trans.offsetMax = new Vector2(-right, -top);
    }

    public static void SetSiblingIndex(this Transform trans, int index, bool moveInactiveSiblingsToBack) {
        if (moveInactiveSiblingsToBack && trans.parent != null) {
            for (int i = 0, count = index; i < count; ++i) {
                if (i >= trans.parent.childCount)
                    continue;

                Transform child = trans.parent.GetChild(i);

                //Move inactive childs out of the way
                if (child != trans && !child.gameObject.activeSelf) {
                    child.transform.SetAsLastSibling();
                }
            }
        }

        trans.SetSiblingIndex(index);
    }
}