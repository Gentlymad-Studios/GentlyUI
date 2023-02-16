using GentlyUI.UIElements;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMDropdown AddDropdown(
            int defaultValue, 
            UnityAction<int> onValueChanged, 
            List<GMDropdown.DropdownOptionData> optionData
        ) {
            return AddDropdown(UIManager.UISettings.DefaultDropdown, defaultValue, onValueChanged, optionData);
        }

        protected GMDropdown AddDropdown(
            string dropdownType,
            int defaultValue,
            UnityAction<int> onValueChanged,
            List<GMDropdown.DropdownOptionData> optionData
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, dropdownType);
            GMDropdown dropdown = UISpawner<GMDropdown>.SpawnUI(path, currentContainer);
            //Set data
            dropdown.SetOptions(optionData);
            //Set default value
            dropdown.SetDefaultValue(defaultValue);
            //Add new listener
            dropdown.OnValueChanged.AddListener(onValueChanged);
            //Cache object
            CacheUIObject(dropdown.gameObject, () => {
                UISpawner<GMDropdown>.ReturnUI(dropdown);
                dropdown.OnValueChanged.RemoveListener(onValueChanged);
            });

            return dropdown;
        }
    }
}
