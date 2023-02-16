using GentlyUI.UIElements;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMButton AddButton(
            UnityAction onClick,
            string label = null,
            Sprite icon = null
        ) {
            return AddButton(UIManager.UISettings.DefaultButton, onClick, label, icon);
        }

        protected GMButton AddButton(
            string buttonType, 
            UnityAction onClick,
            string label = null,
            Sprite icon = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, buttonType);

            GMButton button = UISpawner<GMButton>.SpawnUI(path, currentContainer);

            //Add new listener
            if (onClick != null) {
                button.OnClick.AddListener(onClick);
            }

            //Set styles
            if (!string.IsNullOrWhiteSpace(label)) button.SetLabel(label);
            if (icon != null) button.SetIcon(icon);

            //Cache object
            CacheUIObject(button.gameObject, () => {
                UISpawner<GMButton>.ReturnUI(button);
                if (onClick != null) {
                    button.OnClick.RemoveListener(onClick);
                }
            });

            return button;
        }
    }
}
