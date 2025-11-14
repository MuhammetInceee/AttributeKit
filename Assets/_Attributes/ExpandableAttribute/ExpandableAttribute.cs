using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// Header style options for expandable ScriptableObject display.
    /// </summary>
    public enum ExpandableHeaderStyle
    {
        /// <summary>Normal Unity header style</summary>
        Normal,
        /// <summary>Box style with background</summary>
        Box,
        /// <summary>Foldout style</summary>
        Foldout
    }

    /// <summary>
    /// Attribute to display ScriptableObject references inline in the inspector.
    /// Shows the ScriptableObject's properties directly without needing to open the asset.
    /// </summary>
    /// <example>
    /// // Basic usage
    /// [Expandable]
    /// public MyScriptableObject data;
    ///
    /// // With custom header style
    /// [Expandable(ExpandableHeaderStyle.Box)]
    /// public GameSettings settings;
    ///
    /// // With create button enabled
    /// [Expandable(showCreateButton: true)]
    /// public ItemData itemData;
    ///
    /// // With delete button and custom style
    /// [Expandable(ExpandableHeaderStyle.Foldout, showCreateButton: true, showDeleteButton: true)]
    /// public PlayerProfile profile;
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ExpandableAttribute : PropertyAttribute
    {
        /// <summary>
        /// Header style for the expandable section.
        /// </summary>
        public ExpandableHeaderStyle HeaderStyle { get; }

        /// <summary>
        /// Whether to show a create button when the reference is null.
        /// </summary>
        public bool ShowCreateButton { get; }

        /// <summary>
        /// Whether to show a delete button for the ScriptableObject asset.
        /// </summary>
        public bool ShowDeleteButton { get; }

        /// <summary>
        /// Whether the expandable section is initially expanded.
        /// </summary>
        public bool IsExpandedByDefault { get; }

        /// <summary>
        /// Custom header color (optional). Use Color.clear for default.
        /// </summary>
        public Color HeaderColor { get; }

        /// <summary>
        /// Whether to draw a divider line after the expandable content.
        /// </summary>
        public bool DrawDivider { get; }

        /// <summary>
        /// Creates an Expandable attribute with default settings.
        /// </summary>
        public ExpandableAttribute()
        {
            HeaderStyle = ExpandableHeaderStyle.Normal;
            ShowCreateButton = false;
            ShowDeleteButton = false;
            IsExpandedByDefault = true;
            HeaderColor = Color.clear;
            DrawDivider = true;
        }

        /// <summary>
        /// Creates an Expandable attribute with specified header style.
        /// </summary>
        /// <param name="headerStyle">Style of the header.</param>
        /// <param name="showCreateButton">Whether to show create button when null.</param>
        /// <param name="showDeleteButton">Whether to show delete button.</param>
        public ExpandableAttribute(
            ExpandableHeaderStyle headerStyle,
            bool showCreateButton = false,
            bool showDeleteButton = false)
        {
            HeaderStyle = headerStyle;
            ShowCreateButton = showCreateButton;
            ShowDeleteButton = showDeleteButton;
            IsExpandedByDefault = true;
            HeaderColor = Color.clear;
            DrawDivider = true;
        }

        /// <summary>
        /// Creates an Expandable attribute with create button option.
        /// </summary>
        /// <param name="showCreateButton">Whether to show create button when null.</param>
        public ExpandableAttribute(bool showCreateButton)
        {
            HeaderStyle = ExpandableHeaderStyle.Normal;
            ShowCreateButton = showCreateButton;
            ShowDeleteButton = false;
            IsExpandedByDefault = true;
            HeaderColor = Color.clear;
            DrawDivider = true;
        }

        /// <summary>
        /// Creates an Expandable attribute with full customization.
        /// </summary>
        /// <param name="headerStyle">Style of the header.</param>
        /// <param name="showCreateButton">Whether to show create button when null.</param>
        /// <param name="showDeleteButton">Whether to show delete button.</param>
        /// <param name="isExpandedByDefault">Whether initially expanded.</param>
        /// <param name="drawDivider">Whether to draw divider line.</param>
        public ExpandableAttribute(
            ExpandableHeaderStyle headerStyle,
            bool showCreateButton,
            bool showDeleteButton,
            bool isExpandedByDefault,
            bool drawDivider = true)
        {
            HeaderStyle = headerStyle;
            ShowCreateButton = showCreateButton;
            ShowDeleteButton = showDeleteButton;
            IsExpandedByDefault = isExpandedByDefault;
            HeaderColor = Color.clear;
            DrawDivider = drawDivider;
        }
    }
}
