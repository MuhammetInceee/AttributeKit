#if UNITY_EDITOR
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AttributeKit.Editor
{
    /// <summary>
    /// Custom property drawer for UniqueIdAttribute.
    /// Provides UI for generating, copying, and managing unique identifiers.
    /// </summary>
    [CustomPropertyDrawer(typeof(UniqueIdAttribute))]
    public class UniqueIdAttributeDrawer : PropertyDrawer
    {
        private static int _sequentialCounter = 1;
        private const float ButtonWidth = 70f;
        private const float ButtonSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Validate property type
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [UniqueId] with string fields only.");
                return;
            }

            var attribute = this.attribute as UniqueIdAttribute;
            if (attribute == null)
                return;

            EditorGUI.BeginProperty(position, label, property);

            // Auto-generate if empty and auto-generate is enabled
            if (attribute.AutoGenerate && string.IsNullOrEmpty(property.stringValue))
            {
                property.stringValue = GenerateId(attribute);
                property.serializedObject.ApplyModifiedProperties();
            }

            // Calculate layout
            float totalButtonWidth = (ButtonWidth * 2) + ButtonSpacing;
            Rect fieldRect = new Rect(position.x, position.y, position.width - totalButtonWidth - 5f, position.height);
            Rect copyButtonRect = new Rect(fieldRect.xMax + 5f, position.y, ButtonWidth, position.height);
            Rect generateButtonRect = new Rect(copyButtonRect.xMax + ButtonSpacing, position.y, ButtonWidth, position.height);

            // Draw the ID field (disabled to prevent manual editing for better ID integrity)
            GUI.enabled = true; // Keep enabled as per user request (no readonly)
            EditorGUI.PropertyField(fieldRect, property, label);
            GUI.enabled = true;

            // Copy button
            if (GUI.Button(copyButtonRect, new GUIContent("Copy", "Copy ID to clipboard")))
            {
                if (!string.IsNullOrEmpty(property.stringValue))
                {
                    EditorGUIUtility.systemCopyBuffer = property.stringValue;
                    Debug.Log($"[UniqueId] Copied to clipboard: {property.stringValue}");
                }
            }

            // Generate/Regenerate button
            string buttonLabel = string.IsNullOrEmpty(property.stringValue) ? "Generate" : "Regenerate";
            if (GUI.Button(generateButtonRect, new GUIContent(buttonLabel, "Generate a new unique ID")))
            {
                bool shouldGenerate = true;

                // Show confirmation dialog only if ID already exists
                if (!string.IsNullOrEmpty(property.stringValue))
                {
                    shouldGenerate = EditorUtility.DisplayDialog(
                        "Regenerate ID",
                        $"Current ID: {property.stringValue}\n\nAre you sure you want to generate a new ID? This action cannot be undone.",
                        "Yes", "Cancel");
                }

                if (shouldGenerate)
                {
                    property.stringValue = GenerateId(attribute);
                    property.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }

            EditorGUI.EndProperty();
        }

        private string GenerateId(UniqueIdAttribute attribute)
        {
            string id = attribute.GenerationType switch
            {
                IdGenerationType.GUID => Guid.NewGuid().ToString(),
                IdGenerationType.GUIDNoHyphens => Guid.NewGuid().ToString("N"),
                IdGenerationType.ShortID => GenerateShortId(),
                IdGenerationType.Timestamp => GenerateTimestampId(),
                IdGenerationType.Sequential => GenerateSequentialId(),
                _ => Guid.NewGuid().ToString()
            };

            // Apply case formatting
            id = attribute.CaseFormat switch
            {
                IdCaseFormat.Upper => id.ToUpper(),
                IdCaseFormat.Lower => id.ToLower(),
                _ => id
            };

            // Apply prefix and suffix
            if (!string.IsNullOrEmpty(attribute.Prefix) || !string.IsNullOrEmpty(attribute.Suffix))
            {
                id = $"{attribute.Prefix}{id}{attribute.Suffix}";
            }

            return id;
        }

        private string GenerateShortId()
        {
            // Generate 8-character alphanumeric ID
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var sb = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                sb.Append(buffer[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private string GenerateTimestampId()
        {
            // Format: yyyyMMddHHmmss_xxxx
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string randomPart = GenerateShortId().Substring(0, 4);
            return $"{timestamp}_{randomPart}";
        }

        private string GenerateSequentialId()
        {
            // Format: 00001, 00002, etc.
            string id = _sequentialCounter.ToString("D5");
            _sequentialCounter++;
            return id;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    /// <summary>
    /// Utility class for UniqueId operations outside of the drawer.
    /// </summary>
    public static class UniqueIdUtility
    {
        /// <summary>
        /// Generates a unique ID based on the specified generation type.
        /// </summary>
        /// <param name="generationType">Type of ID to generate.</param>
        /// <param name="caseFormat">Case formatting for the ID.</param>
        /// <param name="prefix">Optional prefix.</param>
        /// <param name="suffix">Optional suffix.</param>
        /// <returns>Generated unique ID.</returns>
        public static string GenerateId(
            IdGenerationType generationType = IdGenerationType.GUID,
            IdCaseFormat caseFormat = IdCaseFormat.Lower,
            string prefix = "",
            string suffix = "")
        {
            string id = generationType switch
            {
                IdGenerationType.GUID => Guid.NewGuid().ToString(),
                IdGenerationType.GUIDNoHyphens => Guid.NewGuid().ToString("N"),
                IdGenerationType.ShortID => GenerateShortId(),
                IdGenerationType.Timestamp => GenerateTimestampId(),
                IdGenerationType.Sequential => DateTime.Now.Ticks.ToString("D19"),
                _ => Guid.NewGuid().ToString()
            };

            // Apply case formatting
            id = caseFormat switch
            {
                IdCaseFormat.Upper => id.ToUpper(),
                IdCaseFormat.Lower => id.ToLower(),
                _ => id
            };

            // Apply prefix and suffix
            if (!string.IsNullOrEmpty(prefix) || !string.IsNullOrEmpty(suffix))
            {
                id = $"{prefix}{id}{suffix}";
            }

            return id;
        }

        private static string GenerateShortId()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var sb = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                sb.Append(buffer[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private static string GenerateTimestampId()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string randomPart = GenerateShortId().Substring(0, 4);
            return $"{timestamp}_{randomPart}";
        }

        /// <summary>
        /// Validates if a string is a valid GUID.
        /// </summary>
        /// <param name="id">ID to validate.</param>
        /// <returns>True if valid GUID, false otherwise.</returns>
        public static bool IsValidGuid(string id)
        {
            return Guid.TryParse(id, out _);
        }

        /// <summary>
        /// Checks if an ID appears to be unique (not empty or default values).
        /// </summary>
        /// <param name="id">ID to check.</param>
        /// <returns>True if ID appears unique, false otherwise.</returns>
        public static bool IsValidUniqueId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            if (id == "00000000-0000-0000-0000-000000000000")
                return false;

            return true;
        }
    }
}
#endif
