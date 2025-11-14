using System;

namespace AttributeKit
{
    /// <summary>
    /// Attribute to display a button in the Unity Inspector for invoking methods.
    /// Can be applied to methods with no parameters or a single UnityEngine.Object parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class InspectorButtonAttribute : Attribute
    {
        /// <summary>
        /// Custom label for the button. If null or empty, the method name will be used.
        /// </summary>
        public string ButtonLabel { get; }

        /// <summary>
        /// Whether to mark the target object as dirty after button click.
        /// </summary>
        public bool MarkDirty { get; }

        /// <summary>
        /// Name of a field to mark as dirty. If the field is a collection, all Unity Objects in it will be marked dirty.
        /// </summary>
        public string FieldToMarkDirty { get; }

        /// <summary>
        /// Creates a new InspectorButton attribute.
        /// </summary>
        /// <param name="buttonLabel">Custom label for the button. If null, method name is used.</param>
        /// <param name="markDirty">Whether to mark the object dirty after invocation.</param>
        /// <param name="fieldToMarkDirty">Optional field name to mark dirty (supports collections).</param>
        public InspectorButtonAttribute(string buttonLabel = null, bool markDirty = false, string fieldToMarkDirty = null)
        {
            ButtonLabel = buttonLabel;
            MarkDirty = markDirty;
            FieldToMarkDirty = fieldToMarkDirty;
        }

        /// <summary>
        /// Creates a new InspectorButton attribute with auto-generated label.
        /// </summary>
        /// <param name="markDirty">Whether to mark the object dirty after invocation.</param>
        /// <param name="fieldToMarkDirty">Optional field name to mark dirty (supports collections).</param>
        public InspectorButtonAttribute(bool markDirty, string fieldToMarkDirty = null)
        {
            ButtonLabel = null;
            MarkDirty = markDirty;
            FieldToMarkDirty = fieldToMarkDirty;
        }
    }
}
