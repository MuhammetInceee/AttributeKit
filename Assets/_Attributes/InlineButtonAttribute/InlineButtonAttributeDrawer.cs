#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AttributeKit.Editor
{
    /// <summary>
    /// Custom property drawer for InlineButtonAttribute.
    /// Displays inline buttons next to fields that invoke specified methods when clicked.
    /// </summary>
    [CustomPropertyDrawer(typeof(InlineButtonAttribute), true)]
    public class InlineButtonAttributeDrawer : PropertyDrawer
    {
        private const float DefaultButtonWidth = 60f;
        private const float ButtonSpacing = 2f;
        private const float MinButtonWidth = 30f;
        private const float MaxButtonWidth = 200f;

        private static readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

        // Track which fields have been drawn in current GUI cycle
        private static readonly HashSet<string> _drawnFields = new HashSet<string>();
        private static readonly HashSet<string> _heightReservedFields = new HashSet<string>();
        private static int _lastEventID = -1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currentAttribute = attribute as InlineButtonAttribute;
            if (currentAttribute == null)
            {
                Debug.Log($"[InlineButton] CurrentAttribute is NULL for {property.propertyPath}");
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Get all attributes
            var allAttributes = GetAllAttributes(property);
            Debug.Log($"[InlineButton] {property.propertyPath} has {allAttributes.Count} attributes, current: {currentAttribute.MethodName}");

            if (allAttributes.Count == 0)
            {
                Debug.LogWarning($"[InlineButton] No attributes found for {property.propertyPath}");
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Find my index in the attribute list
            int myIndex = -1;
            for (int i = 0; i < allAttributes.Count; i++)
            {
                if (allAttributes[i].MethodName == currentAttribute.MethodName &&
                    (allAttributes[i].Label ?? "") == (currentAttribute.Label ?? ""))
                {
                    myIndex = i;
                    break;
                }
            }

            Debug.Log($"[InlineButton] {property.propertyPath} - {currentAttribute.MethodName} is at index {myIndex}");

            // Calculate total button width
            float totalButtonWidth = 0f;
            foreach (var attr in allAttributes)
            {
                totalButtonWidth += GetButtonWidth(attr) + ButtonSpacing;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Only first attribute (index 0) draws everything
            if (myIndex == 0)
            {
                Debug.Log($"[InlineButton] DRAWING {property.propertyPath} with {allAttributes.Count} buttons");

                // Layout: [Property Field] [All Buttons]
                Rect fieldRect = new Rect(
                    position.x,
                    position.y,
                    position.width - totalButtonWidth,
                    position.height
                );

                EditorGUI.PropertyField(fieldRect, property, label, true);

                // Draw all buttons
                float buttonX = fieldRect.xMax + ButtonSpacing;
                foreach (var attr in allAttributes)
                {
                    float btnWidth = GetButtonWidth(attr);
                    Rect buttonRect = new Rect(buttonX, position.y, btnWidth, EditorGUIUtility.singleLineHeight);
                    DrawInlineButton(buttonRect, property, attr);
                    buttonX += btnWidth + ButtonSpacing;
                }
            }
            else
            {
                Debug.Log($"[InlineButton] SKIPPING draw for {property.propertyPath} - {currentAttribute.MethodName} (not first)");
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var allAttrs = GetAllAttributes(property);
            var currentAttr = attribute as InlineButtonAttribute;

            Debug.Log($"[InlineButton HEIGHT] {property.propertyPath} - attrs={allAttrs.Count}, current={currentAttr?.MethodName}");

            if (allAttrs.Count == 0)
            {
                Debug.Log($"[InlineButton HEIGHT] {property.propertyPath} - No attrs, returning normal height");
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            if (allAttrs.Count == 1)
            {
                Debug.Log($"[InlineButton HEIGHT] {property.propertyPath} - Single attr, returning normal height");
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            var firstAttr = allAttrs[0];
            bool isFirst = currentAttr != null &&
                          currentAttr.MethodName == firstAttr.MethodName &&
                          (currentAttr.Label ?? "") == (firstAttr.Label ?? "");

            if (isFirst)
            {
                float height = EditorGUI.GetPropertyHeight(property, label, true);
                Debug.Log($"[InlineButton HEIGHT] {property.propertyPath} - {currentAttr.MethodName} IS FIRST, height={height}");
                return height;
            }

            Debug.Log($"[InlineButton HEIGHT] {property.propertyPath} - {currentAttr?.MethodName} NOT FIRST, height=0");
            return 0f;
        }

        private List<InlineButtonAttribute> GetAllAttributes(SerializedProperty property)
        {
            if (fieldInfo == null) return new List<InlineButtonAttribute>();

            return fieldInfo
                .GetCustomAttributes(typeof(InlineButtonAttribute), true)
                .Cast<InlineButtonAttribute>()
                .ToList();
        }

        private void DrawInlineButton(Rect buttonRect, SerializedProperty property, InlineButtonAttribute attribute)
        {
            string buttonLabel = GetButtonLabel(attribute);

            // Create a styled button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                padding = new RectOffset(4, 4, 2, 2)
            };

            if (GUI.Button(buttonRect, new GUIContent(buttonLabel, GetButtonTooltip(attribute)), buttonStyle))
            {
                InvokeMethod(property, attribute);
            }
        }

        private void InvokeMethod(SerializedProperty property, InlineButtonAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.MethodName))
            {
                Debug.LogError("[InlineButton] Method name is null or empty.");
                return;
            }

            var targetObject = property.serializedObject.targetObject;
            if (targetObject == null)
            {
                Debug.LogError("[InlineButton] Target object is null.");
                return;
            }

            try
            {
                var method = GetMethod(targetObject, attribute.MethodName);

                if (method == null)
                {
                    Debug.LogError($"[InlineButton] Method '{attribute.MethodName}' not found on '{targetObject.GetType().Name}'. " +
                                 "Make sure the method exists and is accessible.");
                    return;
                }

                // Validate method parameters
                var parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    Debug.LogWarning($"[InlineButton] Method '{attribute.MethodName}' has parameters. " +
                                   "InlineButton only supports parameterless methods. Method will not be invoked.");
                    return;
                }

                // Invoke the method
                var result = method.Invoke(targetObject, null);

                // Handle coroutines
                if (result is IEnumerator coroutine && targetObject is MonoBehaviour monoBehaviour)
                {
                    if (Application.isPlaying)
                    {
                        monoBehaviour.StartCoroutine(coroutine);
                    }
                    else
                    {
                        Debug.LogWarning($"[InlineButton] Cannot start coroutine '{attribute.MethodName}' in edit mode.");
                    }
                }

                // Mark dirty if requested
                if (attribute.MarkDirty)
                {
                    EditorUtility.SetDirty(targetObject);
                    property.serializedObject.Update();
                }

                // Repaint inspector to reflect changes
                if (EditorWindow.focusedWindow != null)
                {
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            catch (TargetInvocationException ex)
            {
                Debug.LogError($"[InlineButton] Error invoking method '{attribute.MethodName}' on '{targetObject.name}': " +
                             $"{ex.InnerException?.Message ?? ex.Message}\n{ex.InnerException?.StackTrace ?? ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InlineButton] Unexpected error invoking method '{attribute.MethodName}' on '{targetObject.name}': " +
                             $"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private MethodInfo GetMethod(UnityEngine.Object targetObject, string methodName)
        {
            var type = targetObject.GetType();
            string cacheKey = $"{type.FullName}.{methodName}";

            if (_methodCache.TryGetValue(cacheKey, out var cachedMethod))
            {
                return cachedMethod;
            }

            // Search for method (public and non-public, instance methods)
            var method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method != null)
            {
                _methodCache[cacheKey] = method;
            }

            return method;
        }
        
        private float GetButtonWidth(InlineButtonAttribute attribute)
        {
            if (attribute.ButtonWidth > 0)
            {
                return Mathf.Clamp(attribute.ButtonWidth, MinButtonWidth, MaxButtonWidth);
            }

            // Calculate width based on label length if no explicit width provided
            string label = GetButtonLabel(attribute);
            float calculatedWidth = GUI.skin.button.CalcSize(new GUIContent(label)).x;

            return Mathf.Clamp(calculatedWidth, MinButtonWidth, DefaultButtonWidth);
        }

        private string GetButtonLabel(InlineButtonAttribute attribute)
        {
            if (!string.IsNullOrEmpty(attribute.Label))
            {
                return attribute.Label;
            }

            // Format method name if no label provided
            return FormatMethodName(attribute.MethodName);
        }

        private string GetButtonTooltip(InlineButtonAttribute attribute)
        {
            return $"Invoke: {attribute.MethodName}()";
        }

        private string FormatMethodName(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
                return methodName;

            // Convert PascalCase or camelCase to readable format
            var result = System.Text.RegularExpressions.Regex.Replace(
                methodName,
                "([a-z])([A-Z])",
                "$1 $2");

            // Capitalize first letter
            if (result.Length > 0)
            {
                result = char.ToUpper(result[0]) + result.Substring(1);
            }

            return result;
        }
    }

    /// <summary>
    /// Utility class for InlineButton operations and method validation.
    /// </summary>
    public static class InlineButtonUtility
    {
        /// <summary>
        /// Validates if a method exists and is compatible with InlineButton.
        /// </summary>
        /// <param name="targetType">Type containing the method.</param>
        /// <param name="methodName">Name of the method to validate.</param>
        /// <param name="errorMessage">Error message if validation fails.</param>
        /// <returns>True if method is valid, false otherwise.</returns>
        public static bool ValidateMethod(Type targetType, string methodName, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(methodName))
            {
                errorMessage = "Method name is null or empty.";
                return false;
            }

            var method = targetType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method == null)
            {
                errorMessage = $"Method '{methodName}' not found on type '{targetType.Name}'.";
                return false;
            }

            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                errorMessage = $"Method '{methodName}' has parameters. InlineButton only supports parameterless methods.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all methods in a type that are compatible with InlineButton.
        /// </summary>
        /// <param name="targetType">Type to search.</param>
        /// <returns>List of compatible method names.</returns>
        public static List<string> GetCompatibleMethods(Type targetType)
        {
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return methods
                .Where(m => m.GetParameters().Length == 0) // Parameterless only
                .Where(m => !m.IsSpecialName) // Exclude property getters/setters
                .Where(m => m.DeclaringType != typeof(object)) // Exclude Object methods
                .Where(m => m.DeclaringType != typeof(MonoBehaviour) || m.Name == "Start" || m.Name == "Update") // Include user methods
                .Select(m => m.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToList();
        }
    }
}
#endif
