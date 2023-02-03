using GentlyUI.UIElements;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected void AddPooledScrollView<T> (
            MonoBehaviour scrollViewItemPrefab,
            IList dataList,
            Action<Behaviour, int> onUpdateItem,
            Action<Behaviour> onReturnItem = null,
            Action<GMPooledScrollView> onSpawn = null
        ) where T : Behaviour {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, UIManager.UISettings.DefaultPooledScrollView);

            UISpawner<GMPooledScrollView>.RegisterUIForSpawn(path, currentContainer, (GMPooledScrollView scrollView) => {
                //Set prefab
                scrollView.ItemPrefab = scrollViewItemPrefab;
                //Cache object
                CacheUIObject(scrollView.gameObject, () => UISpawner<GMPooledScrollView>.RegisterUIForReturn(scrollView));
                //Callback
                if (onSpawn != null) onSpawn(scrollView);
                //Initialize scrollView
                scrollView.Initialize<T>(dataList.Count, onUpdateItem, onReturnItem);
            }, currentHierarchyOrder);

            IncrementCurrentHierarchyOrder();
        }
    }
}
