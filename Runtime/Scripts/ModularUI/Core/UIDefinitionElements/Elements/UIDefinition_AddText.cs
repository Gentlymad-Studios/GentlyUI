using GentlyUI.UIElements;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GentlyUI.ModularUI {
    public abstract partial class UIDefinition {
        protected GMTextComponent AddText(
            string text,
            TextAlignmentOptions alignment
        ) {
            return AddText(UIManager.UISettings.DefaultText, text, alignment);
        }

        protected GMTextComponent AddText(
            string textType, 
            string text,
            TextAlignmentOptions alignment
        ) {
            string path = Path.Join(UIPaths.BasePath, UIPaths.ElementPath, textType);

            GMTextComponent textComponent = UISpawner<GMTextComponent>.SpawnUI(path, currentContainer);
            textComponent.SetText(text);
            textComponent.alignment = alignment;

            //Cache object
            CacheUIObject(textComponent.gameObject, () => {
                UISpawner<GMTextComponent>.ReturnUI(textComponent);
            });

            return textComponent;
        }
    }
}
