using EditorHelper;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace GentlyUI {
    public class UIProjectSettingsProvider : SettingsProviderBase {
        private const string path = basePath + nameof(UIProjectSettings);
        private static readonly string[] tags = new string[] { "ui", "ui settings", "uisettings", "gentlyUI", "Gently", "UI" };

		public UIProjectSettingsProvider(SettingsScope scope = SettingsScope.Project) : base(path, scope) {
            keywords = tags;
        }

		// Register the SettingsProvider
		[SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {
            return UIProjectSettings.Instance ? new UIProjectSettingsProvider() : null;
        }

        public override Type GetDataType() {
            return typeof(UIProjectSettings);
        }

        public override dynamic GetInstance() {
            return UIProjectSettings.Instance;
        }

        protected override void OnChange() {
        }
    }
}