using EditorHelper;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GentlyUI {
    public class UIProjectSettingsProvider : ScriptableSingletonProviderBase {
        private static readonly string[] tags = new string[] { nameof(GentlyUI), "ui", "ui settings", "uisettings", "gentlyUI", "Gently", "UI" };

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {
            return UIProjectSettings.instance ? new UIProjectSettingsProvider() : null;
        }

        public UIProjectSettingsProvider(SettingsScope scope = SettingsScope.Project) : base(UIProjectSettings.MENUITEMBASE + nameof(GentlyUI), scope) {
            keywords = tags;
        }

        protected override EventCallback<SerializedPropertyChangeEvent> GetValueChangedCallback() {
            return ValueChanged;
        }

        /// <summary>
        /// Called when any value changed.
        /// </summary>
        /// <param name="evt"></param>
        private void ValueChanged(SerializedPropertyChangeEvent evt) {
            // notify all listeneres (ReactiveSettings)
            serializedObject.ApplyModifiedProperties();
            // call save on our singleton as it is a strange hybrid and not a full ScriptableObject
            UIProjectSettings.instance.Save();
        }

        protected override string GetHeader() {
            return nameof(GentlyUI);
        }

        public override Type GetDataType() {
            return typeof(UIProjectSettings);
        }

        public override dynamic GetInstance() {
            //Force HideFlags
            UIProjectSettings.instance.OnEnable();
            return UIProjectSettings.instance;
        }
    }
}