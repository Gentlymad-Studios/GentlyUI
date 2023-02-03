using GentlyUI.UIElements;
using System.IO;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        private const string DefaultFooter = "footer";

        protected GMFooter AddFooter() {
            return AddFooter(DefaultFooter);
        }

        protected GMFooter AddFooter(string footerType) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.LayoutPath, footerType);

            //Spawn immediately as layout elements are important for visual consistency.
            GMFooter footer = UISpawner<GMFooter>.SpawnImmediately(path, currentContainer, currentHierarchyOrder);
            CacheUIObject(footer.gameObject, () => UISpawner<GMFooter>.ReturnImmediately(footer));

            IncrementCurrentHierarchyOrder();

            return footer;
        }
    }
}
