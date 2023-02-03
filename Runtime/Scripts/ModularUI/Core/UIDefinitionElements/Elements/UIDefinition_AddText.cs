using GentlyUI.UIElements;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected void AddText(
            string text,
            TextAlignmentOptions alignment,
            System.Action<GMTextComponent> onSpawn = null
        ) {
            AddText(UIManager.UISettings.DefaultText, text, alignment, onSpawn);
        }

        protected void AddText(
            string textType, 
            string text,
            TextAlignmentOptions alignment,
            System.Action<GMTextComponent> onSpawn = null
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, textType);

            UISpawner<GMTextComponent>.RegisterUIForSpawn(path, currentContainer, (GMTextComponent t) => {
                t.SetText(text);
                t.alignment = alignment;

                //Cache object
                CacheUIObject(t.gameObject, () => {
                    UISpawner<GMTextComponent>.RegisterUIForReturn(t);
                });

                //Callback
                if (onSpawn != null) onSpawn(t);
            }, currentHierarchyOrder);

            IncrementCurrentHierarchyOrder();
        }
    }
}
