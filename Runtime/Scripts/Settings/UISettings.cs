using GentlyUI.Core;
using GentlyUI.UIElements;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static GentlyUI.ModularUI.UIContainerSpawner;

[CreateAssetMenu(fileName = "UISettings", menuName = "GentlyUI/UISettings", order = 1)]
public class UISettings : ScriptableObject, ISerializationCallbackReceiver {
    [Header("Main")]
    /// <summary>
    /// If set to true the Tick of the UIManager should be called manually from another script.
    /// If set to false the UIManager will update automatically in Unity's LateUpdate.
    /// </summary>
    [Tooltip("If set to true the Tick of the UIManager should be called manually from another script.\r\nIf set to false the UIManager will update automatically in Unity's LateUpdate.")]
    [SerializeField] private bool updateManagerManually;
    public bool UpdateManagerManually => updateManagerManually;

    /// <summary>
    /// If set to true the UIManager will update all tweens based on the ui update rate.
    /// If set to false the default unity update will manage tween updates.
    /// </summary>
    [Tooltip("If set to true the UIManager will update all tweens based on the ui update rate.\r\nIf set to false the default unity update will manage tween updates.")]
    [SerializeField] private bool updateTweensByManager = true;
    public bool UpdateTweensByManager => updateTweensByManager;

    /// <summary>
    /// The rate at which the UI is updated.
    /// </summary>
    [Tooltip("The rate at which the UI is updated.")]
    [SerializeField] private float uiUpdateRate = 1 / 60f;
    public float UIUpdateRate => uiUpdateRate;

    /// <summary>
    /// Used for dynamic UI creation. Setting this to a slightly higher value than the maximum expected size can reduce memory usage and improve performance of SetParent and Object.Destroy.
    /// </summary>
    [Tooltip("Used for dynamic UI creation. Setting this to a slightly higher value than the maximum expected size can reduce memory usage and improve performance of SetParent and Object.Destroy.")]
    [SerializeField] private int maxHierarchyCapacity = 20;
    public int MaxHierarchyCapacity => maxHierarchyCapacity;

    [Header("Drag & Drop")]
    /// <summary>
    /// How fast the dragged object should snap to the origin (either new dropzone or old dropzone).
    /// </summary>
    [Tooltip("How fast the dragged object should snap to the origin (either new dropzone or old dropzone).")]
    [SerializeField] private float dragReturnSpeed = 500;
    public float DragReturnSpeed => dragReturnSpeed;
    /// <summary>
    /// The scale of the element while being dragged.
    /// </summary>
    [Tooltip("The scale of the element while being dragged.")]
    [SerializeField] private float dragObjectScale = 1.1f;
    public float DragObjectScale => dragObjectScale;

    [Header("Data")]
    /// <summary>
    /// A list of canvases to spawn from the UI on initialization.
    /// </summary>
    [Tooltip("A list of canvases to spawn from the UI on initialization.")]
    [SerializeField] private List<CanvasData> canvasData;
    public Dictionary<string, CanvasData> canvasDataLUT;

    public CanvasData GetCanvasData(string identifier) {
        return canvasDataLUT[identifier];
    }

    /// <summary>
    /// Presets for dynamically adding containerAnimations to containers that where created from script.
    /// </summary>
    [Tooltip("Presets for dynamically adding containerAnimations to containers that where created from script.")]
    [SerializeField] private List<UIContainerAnimationPreset> containerAnimationPresets;
    private Dictionary<Anchor, UIContainerAnimationPreset> containerAnimationPresetsLUT;

    public UIContainerAnimationPreset GetContainerAnimationPresetForAnchor(Anchor anchor) {
        if (containerAnimationPresetsLUT.ContainsKey(anchor)) {
            return containerAnimationPresetsLUT[anchor];
        }

        return null;
    }

