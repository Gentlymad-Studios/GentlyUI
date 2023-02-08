using GentlyUI.UIElements;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected void AddButton(
            UnityAction onClick,
            string label = null,
            Sprite icon = null,
            System.Action<GMButton> onSpawn = null
        ) {
            AddButton(UIManager.UISettings.DefaultButton, onClick, label, icon, onSpawn);
        }

        protected void AddButton(
            string buttonType, 
            UnityAction onClick,
            string label = null,
            Sprite icon = null,
            System.Action<GMButton> onSpawn = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, buttonType);

            UISpawner<GMButton>.RegisterUIForSpawn(path, currentContainer, (GMButton b) => {
                //Add new listener
                if (onClick != null) {
                    b.OnClick.AddListener(onClick);
                }
                //Set styles
                if (!string.IsNullOrWhiteSpace(label)) b.SetLabel(label);
                if (icon != null) b.SetIcon(icon);
                //Cache object
                CacheUIObject(b.gameObject, () => {
                    UISpawner<GMButton>.RegisterUIForReturn(b);
                    if (onClick != null) {
                        b.OnClick.RemoveListener(onClick);
                    }
                });
                //Callback
                if (onSpawn != null) onSpawn(b);
            }, currentHierarchyOrder);

            IncrementCurrentHierarchyOrder();
        }
    }
}
