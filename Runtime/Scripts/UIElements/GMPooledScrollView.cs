using GentlyUI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GentlyUI.UIElements {
    [AddComponentMenu("GentlyUI/Generic Pooled ScrollView", 10)]
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class GMPooledScrollView : Core.UIBase, IScrollHandler, IMoveHandler, IUITickable, IPooledUIResetter {
        [SerializeField] private MonoBehaviour itemPrefab;
        [SerializeField] private FlexibleGridLayout itemContainer;
        [SerializeField] private RectTransform content;
        public RectTransform Content => content;
        public FlexibleGridLayout ItemContainer => itemContainer;

        public MonoBehaviour ItemPrefab {
            get { return itemPrefab; }
            set { SetPrefab(value); }
        }

        /// <summary>
        /// The area in which items are visible. Will be the rectTransform this script is attached to if not assigned.
        /// </summary>
        [UnityEngine.Tooltip("The area in which items are visible. Will be the rectTransform this script is attached to if not assigned.")]
        [SerializeField] private RectTransform viewport;
        public RectTransform Viewport {
            get {
                if (viewport == null) {
                    viewport = (RectTransform)transform;
                }
                return viewport;
            }
        }

        [Header("Configuration")]
        /// <summary>
        /// Assign a settings file to use as a preset for different configurations.
        /// </summary>
        [UnityEngine.Tooltip("Assign a settings file to use as a preset for different configurations.")]
        [SerializeField] private UIScrollViewSettings settings;

        /// <summary>
        /// Set to true if the scroll view should scroll row by row instead of fluent movements.
        /// </summary>
        [UnityEngine.Tooltip("Set to true if the scroll view should scroll row by row instead of fluent movements.")]
        [SerializeField] private bool scrollInSteps = false;
        private bool ScrollInSteps {
            get {
                if (settings != null) return settings.ScrollInSteps;
                else return scrollInSteps;
            }
        }

        /// <summary>
        /// Defines whether scrolling should stop immediately (direct) or smoothly (eased) when no input is dected.
        /// </summary>
        [UnityEngine.Tooltip("Defines whether scrolling should stop immediately (direct) or smoothly (eased) when no input is dected.")]
        [SerializeField] private MovementType movementType = MovementType.Direct;
        private MovementType MovementType {
            get {
                if (settings != null) return settings.MovementType;
                else return movementType;
            }
        }

        /// <summary>
        /// How fast should the scroll view scroll?
        /// </summary>
        [UnityEngine.Tooltip("How fast should the scroll view scroll?")]
        [SerializeField] private float scrollSensitivity = 50;
        private float ScrollSensitivity {
            get {
                if (settings != null) return settings.ScrollSensitivity;
                else return scrollSensitivity;
            }
        }

        /// <summary>
        /// The ease duration in seconds. This defines how long it takes for the scroll movement to catch up to the target position.
        /// </summary>
        [UnityEngine.Tooltip("The ease duration in seconds. This defines how long it takes for the scroll movement to catch up to the target position.")]
        [SerializeField] private float easeDuration = 0.15f;
        private float EaseDuration {
            get {
                if (settings != null) return settings.EaseDuration;
                else return easeDuration;
            }
        }

        /// <summary>
        /// The scrollbar! Will be automatically shown or hidden based on user input/interaction.
        /// </summary>
        [UnityEngine.Tooltip("The scrollbar! Will be automatically shown or hidden based on user input/interaction.")]
        [SerializeField] private GMScrollbar scrollbar;

        /// <summary>
        /// Fake position that simulates world scrolling.
        /// But we actually snap the content rect transform back.
        /// </summary>
        private Vector2 internalPosition;
        private Vector2 targetPosition;
        private float maxScrollPosition;

        private UIObjectPool<Behaviour> currentPool;

        private Dictionary<GameObject, UIObjectPool<Behaviour>> poolCache = new Dictionary<GameObject, UIObjectPool<Behaviour>>();

        private Coroutine waitForLayoutCompleteRoutine;

        private bool isScrolling;
        public bool IsScrolling => isScrolling;

        void UpdateItemPool() {
            UIObjectPool<Behaviour> newPool;

            if (!poolCache.ContainsKey(itemPrefab.gameObject)) {
                UIObjectPool<Behaviour> pool = new UIObjectPool<Behaviour>(() => CreateItem<Behaviour>(), OnGetItem, OnReturnItem);
                poolCache.Add(itemPrefab.gameObject, pool);
                newPool = pool;
            } else {
                newPool = poolCache[itemPrefab.gameObject];
            }

            if (newPool != currentPool) {
                if (currentPool != null) {
                    ClearPool();
                }

                currentPool = newPool;
            }
        }

        private float defaultPreferredHeight = 200;

        private LayoutElement layoutElement;
        private LayoutElement LayoutElement {
            get {
                if (layoutElement == null) {
                    layoutElement = gameObject.GetOrAddComponent<LayoutElement>();
                    layoutElement.preferredHeight = Mathf.Max(defaultPreferredHeight, layoutElement.preferredHeight);
                }
                return layoutElement;
            }
        }


        private List<Behaviour> currentItems = new List<Behaviour>();
        private GMSelectable[] selectables;

        private Action<Behaviour, int> onUpdateItem;
        private Action<Behaviour> onReturnItem;

        private int totalItemCount;

        private float viewportHeight;
        private float totalHeight;

        private float rowHeight;
        private float normalizedTargetPosition;

        private bool isInitialized = false;
        private bool isQuitting = false;

        protected override void OnInitialize() {
            base.OnInitialize();

            defaultPreferredHeight = LayoutElement.preferredHeight;

            if (itemPrefab != null) {
                SetPrefab(itemPrefab);
            }
        }

        /****** Scroll Logic *******/
        public virtual void OnScroll(PointerEventData data) {
            if (!IsActive() || !IsScrollingAllowed())
                return;

            Vector2 delta = data.scrollDelta;

            if (ScrollInSteps) {
                if (delta.y > 0) {
                    SetNormalizedScrollPosition((targetPosition.y - rowHeight) / maxScrollPosition);
                } else {
                    SetNormalizedScrollPosition((targetPosition.y + rowHeight) / maxScrollPosition);
                }
            } else {
                SetNormalizedScrollPosition((targetPosition.y
                                             - delta.y
                                             * ScrollSensitivity)
                                                / maxScrollPosition);
            }
        }

        public void OnMove(AxisEventData eventData) {

        }

        public virtual void Tick(float unscaledDeltaTime) {
            if (!IsScrollingAllowed())
                return;

            isScrolling = internalPosition.y != targetPosition.y;

            //Do nothing if position is reached
            if (!isScrolling) {
                return;
            }

            float moveDistance = Mathf.Abs(targetPosition.y - internalPosition.y);
            float movementSpeed = moveDistance / EaseDuration;

            SetContentAnchoredPosition(Vector2.MoveTowards(internalPosition,
                                                            targetPosition,
                                                            unscaledDeltaTime * movementSpeed));

            ///If we are close as half a pixel to the correct position we can snap to the actual position
            if (Mathf.Abs(internalPosition.y - targetPosition.y) <= 0.5f) {
                SetContentAnchoredPosition(targetPosition, true);
                OnScrollEnded();
            }
        }

        void OnScrollEnded() {
            isScrolling = false;

            for (int i = 0, count = selectables.Length; i < count; ++i) {
                selectables[i].UpdateVisualState();
            }
        }

        void SetNormalizedScrollPosition(float normalizedPosition, bool setImmediately = false, bool considerScrollInSteps = true) {
            if (!IsScrollingAllowed())
                return;

            normalizedPosition = Mathf.Clamp01(normalizedPosition);

            if (ScrollInSteps && considerScrollInSteps) {
                normalizedPosition = CalculateNormalizedScrollStep(normalizedPosition);
            }

            normalizedTargetPosition = normalizedPosition;

            Vector3 localPosition = Vector3.up * normalizedPosition * maxScrollPosition;

            SetTargetPosition(localPosition, setImmediately);
        }

        float CalculateNormalizedScrollStep(float defaultNormalizedPosition) {
            if (Mathf.Approximately(defaultNormalizedPosition, 1f)) {
                return 1f;
            } else if (Mathf.Approximately(defaultNormalizedPosition, 0f)) {
                return 0;
            }

            float scrollPosition = defaultNormalizedPosition * maxScrollPosition;
            int numberOfSteps = 0;

            numberOfSteps = Mathf.RoundToInt(scrollPosition / rowHeight);

            scrollPosition = Mathf.Clamp(numberOfSteps * rowHeight, 0, maxScrollPosition);
            return Mathf.InverseLerp(0, maxScrollPosition, scrollPosition);
        }

        void SetTargetPosition(Vector2 position, bool setImmediately = false) {
            if (!setImmediately) setImmediately = MovementType == MovementType.Direct;

            //Clamp scroll position
            float scrollPosition = Mathf.Clamp(position.y, 0, maxScrollPosition);
            targetPosition = new Vector2(Content.anchoredPosition.x, scrollPosition);

            if (setImmediately) {
                SetContentAnchoredPosition(targetPosition, true);
            }
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position, bool forceUpdate = false) {
            if (position != internalPosition) {
                internalPosition = position;
                //Update anchored position of content
                float yPos = internalPosition.y % rowHeight;

                //Only update viewport if we are between items
                if (yPos != 0f || forceUpdate) {
                    Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, yPos);
                    UpdateViewport();
                }

                UpdateScrollbar();
            }
        }

        /// <summary>
        /// Move the scroll position to an element from data list.
        /// </summary>
        /// <param name="index">The index of the element in the data list.</param>
        /// <param name="immediately">Should we jump immediately or animate there?</param>
        public void SnapToElement(int index, bool immediately = true) {
            float normalizedPosition = (rowHeight * Mathf.FloorToInt(index / (float)itemContainer.columns)) / maxScrollPosition;
            SetNormalizedScrollPosition(normalizedPosition, true);
            UpdateViewport();
        }

        void UpdateScrollbar() {
            //Maker sure scrollbar sits at correct position and update its size
            if (scrollbar != null) {
                if (IsScrollingAllowed()) {
                    scrollbar.SetHandleSize(Mathf.InverseLerp(0, (maxScrollPosition + viewportHeight), viewportHeight));
                    if (scrollbar.Value != normalizedTargetPosition) scrollbar.SetValue(normalizedTargetPosition, false);
                } else {
                    scrollbar.SetHandleSize(1f);
                    if (scrollbar.Value != 0f) scrollbar.SetValue(0f, false);
                }
            }
        }

        void UpdateViewport(bool forceUpdate = false) {
            int _lastStartIndex = currentDataStartIndex;
            int currentRowIndex = Mathf.FloorToInt(internalPosition.y / rowHeight);
            currentRowIndex = Mathf.Max(currentRowIndex, 0);
            currentDataStartIndex = currentRowIndex * itemContainer.columns;

            bool wasScrolled = _lastStartIndex != currentDataStartIndex;

            if (wasScrolled || forceUpdate) {
                for (int i = 0, count = currentItems.Count; i < count; ++i) {
                    OnUpdateItem(currentItems[i], currentDataStartIndex + i, wasScrolled);
                }
            }
        }

        /// <summary>
        /// Force updates all visible items of the scroll view.
        /// Make sure to have the onUpdateItem callback correctly updating all info of the item.
        /// </summary>
        public void UpdateAllDisplayedItems() {
            if (isInitialized) {
                UpdateViewport(true);
            }
        }

        bool IsScrollingAllowed() {
            return maxScrollPosition > 0f;
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();

            if (isQuitting)
                return;

            if (waitForLayoutCompleteRoutine == null) {
                waitForLayoutCompleteRoutine = UIManager.Instance.StartCoroutine(WaitForLayoutComplete());
            }
        }

        IEnumerator WaitForLayoutComplete() {
            yield return new WaitForLayoutComplete();
            Setup();
        }

        void SetPrefab(MonoBehaviour prefab) {
            if (prefab == null) {
                return;
            }

            itemPrefab = prefab;

            LayoutElement _layout = prefab.GetComponent<LayoutElement>();
            if (_layout != null) {
                float height = Mathf.Max(_layout.minHeight, _layout.preferredHeight);
                if (height > 0) {
                    ItemContainer.cellHeight = height;
                } else {
                    //Default height = 50
                    ItemContainer.cellHeight = 50f;
                }
            }

            UpdateItemPool();
        }

        /****** Pooling Logic *******/
        private int currentDataStartIndex = 0;

        /// <summary>
        /// Initializes the pooled scroll view with a custom prefab.
        /// </summary>
        public void Initialize<T>(
            MonoBehaviour prefab,
            int defaultCount,
            Action<Behaviour, int> onUpdateItem,
            Action<Behaviour> onReturnItem = null
        ) where T : Behaviour {
            SetPrefab(prefab);
            Initialize<T>(defaultCount, onUpdateItem, onReturnItem);
        }

        public void Initialize<T>(
            int defaultCount,
            Action<Behaviour, int> onUpdateItem,
            Action<Behaviour> onReturnItem = null
        ) where T : Behaviour {
            this.onUpdateItem = onUpdateItem;
            this.onReturnItem = onReturnItem;

            totalItemCount = defaultCount;
            isInitialized = true;

            Setup(true);
        }

        void Setup(bool goToTop = false) {
            if (!isInitialized || itemPrefab == null)
                return;

            UpdateHeights();
            UpdateScrollbar();
            SpawnItems();

            if (goToTop) {
                SnapToElement(0);
            }
        }

        void UpdateHeights() {
            rowHeight = itemContainer.cellHeight + itemContainer.spacing.y;

            //If scrolling in steps we want to have the height to be a multiple of row height
            if (settings.ScrollInSteps) {
                float newPreferredHeight = rowHeight * Mathf.CeilToInt(defaultPreferredHeight / rowHeight) + itemContainer.padding.top;
                LayoutElement.preferredHeight = newPreferredHeight;
            }

            viewportHeight = viewport.GetHeight();
            totalHeight = Mathf.CeilToInt(totalItemCount / (float)itemContainer.columns) * rowHeight + itemContainer.padding.bottom + itemContainer.padding.top - itemContainer.spacing.y;

            UpdateMaxScrollPosition();
        }

        int CalculateMaxItemsToSpawn() {
            int maxRowsToShow = Mathf.CeilToInt(viewportHeight / rowHeight);
            //The maximum number of items to show is the maximum number of rows * items per row which is defined in the layout group
            //We need one more row to allow scrolling!
            int itemsPerRow = itemContainer.columns;
            int maxItemsToShow = maxRowsToShow * itemContainer.columns + itemsPerRow;
            //Either spawn items until maxItemsToShow is reached or the actual
            //number from the list data if its count is less than maxItemsToShow.
            maxItemsToShow = Mathf.Min(maxItemsToShow, totalItemCount);
            return maxItemsToShow;
        }

        void UpdateMaxScrollPosition() {
            maxScrollPosition = Mathf.Max(0, totalHeight - viewportHeight);
        }

        /// <summary>
        /// Spawns items pooled and tries to fill viewport.
        /// </summary>
        void SpawnItems() {
            if (itemContainer == null) {
                Debug.LogError("Item Container is null!");
                return;
            }

            int spawnCount = CalculateMaxItemsToSpawn();

            //Remove unneeded items
            for (int i = currentItems.Count - 1; i > spawnCount; --i) {
                currentPool.Return(currentItems[i]);
            }

            //Get new items
            for (int i = currentItems.Count; i < spawnCount; ++i) {
                currentPool.Get(itemContainer.transform);
            }

            //Cache selectables
            selectables = Content.GetComponentsInChildren<GMSelectable>(true);

            UpdateViewport(true);
        }

        protected virtual T CreateItem<T>() where T : Behaviour {
            GameObject itemGO = Instantiate(itemPrefab.gameObject, itemContainer.transform, false);
            T item = itemGO.GetComponent<T>();
            return item;
        }

        protected virtual void OnGetItem<T>(T item) where T : Behaviour {
            currentItems.Add(item);
            item.transform.SetAsLastSibling();
        }

        protected virtual void OnReturnItem<T>(T item) where T : Behaviour {
            if (onReturnItem != null) {
                onReturnItem(item);
            }
            currentItems.Remove(item);
        }

        void OnUpdateItem<T>(T item, int dataIndex, bool wasScrolled) where T : Behaviour {
            //Should the item be shown?
            bool showItem = dataIndex < totalItemCount;
            //Toggle visibility
            if (item.gameObject.activeSelf != showItem) item.gameObject.SetActive(showItem);
            //Callback if item is visible
            if (showItem) {
                //Callback
                onUpdateItem(item, dataIndex);
            }
        }

        protected override void OnEnable() {
            Application.quitting += OnIsQuitting;

            base.OnEnable();

            SetNormalizedScrollPosition(0, true);
            if (scrollbar != null) scrollbar.OnValueChanged.AddListener((float value) => SetNormalizedScrollPosition(value, false, false));
        }

        protected override void OnDisable() {
            Application.quitting -= OnIsQuitting;

            if (scrollbar != null) scrollbar.OnValueChanged.RemoveListener((float value) => SetNormalizedScrollPosition(value, false, false));
            if (waitForLayoutCompleteRoutine != null) {
                UIManager.Instance.StopCoroutine(waitForLayoutCompleteRoutine);
            }

            base.OnDisable();
        }


        #region <!--------- Pool Cache ---------!>
        int defaultColumns;
        int defaultRows;
        float defaultCellWidth;
        float defaultCellHeight;
        FlexibleGridLayout.LayoutConstraints defaultLayoutConstraints;

        public void CreatePooledUICache() {
            defaultColumns = ItemContainer.columns;
            defaultRows = ItemContainer.rows;
            defaultCellWidth = ItemContainer.cellWidth;
            defaultCellHeight = ItemContainer.cellHeight;
            defaultLayoutConstraints = ItemContainer.layoutConstraints;
        }

        public void ResetPooledUI() {
            ItemContainer.columns = defaultColumns;
            ItemContainer.rows = defaultRows;
            ItemContainer.cellWidth = defaultCellWidth;
            ItemContainer.cellHeight = defaultCellHeight;
            ItemContainer.layoutConstraints = defaultLayoutConstraints;
        }
        #endregion

        void OnIsQuitting() {
            isQuitting = true;
        }

        void ClearPool() {
            while (currentItems.Count > 0) {
                currentPool.Return(currentItems[0]);
            }

            currentItems.Clear();
        }

        public void Dispose() {
            ClearPool();
            onUpdateItem = null;
            onReturnItem = null;
            isInitialized = false;
        }
    }

    public enum MovementType {
        Direct = 0,
        Eased = 1,
    }
}

public class WaitForLayoutComplete : CustomYieldInstruction {
    public override bool keepWaiting => CanvasUpdateRegistry.IsRebuildingLayout();
}
