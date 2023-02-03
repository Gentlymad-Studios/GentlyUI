using GentlyUI.UIElements;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected void AddDropdown(
            int defaultValue, 
            UnityAction<int> onValueChanged, 
            List<GMDropdown.DropdownOptionData> optionData,
            System.Action<GMDropdown> onSpawn = null
        ) {
            AddDropdown(UIManager.UISettings.DefaultDropdown, defaultValue, onValueChanged, optionData, onSpawn);
        }

        protected void AddDropdown(
            string dropdownType,
            int defaultValue,
            UnityAction<int> onValueChanged,
            List<GMDropdown.DropdownOptionData> optionData,
            System.Action<GMDropdown> onSpawn = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, dropdownType);
            UISpawner<GMDropdown>.RegisterUIForSpawn(path, currentContainer, (GMDropdown d) => {
                //Set data
                d.SetOptions(optionData);
                //Set default value
                d.SetDefaultValue(defaultValue);
                //Add new listener
                d.OnValueChanged.AddListener(onValueChanged);
                //Cache object
                CacheUIObject(d.gameObject, () => {
                    UISpawner<GMDropdown>.RegisterUIForReturn(d);
                    d.OnValueChanged.RemoveListener(onValueChanged);
                });
                //Callback
                if (onSpawn != null) onSpawn(d);
            }, currentHierarchyOrder);

            IncrementCurrentHierarchyOrder();
        }
    }
}
