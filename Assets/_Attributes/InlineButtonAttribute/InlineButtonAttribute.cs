using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// Attribute to display an inline button next to a field in the Unity Inspector.
    /// The button will invoke a specified method when clicked.
    /// Similar to Odin Inspector's InlineButton attribute.
    /// </summary>
    /// <example>
    /// // Basic usage with method name
    /// [InlineButton("ResetValue")]
    /// public int health = 100;
    ///
    /// // With custom button label
    /// [InlineButton("ResetValue", "Reset")]
    /// public int health = 100;
    ///
    /// // With custom button width
    /// [InlineButton("ResetValue", "Reset", 80f)]
    /// public int health = 100;
    ///
    /// // Multiple buttons
    /// [InlineButton("Increment", "+")]
    /// [InlineButton("Decrement", "-")]
    /// public int counter = 0;
    ///
    /// // Method can be private or public
    /// private void ResetValue()
    /// {
    ///     health = 100;
    /// }
    ///
    /// private void Increment()
    /// {
    ///     counter++;
    /// }
    ///
    /// private void Decrement()
    /// {
    ///     counter--;
    /// }
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class InlineButtonAttribute : PropertyAttribute
    {
        /// <summary>
        /// Name of the method to invoke when the button is clicked.
        /// The method must be parameterless and can be public or private.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Custom label for the button. If null or empty, the method name will be formatted and used.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Width of the button in pixels. If 0 or negative, a default width will be used.
        /// </summary>
        public float ButtonWidth { get; }

        /// <summary>
        /// Whether to mark the target object as dirty after button click.
        /// This ensures changes are saved properly.
        /// </summary>
        public bool MarkDirty { get; }

        /// <summary>
        /// Creates a new InlineButton attribute with specified method name.
        /// </summary>
        /// <param name="methodName">Name of the method to invoke.</param>
        public InlineButtonAttribute(string methodName)
        {
            MethodName = methodName;
            Label = null;
            ButtonWidth = 0f;
            MarkDirty = true;
        }

        /// <summary>
        /// Creates a new InlineButton attribute with method name and custom label.
        /// </summary>
        /// <param name="methodName">Name of the method to invoke.</param>
        /// <param name="label">Custom label for the button.</param>
        public InlineButtonAttribute(string methodName, string label)
        {
            MethodName = methodName;
            Label = label;
            ButtonWidth = 0f;
            MarkDirty = true;
        }

        /// <summary>
        /// Creates a new InlineButton attribute with method name, label, and custom width.
        /// </summary>
        /// <param name="methodName">Name of the method to invoke.</param>
        /// <param name="label">Custom label for the button.</param>
        /// <param name="buttonWidth">Width of the button in pixels.</param>
        public InlineButtonAttribute(string methodName, string label, float buttonWidth)
        {
            MethodName = methodName;
            Label = label;
            ButtonWidth = buttonWidth;
            MarkDirty = true;
        }

        /// <summary>
        /// Creates a new InlineButton attribute with full customization.
        /// </summary>
        /// <param name="methodName">Name of the method to invoke.</param>
        /// <param name="label">Custom label for the button.</param>
        /// <param name="buttonWidth">Width of the button in pixels.</param>
        /// <param name="markDirty">Whether to mark the object dirty after invocation.</param>
        public InlineButtonAttribute(string methodName, string label, float buttonWidth, bool markDirty)
        {
            MethodName = methodName;
            Label = label;
            ButtonWidth = buttonWidth;
            MarkDirty = markDirty;
        }
    }
}
