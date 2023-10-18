using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GentlyUI {
    [FilePath("ProjectSettings/" + nameof(UIProjectSettings) + ".asset", FilePathAttribute.Location.ProjectFolder)]
    public class UIProjectSettings : ScriptableSingleton<UIProjectSettings> {
        public const string MENUITEMBASE = "Tools/";

        [SerializeField]
        public UISettings uiSettings;

        public void OnEnable() {
            hideFlags &= ~HideFlags.NotEditable;
        }

        public void Save() {
            Save(true);
        }
    }
}