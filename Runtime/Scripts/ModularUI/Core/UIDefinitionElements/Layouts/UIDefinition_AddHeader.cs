using GentlyUI.UIElements;
using System.IO;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        private const string DefaultHeader = "header";

        protected GMHeader AddHeader(string title) {
            return AddHeader(DefaultHeader, title);
        }

        protected GMHeader AddHeader(string headerType, string title) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.LayoutPath, headerType);

            //Spawn immediately as layout elements are important for visual consistency.
            GMHeader header = UISpawner<GMHeader>.SpawnImmediately(path, currentContainer, currentHierarchyOrder);
            header.SetTitle(title);
            CacheUIObject(header.gameObject, () => UISpawner<GMHeader>.ReturnImmediately(header));

            IncrementCurrentHierarchyOrder();

            return header;
        }
    }
}
