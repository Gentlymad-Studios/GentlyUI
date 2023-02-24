using GentlyUI.UIElements;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMPooledScrollView AddPooledScrollView<T>(
            MonoBehaviour scrollViewItemPrefab,
            IList dataList,
            Action<Behaviour, int> onUpdateItem,
            int columns = 1,
            int cellHeight = 50,
            Action<Behaviour> onReturnItem = null
        ) where T : Behaviour {
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
            pooledScrollView.Initialize<T>(scrollViewItemPrefab, dataList.Count, onUpdateItem, onReturnItem);

            return pooledScrollView;
        }
    }
}
