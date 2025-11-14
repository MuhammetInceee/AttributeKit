using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// Read-only mode for inspector fields.
    /// </summary>
    public enum ReadOnlyMode
    {
        /// <summary>Always read-only in inspector (default)</summary>
        Always,
        /// <summary>Read-only only during play mode</summary>
        OnlyInPlayMode,
        /// <summary>Read-only only during edit mode</summary>
        OnlyInEditMode
    }

    /// <summary>
    /// Attribute to make fields read-only in the inspector.
    /// Fields can still be modified through code at runtime.
    /// </summary>
    /// <example>
    /// // Always read-only
    /// [ReadOnly]
    /// public int currentScore;
    ///
    /// // Read-only only in play mode
    /// [ReadOnly(ReadOnlyMode.OnlyInPlayMode)]
    /// public float currentHealth;
    ///
    /// // Read-only only in edit mode
    /// [ReadOnly(ReadOnlyMode.OnlyInEditMode)]
    /// public string editorOnlyData;
    ///
    /// // Works with all field types
    /// [ReadOnly]
    /// public Vector3 position;
    ///
    /// [ReadOnly]
    /// public GameObject targetObject;
    ///
    /// // Works with arrays and lists
    /// [ReadOnly]
    /// public int[] scores;
    ///
    /// [ReadOnly]
    /// public List<string> playerNames;
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        /// <summary>
        /// The read-only mode for this field.
        /// </summary>
        public ReadOnlyMode Mode { get; }

        /// <summary>
        /// Whether to show a visual indicator that the field is read-only.
        /// </summary>
        public bool ShowIndicator { get; }

        /// <summary>
        /// Custom tooltip text to show when hovering over the field.
        /// </summary>
        public string TooltipText { get; }

        /// <summary>
        /// Creates a ReadOnly attribute that is always read-only.
        /// </summary>
        public ReadOnlyAttribute()
        {
            Mode = ReadOnlyMode.Always;
            ShowIndicator = false;
            TooltipText = string.Empty;
        }

        /// <summary>
        /// Creates a ReadOnly attribute with specified mode.
        /// </summary>
        /// <param name="mode">The read-only mode.</param>
        public ReadOnlyAttribute(ReadOnlyMode mode = ReadOnlyMode.Always)
        {
            Mode = mode;
            ShowIndicator = false;
            TooltipText = string.Empty;
        }

        /// <summary>
        /// Creates a ReadOnly attribute with mode and visual indicator option.
        /// </summary>
        /// <param name="mode">The read-only mode.</param>
        /// <param name="showIndicator">Whether to show a visual indicator.</param>
        public ReadOnlyAttribute(ReadOnlyMode mode = ReadOnlyMode.Always, bool showIndicator = true)
        {
            Mode = mode;
            ShowIndicator = showIndicator;
            TooltipText = string.Empty;
        }

        /// <summary>
        /// Creates a ReadOnly attribute with mode, indicator, and custom tooltip.
        /// </summary>
        /// <param name="mode">The read-only mode.</param>
        /// <param name="showIndicator">Whether to show a visual indicator.</param>
        /// <param name="tooltipText">Custom tooltip text.</param>
        public ReadOnlyAttribute(ReadOnlyMode mode, bool showIndicator, string tooltipText)
        {
            Mode = mode;
            ShowIndicator = showIndicator;
            TooltipText = tooltipText ?? string.Empty;
        }

        /// <summary>
        /// Creates a ReadOnly attribute with custom tooltip (always read-only).
        /// </summary>
        /// <param name="tooltipText">Custom tooltip text.</param>
        public ReadOnlyAttribute(string tooltipText)
        {
            Mode = ReadOnlyMode.Always;
            ShowIndicator = false;
            TooltipText = tooltipText ?? string.Empty;
        }
    }
}
