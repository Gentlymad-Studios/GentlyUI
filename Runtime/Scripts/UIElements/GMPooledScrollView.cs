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
		[Header("Scroll Constraints")]
		[SerializeField] private ScrollAxis scrollAxis = ScrollAxis.Vertical;
		[Header("Buttons")]
		[SerializeField] private GMHoldInteractionButton[] scrollButtons;
		[SerializeField] private bool setScrollbarUninteractable = false;

		public enum ScrollAxis {
			Horizontal = 0,
			Vertical = 1
		}

		public class ViewportUpdateEvent : GentlyUIEvent { }
		private ViewportUpdateEvent onViewportUpdate = new ViewportUpdateEvent();
		public ViewportUpdateEvent OnViewportUpdate {
			get { return onViewportUpdate; }
			set { onViewportUpdate = value; }
		}

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

		private UIObjectPool<UIBehaviour> currentPool;

		private Dictionary<GameObject, UIObjectPool<UIBehaviour>> poolCache = new Dictionary<GameObject, UIObjectPool<UIBehaviour>>();

		private bool isScrolling;
		public bool IsScrolling => isScrolling;

		private float defaultPreferredHeight = 200;
		private float defaultPreferredWidth = 200;

		private bool hasWillRenderCanvasListener = false;

		void UpdateItemPool() {
			UIObjectPool<UIBehaviour> newPool;

			if (!poolCache.ContainsKey(itemPrefab.gameObject)) {
				UIObjectPool<UIBehaviour> pool = new UIObjectPool<UIBehaviour>(() => CreateItem<UIBehaviour>(), OnGetItem, OnReturnItem);
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

		private LayoutElement layoutElement;
		private LayoutElement LayoutElement {
			get {
				if (layoutElement == null) {
					layoutElement = gameObject.GetOrAddComponent<LayoutElement>();
					if (scrollAxisInt == 0) {
						layoutElement.preferredWidth = Mathf.Max(defaultPreferredWidth, layoutElement.preferredWidth);
					} else {
						layoutElement.preferredHeight = Mathf.Max(defaultPreferredHeight, layoutElement.preferredHeight);
					}
				}
				return layoutElement;
			}
		}


		private List<UIBehaviour> currentItems = new List<UIBehaviour>();
		private GMSelectable[] selectables;

		private Action<UIBehaviour, int> onUpdateItem;
		private Action<UIBehaviour> onReturnItem;

		private int totalItemCount;

		private float viewportHeight;
		private float viewportWidth;

		private float totalHeight;
		private float totalWidth;

		private float rowHeight;
		private float columnWidth;
		private float normalizedTargetPosition;

		private bool isInitialized = false;
		private bool isQuitting = false;

		private int scrollAxisInt;
		private Vector2 scrollVector;

		protected override void OnInitialize() {
			base.OnInitialize();

			defaultPreferredHeight = _defaultPreferredHeight = LayoutElement.preferredHeight;
			defaultPreferredWidth = _defaultPrefferedWidth = LayoutElement.preferredWidth;

			targetPosition = Content.anchoredPosition;

			scrollAxisInt = (int)scrollAxis;
			scrollVector = new Vector2(1 - scrollAxisInt, scrollAxisInt);

			if (itemPrefab != null) {
				SetPrefab(itemPrefab);
			}

			//Buttons
			for (int i = 0, count = scrollButtons.Length; i < count; ++i) {
				GMHoldInteractionButton button = scrollButtons[i];
				button.OnClick.AddListener(() => {
					OnScroll(new PointerEventData(EventSystem.current) {
						scrollDelta = Vector2.up * button.scrollDirection
					});
				});
			}
		}

		/****** Scroll Logic *******/
		float scrollValue, scrollValueAbsolute;
		Vector2 delta;

		public virtual void OnScroll(PointerEventData data) {
			if (!IsActive() || !IsScrollingAllowed())
				return;

			delta = data.scrollDelta;
			scrollValue = delta.y;

			if (ScrollInSteps) {
				if (scrollValue > 0) {
					scrollValueAbsolute = scrollAxisInt == 0 ? Mathf.Abs(targetPosition.x) - columnWidth : targetPosition.y - rowHeight;
					SetNormalizedScrollPosition(scrollValueAbsolute / maxScrollPosition);
				} else {
					scrollValueAbsolute = scrollAxisInt == 0 ? Mathf.Abs(targetPosition.x) + columnWidth : targetPosition.y + rowHeight;
					SetNormalizedScrollPosition(scrollValueAbsolute / maxScrollPosition);
				}
			} else {
				scrollValueAbsolute = scrollAxisInt == 0 ? targetPosition.x + delta.x : targetPosition.y - delta.y;
				SetNormalizedScrollPosition(scrollValueAbsolute * ScrollSensitivity / maxScrollPosition);
			}
		}

		public void OnMove(AxisEventData eventData) {

		}

		/****** Move Logic *******/
		float moveDistance, movementSpeed;

		public virtual void Tick(float unscaledDeltaTime) {
			if (!IsScrollingAllowed())
				return;

			if (scrollAxisInt == 0) {
				isScrolling = internalPosition.x != targetPosition.x;

				moveDistance = Mathf.Abs(targetPosition.x - internalPosition.x);
				movementSpeed = moveDistance / EaseDuration;
			} else {
				isScrolling = internalPosition.y != targetPosition.y;

				moveDistance = Mathf.Abs(targetPosition.y - internalPosition.y);
				movementSpeed = moveDistance / EaseDuration;
			}

			//Do nothing if position is reached
			if (!isScrolling) {
				return;
			}

			SetContentAnchoredPosition(Vector2.MoveTowards(internalPosition,
															targetPosition,
															unscaledDeltaTime * movementSpeed));

			///If we are close as half a pixel to the correct position we can snap to the actual position
			if (scrollAxisInt == 0) {
				if (Mathf.Abs(internalPosition.x - targetPosition.x) <= 0.5f) {
					SetContentAnchoredPosition(targetPosition, true);
					OnScrollEnded();
				}
			} else {
				if (Mathf.Abs(internalPosition.y - targetPosition.y) <= 0.5f) {
					SetContentAnchoredPosition(targetPosition, true);
					OnScrollEnded();
				}
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

			Vector3 localPosition = scrollVector * normalizedPosition * maxScrollPosition;

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

			if (scrollAxisInt == 0) {
				numberOfSteps = Mathf.RoundToInt(scrollPosition / columnWidth);
				scrollPosition = Mathf.Clamp(numberOfSteps * columnWidth, 0, maxScrollPosition);
			} else {
				numberOfSteps = Mathf.RoundToInt(scrollPosition / rowHeight);
				scrollPosition = Mathf.Clamp(numberOfSteps * rowHeight, 0, maxScrollPosition);
			}

			return Mathf.InverseLerp(0, maxScrollPosition, scrollPosition);
		}

		void SetTargetPosition(Vector2 position, bool setImmediately = false) {
			if (!setImmediately) setImmediately = MovementType == MovementType.Direct;

			//Clamp scroll position
			float scrollPosition;

			if (scrollAxisInt == 0) {
				scrollPosition = Mathf.Clamp(position.x, 0, maxScrollPosition);
				targetPosition.x = -scrollPosition;
			} else {
				scrollPosition = Mathf.Clamp(position.y, 0, maxScrollPosition);
				targetPosition.y = scrollPosition;
			}

			if (setImmediately) {
				SetContentAnchoredPosition(targetPosition, true);
			}
		}

		protected virtual void SetContentAnchoredPosition(Vector2 position, bool forceUpdate = false) {
			if (position != internalPosition) {
				internalPosition = position;
				//Update anchored position of content
				float pos;

				if (scrollAxisInt == 0) {
					//horizontal scroll is in negative space, so columnWidth needs to be negative too
					pos = internalPosition.x % -columnWidth;
				} else {
					pos = internalPosition.y % rowHeight;
				}

				//Only update viewport if we are between items
				if (pos != 0f || forceUpdate) {
					if (scrollAxisInt == 0) {
						Content.anchoredPosition = new Vector2(pos, Content.anchoredPosition.y);
					} else {
						Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, pos);
					}

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
			float normalizedPosition;

			if (scrollAxisInt == 0) {
				normalizedPosition = columnWidth * Mathf.FloorToInt(index / (float)itemContainer.rows) / maxScrollPosition;
			} else {
				normalizedPosition = rowHeight * Mathf.FloorToInt(index / (float)itemContainer.columns) / maxScrollPosition;
			}

			SetNormalizedScrollPosition(normalizedPosition, true);
			UpdateViewport();
		}

		void UpdateScrollbar() {
			//Maker sure scrollbar sits at correct position and update its size
			if (scrollbar != null) {
				if (IsScrollingAllowed()) {
					scrollbar.SetHandleSize(Mathf.InverseLerp(0, (maxScrollPosition + viewportHeight), viewportHeight));
					if (scrollbar.Value != normalizedTargetPosition) scrollbar.SetValue(normalizedTargetPosition, false);

					if (setScrollbarUninteractable) {
						scrollbar.SetInteractable(true);
					}
				} else {
					scrollbar.SetHandleSize(1f);
					if (scrollbar.Value != 0f) scrollbar.SetValue(0f, false);

					if (setScrollbarUninteractable) {
						scrollbar.SetInteractable(false);
					}
				}
			}

			//Buttons
			for (int i = 0, count = scrollButtons.Length; i < count; ++i) {
				GMHoldInteractionButton button = scrollButtons[i];
				if (button.scrollDirection < 0) {
					button.SetInteractable(normalizedTargetPosition < 1f && IsScrollingAllowed());
				} else if (button.scrollDirection > 0) {
					button.SetInteractable(normalizedTargetPosition > 0f && IsScrollingAllowed());
				}
			}
		}

		int dataIndexOffset, currentColumnIndex, currentRowIndex;

		void UpdateViewport(bool forceUpdate = false) {
			int _lastStartIndex = currentDataStartIndex;

			if (scrollAxisInt == 0) {
				currentColumnIndex = Mathf.FloorToInt(Mathf.Abs(internalPosition.x) / columnWidth);
				currentColumnIndex = Mathf.Max(currentColumnIndex, 0);
				currentDataStartIndex = currentColumnIndex * itemContainer.rows;
			} else {
				currentRowIndex = Mathf.FloorToInt(internalPosition.y / rowHeight);
				currentRowIndex = Mathf.Max(currentRowIndex, 0);
				currentDataStartIndex = currentRowIndex * itemContainer.columns;
			}

			bool wasScrolled = _lastStartIndex != currentDataStartIndex;

			if (wasScrolled || forceUpdate) {
				for (int i = 0, count = currentItems.Count; i < count; ++i) {
					UIBehaviour item = currentItems[i];
					//We use the sibling index instead of the index in the loop, because it might happen, that items were reordered.
					//So sibling index is more reliable on which data to display in this item.
					dataIndexOffset = item.transform.GetSiblingIndex();
					OnUpdateItem(item, currentDataStartIndex + dataIndexOffset, wasScrolled);
				}

				if (onViewportUpdate != null) {
					onViewportUpdate.Invoke();
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

			AddWillRenderCanvasCallback();
		}

		void AddWillRenderCanvasCallback() {
			if (!hasWillRenderCanvasListener && !isQuitting) {
				Canvas.willRenderCanvases += OnWillRenderCanvases;
				hasWillRenderCanvasListener = true;
			}
		}

		void RemoveWillRenderCanvasCallback() {
			if (hasWillRenderCanvasListener) {
				Canvas.willRenderCanvases -= OnWillRenderCanvases;
				hasWillRenderCanvasListener = false;
			}
		}

		void SetPrefab(MonoBehaviour prefab) {
			if (prefab == null) {
				return;
			}

			itemPrefab = prefab;

			LayoutElement _layout = prefab.GetComponent<LayoutElement>();
			if (_layout != null) {
				if (scrollAxisInt == 1) {
					float height = Mathf.Max(_layout.minHeight, _layout.preferredHeight);
					if (height > 0) {
						ItemContainer.cellHeight = height;
					} else {
						RectTransform prefabRectT = prefab.transform as RectTransform;
						ItemContainer.cellHeight = prefabRectT.GetHeight();
					}

				} else {
					float width = Mathf.Max(_layout.minWidth, _layout.preferredWidth);
					if (width > 0) {
						ItemContainer.cellWidth = width;
					} else {
						RectTransform prefabRectT = prefab.transform as RectTransform;
						ItemContainer.cellHeight = prefabRectT.GetWidth();
					}
				}
			}

			UpdateItemPool();
		}

		/****** Pooling Logic *******/
		private int currentDataStartIndex = 0;
		private Type currentType;

		/// <summary>
		/// Initializes the pooled scroll view with a custom prefab.
		/// </summary>
		public void Initialize<T>(
			MonoBehaviour prefab,
			int defaultCount,
			Action<UIBehaviour, int> onUpdateItem,
			Action<UIBehaviour> onReturnItem = null
		) where T : UIBehaviour {
			SetPrefab(prefab);
			Initialize<T>(defaultCount, onUpdateItem, onReturnItem);
		}

		public void Initialize<T>(
			int defaultCount,
			Action<UIBehaviour, int> onUpdateItem,
			Action<UIBehaviour> onReturnItem = null
		) where T : UIBehaviour {
			currentType = typeof(T);

			this.onUpdateItem = onUpdateItem;
			this.onReturnItem = onReturnItem;

			totalItemCount = defaultCount;
			isInitialized = true;

			Setup(true);
		}

		public void UpdateItemCount(int itemCount) {
			totalItemCount = itemCount;

			UpdateAll();
		}

		void UpdateAll() {
			UpdateSizes();
			UpdateScrollbar();
			UpdateAllDisplayedItems();

			if (targetPosition.y > maxScrollPosition) {
				SetTargetPosition(Vector3.up * maxScrollPosition, true);
			}
		}

		public void AutoAdjustToScrollviewSettings() {
			if (scrollAxisInt == 0) {
				//Horizontal scrolling
				LayoutElement.preferredHeight = itemContainer.rows * rowHeight - itemContainer.spacing.y + itemContainer.padding.top + itemContainer.padding.bottom + viewport.offsetMin.y + viewport.offsetMax.y * -1;
			} else {
				//Vertical scrolling
				LayoutElement.preferredWidth = itemContainer.columns * columnWidth - itemContainer.spacing.x + itemContainer.padding.left + itemContainer.padding.right + viewport.offsetMin.x + viewport.offsetMax.x * -1;
			}
		}

		public void SetRows(int rows) {
			ItemContainer.rows = rows;
			UpdateAll();
		}

		public void SetColumns(int columns) {
			ItemContainer.columns = columns;
			UpdateAll();
		}

		public void SetRowsAndColumns(int rows, int columns) {
			ItemContainer.rows = rows;
			ItemContainer.columns = columns;
			UpdateAll();
		}

		public void SetSize(float width, float height) {
			if (scrollAxisInt == 0) {
				LayoutElement.preferredHeight = height;
				defaultPreferredWidth = width;
			} else {
				LayoutElement.preferredWidth = width;
				defaultPreferredHeight = height;
			}

			UpdateAll();
		}

		public void SetHeight(float height) {
			if (scrollAxisInt == 0) {
				LayoutElement.preferredHeight = height;
			} else {
				defaultPreferredHeight = height;
				UpdateAll();
			}
		}

		public void SetWidth(float width) {
			if (scrollAxisInt == 1) {
				LayoutElement.preferredWidth = width;
			} else {
				defaultPreferredWidth = width;
				UpdateAll();
			}
		}

		public void SetCellSize(float cellWidth, float cellHeight) {
			itemContainer.cellWidth = cellWidth;
			itemContainer.cellHeight = cellHeight;

			UpdateAll();
		}

		void Setup(bool goToTop = false) {
			if (!isInitialized || itemPrefab == null)
				return;

			UpdateSizes();
			UpdateScrollbar();
			SpawnItems();

			if (goToTop) {
				SnapToElement(0);
			}
		}

		void UpdateSizes() {
			rowHeight = itemContainer.cellHeight + itemContainer.spacing.y;
			columnWidth = itemContainer.cellWidth + itemContainer.spacing.x;

			//If scrolling in steps we want to have the height to be a multiple of row height
			if (settings.ScrollInSteps) {
				if (scrollAxisInt == 0) {
					float newPreferredWidth = columnWidth * Mathf.CeilToInt(defaultPreferredWidth / columnWidth) + itemContainer.padding.left;
					LayoutElement.preferredWidth = LayoutElement.minWidth = newPreferredWidth;
				} else {
					float newPreferredHeight = rowHeight * Mathf.CeilToInt(defaultPreferredHeight / rowHeight) + itemContainer.padding.top;
					LayoutElement.preferredHeight = LayoutElement.minHeight = newPreferredHeight;
				}
			}

			viewportHeight = viewport.GetHeight();
			viewportWidth = viewport.GetWidth();

			totalHeight = Mathf.CeilToInt(totalItemCount / (float)itemContainer.columns) * rowHeight + itemContainer.padding.bottom + itemContainer.padding.top - itemContainer.spacing.y;
			totalWidth = Mathf.CeilToInt(totalItemCount / (float)itemContainer.rows) * columnWidth + itemContainer.padding.left + itemContainer.padding.right - itemContainer.spacing.x;

			UpdateMaxScrollPosition();
		}

		int CalculateMaxItemsToSpawn() {
			//The maximum number of items to show is the maximum number of rows * items per row which is defined in the layout group
			//We need one more row to allow scrolling!
			if (scrollAxisInt == 0) {
				int maxColumnsToShow = Mathf.CeilToInt(viewportWidth / columnWidth);
				return maxColumnsToShow * itemContainer.rows + itemContainer.rows;
			} else {
				int maxRowsToShow = Mathf.CeilToInt(viewportHeight / rowHeight);
				return maxRowsToShow * itemContainer.columns + itemContainer.columns;
			}
		}

		void UpdateMaxScrollPosition() {
			if (scrollAxisInt == 0) {
				maxScrollPosition = Mathf.Max(0, totalWidth - viewportWidth);
			} else {
				maxScrollPosition = Mathf.Max(0, totalHeight - viewportHeight);
			}
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

		protected virtual T CreateItem<T>() where T : UIBehaviour {
			GameObject itemGO = Instantiate(itemPrefab.gameObject, itemContainer.transform, false);

			T item = itemGO.GetComponent<T>();

			if (item is IInitializeUIOnSpawn initializeOnSpawn) {
				initializeOnSpawn.Initialize();
			}

			return item;
		}

		protected virtual void OnGetItem<T>(T item) where T : UIBehaviour {
			currentItems.Add(item.GetComponent(currentType) as UIBehaviour);
			item.transform.SetAsLastSibling();
		}

		protected virtual void OnReturnItem<T>(T item) where T : UIBehaviour {
			if (onReturnItem != null) {
				onReturnItem(item);
			}
			currentItems.Remove(item);
		}

		void OnUpdateItem<T>(T item, int dataIndex, bool wasScrolled) where T : UIBehaviour {
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
			AddWillRenderCanvasCallback();

			base.OnEnable();

			SetNormalizedScrollPosition(0, true);
			if (scrollbar != null) scrollbar.OnValueChanged.AddListener((float value) => SetNormalizedScrollPosition(value, false, false));
		}

		protected override void OnDisable() {
			Application.quitting -= OnIsQuitting;
			RemoveWillRenderCanvasCallback();

			if (scrollbar != null) scrollbar.OnValueChanged.RemoveListener((float value) => SetNormalizedScrollPosition(value, false, false));

			base.OnDisable();
		}

        protected override void OnDestroy() {
			RemoveWillRenderCanvasCallback();

			base.OnDestroy();
        }

        void OnWillRenderCanvases() {
			//Remove callback as we only want to do this once after OnEnable() and OnRectTransformDimensionsChanged()
			RemoveWillRenderCanvasCallback();
			Setup();
		}


		#region <!--------- Pool Cache ---------!>
		int defaultColumns;
		int defaultRows;
		float defaultCellWidth;
		float defaultCellHeight;
		float _defaultPreferredHeight;
		float _defaultPrefferedWidth;
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

			defaultPreferredWidth = _defaultPrefferedWidth;
			defaultPreferredHeight = _defaultPreferredHeight;
		}
		#endregion

		void OnIsQuitting() {
			isQuitting = true;
			RemoveWillRenderCanvasCallback();
		}

		void ClearPool() {
			//Returns items. OnReturn will remove them from currentItems.
			while (currentItems.Count > 0) {
				currentPool.Return(currentItems[0]);
			}
		}

		public List<UIBehaviour> GetAllItems(bool excludeInvisible = false) {
			if (excludeInvisible) {
				List<UIBehaviour> visibleItems = new List<UIBehaviour>();

				for (int i = 0, count = currentItems.Count; i < count; ++i) {
					UIBehaviour item = currentItems[i];
					if (item.gameObject.activeSelf) {
						visibleItems.Add(item);
					}
				}

				return visibleItems;
			} else {
				return currentItems;
			}
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
