using GentlyUI;
using GentlyUI.UIElements;
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(GMImageComponent))]
public class GMImageComponentEditor : ImageEditor {
    SerializedProperty globalUIColor;
    SerializedProperty useGlobalUIColor;
    VisualElement root;

    protected override void OnEnable() {
        base.OnEnable();

        globalUIColor = serializedObject.FindProperty(nameof(globalUIColor));
        useGlobalUIColor = serializedObject.FindProperty(nameof(useGlobalUIColor));
    }

    private void SetupUI(GeometryChangedEvent e) {
        Toggle toggle = root.Q<Toggle>($"unity-input-{nameof(useGlobalUIColor)}");
        PropertyField defaultColorContainer = root.Q<PropertyField>($"{nameof(PropertyField)}:m_Color");
        ColorField defaultColorField = defaultColorContainer.Q<ColorField>("unity-input-m_Color");
        PropertyField uiColorField = root.Q<PropertyField>($"{nameof(PropertyField)}:{nameof(globalUIColor)}");

        uiColorField.PlaceBehind(defaultColorContainer);
        toggle.parent.PlaceBehind(uiColorField);

        //Add callbacks for specific elements
        toggle.RegisterValueChangedCallback(_ => { ToggleElements(_.newValue); });
        uiColorField.RegisterValueChangeCallback(_ => { 
            if (toggle.value) {
                UpdateColor();
            }
        });

        //Initialize UI
        ToggleElements(toggle.value);
        //Remove callback
        root.UnregisterCallback<GeometryChangedEvent>(SetupUI);

        void ToggleElements(bool useGlobalUIColor) {
            defaultColorContainer.style.display = useGlobalUIColor ? DisplayStyle.None : DisplayStyle.Flex;
            uiColorField.style.display = useGlobalUIColor ? DisplayStyle.Flex : DisplayStyle.None;
            
            if (useGlobalUIColor) {
                defaultColorField.value = UIProjectSettings.instance.uiSettings.GetColor(globalUIColor.stringValue);
            }
        }

        void UpdateColor() {
            defaultColorField.value = UIProjectSettings.instance.uiSettings.GetColor(globalUIColor.stringValue);
        }
    }
    

    public override VisualElement CreateInspectorGUI() {
        root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        root.RegisterCallback<GeometryChangedEvent>(SetupUI);
        return root;
    }
}