using GentlyUI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(GlobalUIColorPropertyAttribute))]
public class GlobalUIColorPropertyDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        List<string> identifiers = UIProjectSettings.instance.uiSettings.ColorIdentifiers;

        if (string.IsNullOrEmpty(property.stringValue)) {
            property.stringValue = identifiers[0];
            property.serializedObject.ApplyModifiedProperties();
        }

        string currentValue = property.stringValue;
        int defaultIndex = identifiers.IndexOf(currentValue);

        PopupField<string> field = new PopupField<string>(property.displayName, identifiers, defaultIndex);

        field.RegisterCallback <ChangeEvent<string>>((evt) => {
            property.stringValue = evt.newValue;
            property.serializedObject.ApplyModifiedProperties();
        });

        field.AddToClassList("unity-base-field__aligned");

        return field;
    }
}
