using GentlyUI.UIElements;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected void AddSlider(
            float initialValue,
            float minValue,
            float maxValue,
            bool wholeNumbers,
            UnityAction<float> onValueChanged,
            string label = null,
            Sprite icon = null,
            System.Action<GMSlider> onSpawn = null
        ) {
            AddSlider(UIManager.UISettings.DefaultSlider, initialValue, minValue, maxValue, wholeNumbers, onValueChanged, label, icon, onSpawn);
        }

        protected void AddSlider(
            string sliderType,
            float initialValue,
            float minValue,
            float maxValue,
            bool wholeNumbers,
            UnityAction<float> onValueChanged,
            string label = null,
            Sprite icon = null,
            System.Action<GMSlider> onSpawn = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, sliderType);

            UISpawner<GMSlider>.RegisterUIForSpawn(path, currentContainer, (GMSlider s) => {
                //Set default value
                s.SetInitialValue(initialValue, minValue, maxValue, wholeNumbers);
                //Add new listener
                s.OnValueChanged.AddListener(onValueChanged);
                //Set styles
                if (!string.IsNullOrWhiteSpace(label)) s.SetLabel(label);
                if (icon != null) s.SetIcon(icon);
                //Cache object
                CacheUIObject(s.gameObject, () => { 
                    UISpawner<GMSlider>.RegisterUIForReturn(s); 
                    s.OnValueChanged.RemoveListener(onValueChanged); 
                });
                //Callback
                if (onSpawn != null) onSpawn(s);
            }, currentHierarchyOrder);

            IncrementCurrentHierarchyOrder();
        }
    }
}
