using GentlyUI.UIElements;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected void AddToggle(
            bool defaultValue, 
            UnityAction<bool> onValueChanged, 
            string label = null, 
            Sprite icon = null,
            System.Action<GMToggle> onSpawn = null
        ) {
            AddToggle(UIManager.UISettings.DefaultToggle, defaultValue, onValueChanged, label, icon, onSpawn);
        }

        protected void AddToggle(
            string toggleType, 
            bool defaultValue, 
            UnityAction<bool> onValueChanged, 
            string label = null, 
            Sprite icon = null,
            System.Action<GMToggle> onSpawn = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, toggleType);
            GMToggleGroup group = currentToggleGroup;

            UISpawner<GMToggle>.RegisterUIForSpawn(path, currentContainer, (GMToggle t) => {
                SetupToggle(t, defaultValue, onValueChanged, group, label, icon);
                //Cache object
                CacheUIObject(t.gameObject, () => {
                    if (onValueChanged != null) t.OnValueChanged.RemoveListener(onValueChanged);
                    t.Group = null;
                    UISpawner<GMToggle>.RegisterUIForReturn(t);
                });
                //Callback
                if (onSpawn != null) onSpawn(t);
            }, currentHierarchyOrder);

            IncrementCurrentHierarchyOrder();
        }

        protected GMToggle AddToggleImmediately(
            bool defaultValue,
            UnityAction<bool> onValueChanged,
            string label = null,
            Sprite icon = null
        ) {
            return AddToggleImmediately(UIManager.UISettings.DefaultToggle, defaultValue, onValueChanged, label, icon);
        }

        protected GMToggle AddToggleImmediately(
            string toggleType,
            bool defaultValue, 
            UnityAction<bool> onValueChanged, 
            string label = null, 
            Sprite icon = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, toggleType);
            GMToggleGroup group = currentToggleGroup;

            //Spawn
            GMToggle toggle = UISpawner<GMToggle>.SpawnImmediately(path, currentContainer, currentHierarchyOrder);
            SetupToggle(toggle, defaultValue, onValueChanged, group, label, icon);

            CacheUIObject(toggle.gameObject, () => {
                if (onValueChanged != null) toggle.OnValueChanged.RemoveListener(onValueChanged);
                toggle.Group = null;
                UISpawner<GMToggle>.ReturnImmediately(toggle);
            });

            IncrementCurrentHierarchyOrder();

            return toggle;
        }

        void SetupToggle(GMToggle toggle, bool defaultValue, UnityAction<bool> onValueChanged, GMToggleGroup group = null, string label = null, Sprite icon = null) {
            //Set styles
            if (!string.IsNullOrWhiteSpace(label)) toggle.SetLabel(label);
            if (icon != null) toggle.SetIcon(icon);
            //Add new listener
            if (onValueChanged != null) toggle.OnValueChanged.AddListener(onValueChanged);
            //Set group
            toggle.Group = group;
            //Set default value (Has to happen after group was assigned!!!)
            toggle.SetInitialValue(defaultValue);
        }
    }
}
