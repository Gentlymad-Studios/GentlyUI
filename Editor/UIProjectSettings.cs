using System.Collections.Generic;
using UnityEngine;

namespace GentlyUI {
    public class UIProjectSettings : ScriptableObject {
		public UISettings uiSettings;

		/// <summary>
		/// We only want to have one global settings file so we make use of the singleton pattern here
		/// </summary>
		private static UIProjectSettings _instance = null;
		public static UIProjectSettings Instance {
			get {
				if (_instance == null) {
					_instance = EditorHelper.Utility.CreateSettingsFile<UIProjectSettings>();
				}
				return _instance;
			}
		}
	}
}