#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AttributeKit.Editor
{
    /// <summary>
    /// Custom property drawer for ReadOnlyAttribute.
    /// Disables field editing in the inspector based on the specified mode.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        private const float IndicatorWidth = 4f;
        private const float IndicatorPadding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = this.attribute as ReadOnlyAttribute;
            if (attribute == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Determine if field should be read-only
            bool isReadOnly = ShouldBeReadOnly(attribute.Mode);

            // Add custom tooltip if specified
            if (!string.IsNullOrEmpty(attribute.TooltipText))
            {
                label.tooltip = attribute.TooltipText;
            }
            else if (isReadOnly)
            {
                // Add default tooltip based on mode
                label.tooltip = GetDefaultTooltip(attribute.Mode);
            }

            // Draw visual indicator if enabled
            if (attribute.ShowIndicator && isReadOnly)
            {
                DrawReadOnlyIndicator(position);
                position.x += IndicatorWidth + IndicatorPadding;
                position.width -= IndicatorWidth + IndicatorPadding;
            }

            // Draw the property field with GUI disabled if read-only
            bool wasEnabled = GUI.enabled;
            GUI.enabled = !isReadOnly;

            EditorGUI.PropertyField(position, property, label, true);

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Return the default height for the property (handles arrays, nested objects, etc.)
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool ShouldBeReadOnly(ReadOnlyMode mode)
        {
            return mode switch
            {
                ReadOnlyMode.Always => true,
                ReadOnlyMode.OnlyInPlayMode => Application.isPlaying,
                ReadOnlyMode.OnlyInEditMode => !Application.isPlaying,
                _ => true
            };
        }

        private string GetDefaultTooltip(ReadOnlyMode mode)
        {
            return mode switch
            {
                ReadOnlyMode.Always => "This field is read-only",
                ReadOnlyMode.OnlyInPlayMode => "This field is read-only during play mode",
                ReadOnlyMode.OnlyInEditMode => "This field is read-only during edit mode",
                _ => "This field is read-only"
            };
        }

        private void DrawReadOnlyIndicator(Rect position)
        {
            Color indicatorColor = GetIndicatorColor();
            Rect indicatorRect = new Rect(
                position.x,
                position.y,
                IndicatorWidth,
                position.height);

            EditorGUI.DrawRect(indicatorRect, indicatorColor);
        }

        private Color GetIndicatorColor()
        {
            // Use different colors based on pro/light skin
            if (EditorGUIUtility.isProSkin)
            {
                return new Color(1f, 0.5f, 0f, 0.8f); // Orange
            }
            else
            {
                return new Color(1f, 0.3f, 0f, 0.8f); // Darker orange
            }
        }
    }

    /// <summary>
    /// Decorator drawer for displaying read-only fields in arrays and lists.
    /// This ensures that array/list elements also respect the ReadOnly attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute), useForChildren: true)]
    public class ReadOnlyDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return 0f;
        }

        public override void OnGUI(Rect position)
        {
            // This drawer doesn't draw anything, it just exists to handle arrays/lists
        }
    }
}
#endif
