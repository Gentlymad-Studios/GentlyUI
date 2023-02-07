using UnityEditor;

namespace GentlyUI.UIElements {
    [CustomEditor(typeof(GMSelectableStyled))]
    public class GMSelectableStyledEditor : GMSelectableEditor {
        SerializedProperty labelOutputProperty;
        SerializedProperty iconOutputProperty;

        protected override void OnEnable() {
            base.OnEnable();

            labelOutputProperty = serializedObject.FindProperty("labelOutput");
            iconOutputProperty = serializedObject.FindProperty("iconOutput");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(labelOutputProperty);
            EditorGUILayout.PropertyField(iconOutputProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}