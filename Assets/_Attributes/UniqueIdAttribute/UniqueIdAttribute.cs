using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// ID generation strategies for unique identifiers.
    /// </summary>
    public enum IdGenerationType
    {
        /// <summary>Full GUID with hyphens (e.g., 550e8400-e29b-41d4-a716-446655440000)</summary>
        GUID,
        /// <summary>GUID without hyphens (e.g., 550e8400e29b41d4a716446655440000)</summary>
        GUIDNoHyphens,
        /// <summary>Short 8-character ID (e.g., a3f8c9d2)</summary>
        ShortID,
        /// <summary>Timestamp-based ID (e.g., 20250115143052_a3f8)</summary>
        Timestamp,
        /// <summary>Numeric sequential ID (e.g., 00001, 00002, etc.)</summary>
        Sequential
    }

    /// <summary>
    /// Case formatting for generated IDs.
    /// </summary>
    public enum IdCaseFormat
    {
        /// <summary>Lowercase letters (default)</summary>
        Lower,
        /// <summary>Uppercase letters</summary>
        Upper,
        /// <summary>Keep original case</summary>
        Default
    }

    /// <summary>
    /// Attribute to automatically generate and manage unique identifiers in the inspector.
    /// Supports various ID generation strategies and formatting options.
    /// </summary>
    /// <example>
    /// // Simple GUID generation
    /// [UniqueId]
    /// public string itemId;
    ///
    /// // GUID without hyphens, uppercase
    /// [UniqueId(IdGenerationType.GUIDNoHyphens, IdCaseFormat.Upper)]
    /// public string entityId;
    ///
    /// // Short 8-character ID
    /// [UniqueId(IdGenerationType.ShortID)]
    /// public string quickId;
    ///
    /// // Auto-generate on creation
    /// [UniqueId(autoGenerate: true)]
    /// public string autoId;
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class UniqueIdAttribute : PropertyAttribute
    {
        /// <summary>
        /// The type of ID generation to use.
        /// </summary>
        public IdGenerationType GenerationType { get; }

        /// <summary>
        /// Case formatting for the generated ID.
        /// </summary>
        public IdCaseFormat CaseFormat { get; }

        /// <summary>
        /// Whether to automatically generate an ID if the field is empty.
        /// </summary>
        public bool AutoGenerate { get; }

        /// <summary>
        /// Prefix to add before the generated ID (optional).
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Suffix to add after the generated ID (optional).
        /// </summary>
        public string Suffix { get; }

        /// <summary>
        /// Creates a UniqueId attribute with default settings (GUID, lowercase, no auto-generate).
        /// </summary>
        public UniqueIdAttribute()
        {
            GenerationType = IdGenerationType.GUID;
            CaseFormat = IdCaseFormat.Lower;
            AutoGenerate = false;
            Prefix = string.Empty;
            Suffix = string.Empty;
        }

        /// <summary>
        /// Creates a UniqueId attribute with specified generation type.
        /// </summary>
        /// <param name="generationType">Type of ID generation to use.</param>
        /// <param name="caseFormat">Case formatting for the ID.</param>
        /// <param name="autoGenerate">Whether to auto-generate if empty.</param>
        public UniqueIdAttribute(IdGenerationType generationType, IdCaseFormat caseFormat = IdCaseFormat.Lower, bool autoGenerate = false)
        {
            GenerationType = generationType;
            CaseFormat = caseFormat;
            AutoGenerate = autoGenerate;
            Prefix = string.Empty;
            Suffix = string.Empty;
        }

        /// <summary>
        /// Creates a UniqueId attribute with auto-generate option.
        /// </summary>
        /// <param name="autoGenerate">Whether to auto-generate if empty.</param>
        public UniqueIdAttribute(bool autoGenerate)
        {
            GenerationType = IdGenerationType.GUID;
            CaseFormat = IdCaseFormat.Lower;
            AutoGenerate = autoGenerate;
            Prefix = string.Empty;
            Suffix = string.Empty;
        }

        /// <summary>
        /// Creates a UniqueId attribute with prefix and suffix.
        /// </summary>
        /// <param name="generationType">Type of ID generation to use.</param>
        /// <param name="prefix">Prefix to add before the ID.</param>
        /// <param name="suffix">Suffix to add after the ID.</param>
        public UniqueIdAttribute(IdGenerationType generationType, string prefix, string suffix = "")
        {
            GenerationType = generationType;
            CaseFormat = IdCaseFormat.Lower;
            AutoGenerate = false;
            Prefix = prefix ?? string.Empty;
            Suffix = suffix ?? string.Empty;
        }
    }
}
