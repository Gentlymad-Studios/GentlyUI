using GentlyUI.UIElements;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMToggle AddToggle(
            bool defaultValue, 
            UnityAction<bool> onValueChanged, 
            string label = null, 
            Sprite icon = null
        ) {
            return AddToggle(UIManager.UISettings.DefaultToggle, defaultValue, onValueChanged, label, icon);
        }

        protected GMToggle AddToggle(
            string toggleType, 
            bool defaultValue, 
            UnityAction<bool> onValueChanged, 
            string label = null, 
            Sprite icon = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, toggleType);
            GMToggleGroup group = currentToggleGroup;

            GMToggle toggle = UISpawner<GMToggle>.SpawnUI(path, currentContainer);
            SetupToggle(toggle, defaultValue, onValueChanged, group, label, icon);
            //Cache object
            CacheUIObject(toggle.gameObject, () => {
                if (onValueChanged != null) toggle.OnValueChanged.RemoveListener(onValueChanged);
                toggle.Group = null;
                UISpawner<GMToggle>.ReturnUI(toggle);
            });

            return toggle;
        }

        void SetupToggle(GMToggle toggle, bool defaultValue, UnityAction<bool> onValueChanged, GMToggleGroup group = null, string label = null, Sprite icon = null) {
            //Set styles
            if (!string.IsNullOrWhiteSpace(label)) toggle.SetLabel(label);
            if (icon != null) toggle.SetIcon(icon);
            //Set default value (Has to happen before group was assigned!)
            toggle.SetInitialValue(defaultValue);
            //Set group
            toggle.Group = group;
            //Add new listener
            if (onValueChanged != null) toggle.OnValueChanged.AddListener(onValueChanged);
        }
    }
}
