using GentlyUI.UIElements;
using System.IO;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        private const string DefaultToggleGroup = "toggleGroup";

        protected GMToggleGroup AddToggleGroup(bool allowSwitchOff = false) {
            return AddToggleGroup(DefaultToggleGroup, allowSwitchOff);
        }

        protected GMToggleGroup AddToggleGroup(string contentType, bool allowSwitchOff = false) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.LayoutPath, contentType);

            //Spawn immediately as content is needed to nest other prefabs in it
            GMToggleGroup toggleGroup = UISpawner<GMToggleGroup>.SpawnUI(path, currentContainer);
            toggleGroup.allowSwitchOff = allowSwitchOff;
            CacheUIObject(toggleGroup.gameObject, () => UISpawner<GMToggleGroup>.ReturnUI(toggleGroup));

            SetCurrentContainer(toggleGroup.container);

            return toggleGroup;
        }

        protected void EndToggleGroup() {
            LeaveCurrentContainer();
        }
    }
}
