using GentlyUI.UIElements;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMSlider AddSlider(
            float initialValue,
            float minValue,
            float maxValue,
            bool wholeNumbers,
            UnityAction<float> onValueChanged,
            string label = null,
            Sprite icon = null
        ) {
            return AddSlider (UIManager.UISettings.DefaultSlider, initialValue, minValue, maxValue, wholeNumbers, onValueChanged, label, icon);
        }

        protected GMSlider AddSlider(
            string sliderType,
            float initialValue,
            float minValue,
            float maxValue,
            bool wholeNumbers,
            UnityAction<float> onValueChanged,
            string label = null,
            Sprite icon = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, sliderType);

            GMSlider slider = UISpawner<GMSlider>.SpawnUI(path, currentContainer);
            //Set default value
            slider.SetInitialValue(initialValue, minValue, maxValue, wholeNumbers);
            //Add new listener
            slider.OnValueChanged.AddListener(onValueChanged);
            //Set styles
            if (!string.IsNullOrWhiteSpace(label)) slider.SetLabel(label);
            if (icon != null) slider.SetIcon(icon);
            //Cache object
            CacheUIObject(slider.gameObject, () => { 
                UISpawner<GMSlider>.ReturnUI(slider); 
                slider.OnValueChanged.RemoveListener(onValueChanged); 
            });

            return slider;
        }
    }
}
