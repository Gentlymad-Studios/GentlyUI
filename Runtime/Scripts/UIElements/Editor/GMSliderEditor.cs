#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GentlyUI.UIElements {
    [CustomEditor(typeof(GMSlider))]
    public class GMSliderEditor : GMSelectableStyledEditor {
        SerializedProperty directionProperty;
        SerializedProperty fillImageProperty;
        SerializedProperty handleProperty;
        SerializedProperty wholeNumbersProperty;
        SerializedProperty minValueProperty;
        SerializedProperty maxValueProperty;
        SerializedProperty valueProperty;
        SerializedProperty onValueChangedProperty;
        SerializedProperty valueOutputProperty;
        SerializedProperty valueOutputContainerProperty;
        SerializedProperty onlyShowValueOnHoverProperty;

        GMSlider slider;

        protected override void OnEnable() {
            base.OnEnable();

            slider = (GMSlider)target;

            directionProperty = serializedObject.FindProperty("direction");
            fillImageProperty = serializedObject.FindProperty("fillImage");
            handleProperty = serializedObject.FindProperty("handle");
            wholeNumbersProperty = serializedObject.FindProperty("wholeNumbers");
            minValueProperty = serializedObject.FindProperty("minValue");
            maxValueProperty = serializedObject.FindProperty("maxValue");
            valueProperty = serializedObject.FindProperty("value");
            onValueChangedProperty = serializedObject.FindProperty("onValueChanged");
            valueOutputProperty = serializedObject.FindProperty("valueOutput");
            valueOutputContainerProperty = serializedObject.FindProperty("valueOutputContainer");
            onlyShowValueOnHoverProperty = serializedObject.FindProperty("onlyShowValueOnHover");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            serializedObject.Update();

            EditorGUILayout.PropertyField(fillImageProperty);
            EditorGUILayout.PropertyField(handleProperty);           

            if (fillImageProperty.objectReferenceValue != null || handleProperty.objectReferenceValue != null) {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(directionProperty);
                if (EditorGUI.EndChangeCheck()) {
                    GMSlider.Direction direction = (GMSlider.Direction)directionProperty.enumValueIndex;
                    foreach (Object obj in serializedObject.targetObjects) {
                        GMSlider slider = obj as GMSlider;
                        slider.SetDirection(direction, true);
                    }
                }

                EditorGUILayout.PropertyField(minValueProperty);
                EditorGUILayout.PropertyField(maxValueProperty);
                EditorGUILayout.PropertyField(wholeNumbersProperty);
                EditorGUILayout.Slider(valueProperty, minValueProperty.floatValue, maxValueProperty.floatValue);
                // Draw the event notification options
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(onValueChangedProperty);
            } else {
                EditorGUILayout.HelpBox("Specify a RectTransform for the slider fill or the slider handle or both. Each must have a parent RectTransform that it can slide within.", MessageType.Info);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(valueOutputProperty);
            EditorGUILayout.PropertyField(valueOutputContainerProperty);
            EditorGUILayout.PropertyField(onlyShowValueOnHoverProperty);

            slider.EDITOR_UpdateSlider();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif