using GentlyUI.UIElements;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIScrollViewSettings", menuName = "GentlyUI/UIScrollViewSettings", order = 1)]
public class UIScrollViewSettings : ScriptableObject {

    /// Defines whether scrolling should stop immediately (direct) or smoothly (eased) when no input is dected.
    /// </summary>
    [Tooltip("")]
    [SerializeField] private MovementType movementType = MovementType.Direct;
    public MovementType MovementType => movementType;

    /// <summary>
    /// Set to true if the scroll view should scroll row by row instead of fluent movements.
    /// </summary>
    [UnityEngine.Tooltip("Set to true if the scroll view should scroll row by row instead of fluent movements.")]
    [SerializeField] private bool scrollInSteps = false;
    public bool ScrollInSteps => scrollInSteps;

    /// <summary>
    /// How fast should the scroll view scroll?
    /// </summary>
    [UnityEngine.Tooltip("How fast should the scroll view scroll?")]
    [SerializeField] private float scrollSensitivity = 50;
    public float ScrollSensitivity => scrollSensitivity;

    /// <summary>
    /// The ease duration in seconds. This defines how long it takes for the scroll movement to catch up to the target position.
    /// </summary>
    [UnityEngine.Tooltip("The ease duration in seconds. This defines how long it takes for the scroll movement to catch up to the target position.")]
    [SerializeField] private float easeDuration = 0.15f;
    public float EaseDuration => easeDuration;
}