    public void Initialize() {
        if (colorLUT == null) {
            colorLUT = new Dictionary<string, Color>();

            for (int i = 0, count = colors.Count; i < count; ++i) {
                GlobalUIColor uiColor = colors[i];
                colorLUT.Add(uiColor.identifier, uiColor.color);
            }
        }

        if (paddingLUT == null) {
            paddingLUT = new Dictionary<string, int>();

            for (int i = 0, count = paddings.Count; i < count; ++i) {
                GlobalPadding padding = paddings[i];
                paddingLUT.Add(padding.identifier, padding.padding);
            }
        }

        if (gradientLUT == null) {
            gradientLUT = new Dictionary<string, Gradient>();

            for (int i = 0, count = gradients.Count; i < count; ++i) {
                GlobalUIGradient uiGradient = gradients[i];
                gradientLUT.Add(uiGradient.identifier, uiGradient.gradient);
            }
        }

        if (iconLUT == null) {
            iconLUT = new Dictionary<string, Sprite>();

            for (int i = 0, count = icons.Count; i < count; ++i) {
                UIIcon uiIcon = icons[i];
                iconLUT.Add(uiIcon.identifier, uiIcon.icon);
            }
        }

        if (canvasDataLUT == null) {
            canvasDataLUT = new Dictionary<string, CanvasData>();

            for (int i = 0, count = canvasData.Count; i < count; ++i) {
                CanvasData cd = canvasData[i];
                canvasDataLUT.Add(cd.identifier, cd);
            }
        }

        if (containerAnimationPresetsLUT == null) {
            containerAnimationPresetsLUT = new Dictionary<Anchor, UIContainerAnimationPreset>();

            for (int i = 0, count = containerAnimationPresets.Count; i < count; ++i) {
                UIContainerAnimationPreset preset = containerAnimationPresets[i];
                containerAnimationPresetsLUT.Add(preset.anchor, preset);
            }
        }
    }

    [SerializeField]
    private List<GlobalUIColor> colors;
    private List<string> colorIdentifiers = new List<string>();
    public List<string> ColorIdentifiers => colorIdentifiers;
    private Dictionary<string, Color> colorLUT = new Dictionary<string, Color>();

    public Color GetColor(string colorIdentifier) {
#if UNITY_EDITOR
        if (colorLUT.ContainsKey(colorIdentifier)) {
            return colorLUT[colorIdentifier];
        } else {
            Debug.LogWarning($"Color {colorIdentifier} from ui settings was not found. Returning magenta!");
            return Color.magenta;
        }
#else
        return colorLUT[colorIdentifier];
#endif
    }

    [SerializeField]
    private List<GlobalPadding> paddings;
    private Dictionary<string, int> paddingLUT;

    public int GetPadding(string paddingIdentifier) {
        return paddingLUT[paddingIdentifier];
    }

    [SerializeField]
    private List<GlobalUIGradient> gradients;
    private Dictionary<string, Gradient> gradientLUT;

    public Gradient GetGradient(string gradientIdentifier) {
        return gradientLUT[gradientIdentifier];
    }

    [SerializeField]
    private List<UIIcon> icons;
    private Dictionary<string, Sprite> iconLUT;

    public Sprite GetIcon(string iconIdentifier) {
        return iconLUT[iconIdentifier];
    }

    public void OnBeforeSerialize() {}

    public void OnAfterDeserialize() {
        colorIdentifiers.Clear();
        colorLUT.Clear();

        for (int i = 0, count = colors.Count; i < count; ++i) {
            GlobalUIColor current = colors[i];
            if (colorIdentifiers.Contains(current.identifier)) {
                continue;
            }

            colorIdentifiers.Add(current.identifier);
            colorLUT.Add(current.identifier, current.color);
        }
    }

    [Header("Default UI Elements")]
    [SerializeField] private string defaultButton = "button";
    public string DefaultButton => defaultButton;

    [SerializeField] private string defaultDropdown = "dropdown";
    public string DefaultDropdown => defaultDropdown;

    [SerializeField] private string defaultInputField = "inputField";
    public string DefaultInputField => defaultInputField;

    [SerializeField] private string defaultPooledScrollView = "pooledScrollView";
    public string DefaultPooledScrollView => defaultPooledScrollView;

    [SerializeField] private string defaultHorizontalPooledScrollView = "pooledScrollView";
    public string DefaultHorizontalPooledScrollView => defaultHorizontalPooledScrollView;

    [SerializeField] private string defaultSlider = "slider";
    public string DefaultSlider => defaultSlider;

    [SerializeField] private string defaultText = "text";
    public string DefaultText => defaultText;

    [SerializeField] private string defaultToggle = "toggle";
    public string DefaultToggle => defaultToggle;
}

[System.Serializable]
public class CanvasData {
    public string identifier;
    public string pathToCanvas;
    public bool spawnOnGameStart;
}

[System.Serializable]
public class GlobalUIColor {
    public string identifier;
    public Color color;
}

[System.Serializable]
public class GlobalUIGradient {
    public string identifier;
    public Gradient gradient;
}

[System.Serializable]
public class GlobalPadding : Namable {
    public string identifier;
    public int padding;

    public override void UpdateName() {
        SetName(identifier + ": " + padding);
    }
}

[System.Serializable]
public class UIIcon {
    public string identifier;
    public Sprite icon;
}

[System.Serializable]
public class UIContainerAnimationPreset : Namable {
    public Anchor anchor;
    public GMAnimatedContainerState showState;
    public GMAnimatedContainerState hideState;

    public override void UpdateName() {
        SetName(anchor.ToString());
    }
}