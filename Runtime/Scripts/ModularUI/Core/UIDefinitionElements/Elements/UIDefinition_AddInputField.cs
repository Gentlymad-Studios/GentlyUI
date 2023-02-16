using GentlyUI.UIElements;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMInputField AddInputField(
            string defaultValue,
            UnityAction<string> onSubmit,
            GMInputField.InputType inputType = GMInputField.InputType.Standard,
            TMP_InputField.CharacterValidation validation = TMP_InputField.CharacterValidation.None,
            string placeholderText = ""
        ) {
            return AddInputField(UIManager.UISettings.DefaultInputField, defaultValue, onSubmit, inputType, validation, placeholderText);
        }

        protected GMInputField AddInputField(
            string inputFieldType, 
            string defaultValue,
            UnityAction<string> onSubmit,
            GMInputField.InputType inputType = GMInputField.InputType.Standard,
            TMP_InputField.CharacterValidation validation = TMP_InputField.CharacterValidation.None,
            string placeholderText = ""
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, inputFieldType);

            GMInputField inputField = UISpawner<GMInputField>.SpawnUI(path, currentContainer);

            //Set properties
            inputField.SetCharacterValidation(validation);
            inputField.SetInputType(inputType);
            //Set placeholder text
            if (!string.IsNullOrWhiteSpace(placeholderText)) inputField.SetPlaceholderText(placeholderText);
            //Set default text
            inputField.Text = defaultValue;
            //Add listener
            inputField.onSubmit.AddListener(onSubmit);
            //Cache object
            CacheUIObject(inputField.gameObject, () => {
                UISpawner<GMInputField>.ReturnUI(inputField);
                inputField.onSubmit.RemoveListener(onSubmit);
            });

            return inputField;
        }
    }
}
