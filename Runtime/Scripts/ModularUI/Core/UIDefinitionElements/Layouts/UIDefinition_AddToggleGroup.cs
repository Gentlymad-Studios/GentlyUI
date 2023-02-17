using GentlyUI.UIElements;
using System.IO;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        private const string DefaultToggleGroup = "toggleGroup";

        protected GMToggleGroup AddToggleGroup(bool allowSwitchOff = false) {
            return AddToggleGroup(DefaultToggleGroup, allowSwitchOff);
        }

        /// <summary>
        /// Cache of allowSwitchOff for current toggle group that is applied after the toggle group was ended using EndToggleGroup();
        /// This is because we want to add toggles to the toggle group from definitions without having the toggle group to activate them by its default behaviour.
        /// </summary>
        private bool _allowSwitchOff;

        protected GMToggleGroup AddToggleGroup(string contentType, bool allowSwitchOff = false) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.LayoutPath, contentType);

            _allowSwitchOff = allowSwitchOff;

            //Spawn immediately as content is needed to nest other prefabs in it
            GMToggleGroup toggleGroup = UISpawner<GMToggleGroup>.SpawnUI(path, currentContainer);
            toggleGroup.allowSwitchOff = true;
            CacheUIObject(toggleGroup.gameObject, () => UISpawner<GMToggleGroup>.ReturnUI(toggleGroup));

            SetCurrentContainer(toggleGroup.container);

            return toggleGroup;
        }

        protected void EndToggleGroup() {
            currentToggleGroup.allowSwitchOff = _allowSwitchOff;

            LeaveCurrentContainer();
        }
    }
}
