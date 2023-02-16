using GentlyUI.UIElements;
using System.IO;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        private const string DefaultContent = "content";

        protected GMContent AddContent() {
            return AddContent(DefaultContent);
        }

        protected GMContent AddContent(string contentType) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.LayoutPath, contentType);

            //Spawn immediately as content is needed to nest other prefabs in it
            GMContent content = UISpawner<GMContent>.SpawnUI(path, currentContainer);
            CacheUIObject(content.gameObject, () => UISpawner<GMContent>.ReturnUI(content));

            SetCurrentContainer(content.container);

            return content;
        }

        protected void EndContent() {
            LeaveCurrentContainer();
        }
    }
}
