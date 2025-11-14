using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// Box style for grouped fields.
    /// </summary>
    public enum BoxStyle
    {
        /// <summary>Standard box with border</summary>
        Default,
        /// <summary>Rounded corners box</summary>
        Rounded,
        /// <summary>Flat box without border</summary>
        Flat,
        /// <summary>Help box style (like Unity's HelpBox)</summary>
        HelpBox
    }
    
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class BoxGroupAttribute : PropertyAttribute
    {
        /// <summary>
        /// Unique identifier for this group. Fields with the same ID will be grouped together.
        /// </summary>
        public string GroupId { get; }

        /// <summary>
        /// Display title for the group. If null or empty, the GroupId will be used.
        /// </summary>
        public string GroupTitle { get; }

        /// <summary>
        /// Visual style of the box.
        /// </summary>
        public BoxStyle Style { get; }

        /// <summary>
        /// Whether the group should have a foldout to collapse/expand.
        /// </summary>
        public bool Foldable { get; }

        /// <summary>
        /// Whether the group is expanded by default (only applies if Foldable is true).
        /// </summary>
        public bool ExpandedByDefault { get; }

        /// <summary>
        /// Whether to show the group title label.
        /// </summary>
        public bool ShowLabel { get; }

        /// <summary>
        /// Custom background color for the box. Use Color.clear for default.
        /// </summary>
        public Color BackgroundColor { get; }

        /// <summary>
        /// Custom header color. Use Color.clear for default.
        /// </summary>
        public Color HeaderColor { get; }

        /// <summary>
        /// Order of this group in the inspector (lower values appear first).
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Creates a BoxGroup attribute with minimal configuration.
        /// </summary>
        /// <param name="groupId">Unique identifier for this group.</param>
        public BoxGroupAttribute(string groupId)
        {
            GroupId = groupId;
            GroupTitle = groupId;
            Style = BoxStyle.Default;
            Foldable = false;
            ExpandedByDefault = true;
            ShowLabel = true;
            BackgroundColor = Color.clear;
            HeaderColor = Color.clear;
            Order = 0;
        }

        /// <summary>
        /// Creates a BoxGroup attribute with custom title.
        /// </summary>
        /// <param name="groupId">Unique identifier for this group.</param>
        /// <param name="groupTitle">Display title for the group.</param>
        public BoxGroupAttribute(string groupId, string groupTitle)
        {
            GroupId = groupId;
            GroupTitle = string.IsNullOrEmpty(groupTitle) ? groupId : groupTitle;
            Style = BoxStyle.Default;
            Foldable = false;
            ExpandedByDefault = true;
            ShowLabel = true;
            BackgroundColor = Color.clear;
            HeaderColor = Color.clear;
            Order = 0;
        }

        /// <summary>
        /// Creates a BoxGroup attribute with custom title and style.
        /// </summary>
        /// <param name="groupId">Unique identifier for this group.</param>
        /// <param name="groupTitle">Display title for the group.</param>
        /// <param name="style">Visual style of the box.</param>
        public BoxGroupAttribute(string groupId, string groupTitle, BoxStyle style)
        {
            GroupId = groupId;
            GroupTitle = string.IsNullOrEmpty(groupTitle) ? groupId : groupTitle;
            Style = style;
            Foldable = false;
            ExpandedByDefault = true;
            ShowLabel = true;
            BackgroundColor = Color.clear;
            HeaderColor = Color.clear;
            Order = 0;
        }

        /// <summary>
        /// Creates a BoxGroup attribute with foldable option.
        /// </summary>
        /// <param name="groupId">Unique identifier for this group.</param>
        /// <param name="foldable">Whether the group can be collapsed.</param>
        /// <param name="expandedByDefault">Whether the group starts expanded.</param>
        public BoxGroupAttribute(string groupId, bool foldable, bool expandedByDefault = true)
        {
            GroupId = groupId;
            GroupTitle = groupId;
            Style = BoxStyle.Default;
            Foldable = foldable;
            ExpandedByDefault = expandedByDefault;
            ShowLabel = true;
            BackgroundColor = Color.clear;
            HeaderColor = Color.clear;
            Order = 0;
        }

        /// <summary>
        /// Creates a BoxGroup attribute with full customization.
        /// </summary>
        /// <param name="groupId">Unique identifier for this group.</param>
        /// <param name="groupTitle">Display title for the group.</param>
        /// <param name="style">Visual style of the box.</param>
        /// <param name="foldable">Whether the group can be collapsed.</param>
        /// <param name="expandedByDefault">Whether the group starts expanded.</param>
        /// <param name="showLabel">Whether to show the group title.</param>
        /// <param name="order">Display order (lower values first).</param>
        public BoxGroupAttribute(
            string groupId,
            string groupTitle = null,
            BoxStyle style = BoxStyle.Default,
            bool foldable = false,
            bool expandedByDefault = true,
            bool showLabel = true,
            int order = 0)
        {
            GroupId = groupId;
            GroupTitle = string.IsNullOrEmpty(groupTitle) ? groupId : groupTitle;
            Style = style;
            Foldable = foldable;
            ExpandedByDefault = expandedByDefault;
            ShowLabel = showLabel;
            BackgroundColor = Color.clear;
            HeaderColor = Color.clear;
            Order = order;
        }
    }
}
