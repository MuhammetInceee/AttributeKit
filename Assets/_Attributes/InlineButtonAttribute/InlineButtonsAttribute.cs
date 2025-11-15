using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// Attribute to display multiple inline buttons next to a field in the Unity Inspector.
    /// Each button definition is a string in format: "MethodName" or "MethodName|Label" or "MethodName|Label|Width"
    /// </summary>
    /// <example>
    /// // Basic usage - method names only
    /// [InlineButtons("Increment", "Decrement", "Reset")]
    /// public int counter = 0;
    ///
    /// // With custom labels
    /// [InlineButtons("Increment|+", "Decrement|-", "Reset|↻")]
    /// public int counter = 0;
    ///
    /// // With custom labels and widths
    /// [InlineButtons("Increment|+|30", "Decrement|-|30", "Reset|Reset|50")]
    /// public int counter = 0;
    ///
    /// // Methods
    /// private void Increment() => counter++;
    /// private void Decrement() => counter--;
    /// private void Reset() => counter = 0;
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class InlineButtonsAttribute : PropertyAttribute
    {
        /// <summary>
        /// Button definitions in format: "MethodName|Label|Width"
        /// Label and Width are optional.
        /// </summary>
        public string[] ButtonDefinitions { get; }

        /// <summary>
        /// Whether to mark the target object as dirty after button click.
        /// </summary>
        public bool MarkDirty { get; }

        /// <summary>
        /// Creates a new InlineButtons attribute with button definitions.
        /// </summary>
        /// <param name="buttonDefinitions">
        /// Button definitions in format: "MethodName|Label|Width"
        /// Examples: "Reset", "Reset|↻", "Reset|Reset|50"
        /// </param>
        public InlineButtonsAttribute(params string[] buttonDefinitions)
        {
            ButtonDefinitions = buttonDefinitions ?? new string[0];
            MarkDirty = true;
        }

        /// <summary>
        /// Parses a button definition string into components.
        /// </summary>
        /// <param name="definition">Definition string</param>
        /// <param name="methodName">Output: Method name</param>
        /// <param name="label">Output: Button label (null if not specified)</param>
        /// <param name="width">Output: Button width (0 if not specified)</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool ParseDefinition(string definition, out string methodName, out string label, out float width)
        {
            methodName = null;
            label = null;
            width = 0f;

            if (string.IsNullOrEmpty(definition))
                return false;

            var parts = definition.Split('|');

            // Method name (required)
            methodName = parts[0].Trim();
            if (string.IsNullOrEmpty(methodName))
                return false;

            // Label (optional)
            if (parts.Length > 1)
            {
                label = parts[1].Trim();
            }

            // Width (optional)
            if (parts.Length > 2)
            {
                if (float.TryParse(parts[2].Trim(), out float parsedWidth))
                {
                    width = parsedWidth;
                }
            }

            return true;
        }
    }
}
