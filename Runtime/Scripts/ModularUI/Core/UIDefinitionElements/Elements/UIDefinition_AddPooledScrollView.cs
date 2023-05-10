using GentlyUI.UIElements;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMPooledScrollView AddPooledScrollView<T>(
            MonoBehaviour scrollViewItemPrefab,
            int initialCount,
            Action<UIBehaviour, int> onUpdateItem,
            int columns = 1,
            int cellHeight = 50,
            Action<UIBehaviour> onReturnItem = null
        ) where T : UIBehaviour {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, UIManager.UISettings.DefaultPooledScrollView);

            GMPooledScrollView pooledScrollView = UISpawner<GMPooledScrollView>.SpawnUI(path, currentContainer);
            //Cache object
            CacheUIObject(pooledScrollView.gameObject, () => {
                pooledScrollView.Dispose();
                UISpawner<GMPooledScrollView>.ReturnUI(pooledScrollView);
            });
            //Set constraints to columns
            pooledScrollView.ItemContainer.layoutConstraints = FlexibleGridLayout.LayoutConstraints.FixedColumns;
            //Set columns before initializing
            pooledScrollView.ItemContainer.columns = columns;
            //Set cell height before initializing
            pooledScrollView.ItemContainer.cellHeight = cellHeight;
            //Initialize scrollView
            pooledScrollView.Initialize<T>(scrollViewItemPrefab, initialCount, onUpdateItem, onReturnItem);

            return pooledScrollView;
        }

        protected GMPooledScrollView AddHorizontalPooledScrollView<T>(
            MonoBehaviour scrollViewItemPrefab,
            int initialCount,
            Action<UIBehaviour, int> onUpdateItem,
            int rows = 1,
            int cellWidth = 50,
            Action<UIBehaviour> onReturnItem = null
        ) where T : UIBehaviour {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, UIManager.UISettings.DefaultHorizontalPooledScrollView);

            GMPooledScrollView pooledScrollView = UISpawner<GMPooledScrollView>.SpawnUI(path, currentContainer);
            //Cache object
            CacheUIObject(pooledScrollView.gameObject, () => {
                pooledScrollView.Dispose();
                UISpawner<GMPooledScrollView>.ReturnUI(pooledScrollView);
            });
            //Set constraints to rows
            pooledScrollView.ItemContainer.layoutConstraints = FlexibleGridLayout.LayoutConstraints.FixedRows;
            //Set columns before initializing
            pooledScrollView.ItemContainer.rows = rows;
            //Set cell height before initializing
            pooledScrollView.ItemContainer.cellWidth = cellWidth;
            //Initialize scrollView
            pooledScrollView.Initialize<T>(scrollViewItemPrefab, initialCount, onUpdateItem, onReturnItem);

            return pooledScrollView;
        }
    }
}
