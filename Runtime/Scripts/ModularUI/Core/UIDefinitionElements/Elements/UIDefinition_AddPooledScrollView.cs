using GentlyUI.UIElements;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMPooledScrollView AddPooledScrollView<T>(
            MonoBehaviour scrollViewItemPrefab,
            IList dataList,
            Action<Behaviour, int> onUpdateItem,
            int columns = 1,
            Action<Behaviour> onReturnItem = null
        ) where T : Behaviour {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, UIManager.UISettings.DefaultPooledScrollView);

            GMPooledScrollView pooledScrollView = UISpawner<GMPooledScrollView>.SpawnUI(path, currentContainer);
            //Cache object
            CacheUIObject(pooledScrollView.gameObject, () => {
                pooledScrollView.Dispose();
                UISpawner<GMPooledScrollView>.ReturnUI(pooledScrollView);
            });
            //Set columns before initializing
            pooledScrollView.ItemContainer.columns = columns;
            //Initialize scrollView
            pooledScrollView.Initialize<T>(scrollViewItemPrefab, dataList.Count, onUpdateItem, onReturnItem);

            return pooledScrollView;
        }
    }
}
