#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GentlyUI.UIElements {
    [CustomEditor(typeof(GMSelectable))]
    public class GMSelectableEditor : Editor {
        SerializedProperty navigationProperty;
        SerializedProperty interactableProperty;
        SerializedProperty visualElementsProperty;

        protected virtual void OnEnable() {
            navigationProperty = serializedObject.FindProperty("navigation");
            interactableProperty = serializedObject.FindProperty("interactable");
            visualElementsProperty = serializedObject.FindProperty("visualElements");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(navigationProperty);
            EditorGUILayout.PropertyField(interactableProperty);
            EditorGUILayout.PropertyField(visualElementsProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif