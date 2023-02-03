using GentlyUI.UIElements;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected void AddInputField(
            string defaultValue,
            UnityAction<string> onSubmit,
            GMInputField.InputType inputType = GMInputField.InputType.Standard,
            TMP_InputField.CharacterValidation validation = TMP_InputField.CharacterValidation.None,
            string placeholderText = "",
            System.Action<GMInputField> onSpawn = null
        ) {
            AddInputField(UIManager.UISettings.DefaultInputField, defaultValue, onSubmit, inputType, validation, placeholderText, onSpawn);
        }

        protected void AddInputField(
            string inputFieldType, 
            string defaultValue,
            UnityAction<string> onSubmit,
            GMInputField.InputType inputType = GMInputField.InputType.Standard,
            TMP_InputField.CharacterValidation validation = TMP_InputField.CharacterValidation.None,
            string placeholderText = "",
            System.Action<GMInputField> onSpawn = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, inputFieldType);

            UISpawner<GMInputField>.RegisterUIForSpawn(path, currentContainer, (GMInputField i) => {
                //Set properties
                i.SetCharacterValidation(validation);
                i.SetInputType(inputType);
                //Set placeholder text
                if (!string.IsNullOrWhiteSpace(placeholderText)) i.SetPlaceholderText(placeholderText);
                //Set default text
                i.Text = defaultValue;
                //Add listener
                i.onSubmit.AddListener(onSubmit);
                //Cache object
                CacheUIObject(i.gameObject, () => {
                    UISpawner<GMInputField>.RegisterUIForReturn(i);
                    i.onSubmit.RemoveListener(onSubmit);
                });
                //Callback
                if (onSpawn != null) onSpawn(i);
            }, currentHierarchyOrder);

            IncrementCurrentHierarchyOrder();
        }
    }
}
