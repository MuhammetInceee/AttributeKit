#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AttributeKit.Editor
{
    /// <summary>
    /// Custom property drawer for InlineButtonsAttribute.
    /// Displays multiple inline buttons next to a field.
    /// </summary>
    [CustomPropertyDrawer(typeof(InlineButtonsAttribute))]
    public class InlineButtonsAttributeDrawer : PropertyDrawer
    {
        private const float DefaultButtonWidth = 60f;
        private const float ButtonSpacing = 2f;
        private const float MinButtonWidth = 30f;
        private const float MaxButtonWidth = 200f;

        private static readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as InlineButtonsAttribute;
            if (attr == null || attr.ButtonDefinitions == null || attr.ButtonDefinitions.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Calculate total button width
            float totalButtonWidth = 0f;
            foreach (var definition in attr.ButtonDefinitions)
            {
                totalButtonWidth += CalculateButtonWidth(definition) + ButtonSpacing;
            }

            // Layout: [Property Field] [Button1] [Button2] [Button3]...
            Rect fieldRect = new Rect(
                position.x,
                position.y,
                position.width - totalButtonWidth,
                position.height
            );

            // Draw the property field
            EditorGUI.PropertyField(fieldRect, property, label, true);

            // Draw all buttons
            float buttonX = fieldRect.xMax + ButtonSpacing;
            foreach (var definition in attr.ButtonDefinitions)
            {
                if (InlineButtonsAttribute.ParseDefinition(definition, out string methodName, out string buttonLabel, out float buttonWidth))
                {
                    float actualWidth = CalculateButtonWidth(definition);
                    Rect buttonRect = new Rect(buttonX, position.y, actualWidth, EditorGUIUtility.singleLineHeight);

                    DrawButton(buttonRect, property, methodName, buttonLabel, attr.MarkDirty);

                    buttonX += actualWidth + ButtonSpacing;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private void DrawButton(Rect buttonRect, SerializedProperty property, string methodName, string label, bool markDirty)
        {
            // Use method name as label if not specified
            string buttonLabel = string.IsNullOrEmpty(label) ? FormatMethodName(methodName) : label;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                padding = new RectOffset(4, 4, 2, 2)
            };

            if (GUI.Button(buttonRect, new GUIContent(buttonLabel, $"Invoke: {methodName}()"), buttonStyle))
            {
                InvokeMethod(property, methodName, markDirty);
            }
        }

        private void InvokeMethod(SerializedProperty property, string methodName, bool markDirty)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                Debug.LogError("[InlineButtons] Method name is null or empty.");
                return;
            }

            var targetObject = property.serializedObject.targetObject;
            if (targetObject == null)
            {
                Debug.LogError("[InlineButtons] Target object is null.");
                return;
            }

            try
            {
                var method = GetMethod(targetObject, methodName);

                if (method == null)
                {
                    Debug.LogError($"[InlineButtons] Method '{methodName}' not found on '{targetObject.GetType().Name}'. " +
                                 "Make sure the method exists and is accessible.");
                    return;
                }

                // Validate method parameters
                var parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    Debug.LogWarning($"[InlineButtons] Method '{methodName}' has parameters. " +
                                   "InlineButtons only supports parameterless methods. Method will not be invoked.");
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
                        Debug.LogWarning($"[InlineButtons] Cannot start coroutine '{methodName}' in edit mode.");
                    }
                }

                // Mark dirty if requested
                if (markDirty)
                {
                    EditorUtility.SetDirty(targetObject);
                    property.serializedObject.Update();
                }

                // Repaint inspector
                if (EditorWindow.focusedWindow != null)
                {
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            catch (TargetInvocationException ex)
            {
                Debug.LogError($"[InlineButtons] Error invoking method '{methodName}' on '{targetObject.name}': " +
                             $"{ex.InnerException?.Message ?? ex.Message}\n{ex.InnerException?.StackTrace ?? ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InlineButtons] Unexpected error invoking method '{methodName}' on '{targetObject.name}': " +
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

        private float CalculateButtonWidth(string definition)
        {
            if (InlineButtonsAttribute.ParseDefinition(definition, out string methodName, out string label, out float width))
            {
                // Use explicit width if provided
                if (width > 0)
                {
                    return Mathf.Clamp(width, MinButtonWidth, MaxButtonWidth);
                }

                // Calculate based on label
                string displayLabel = string.IsNullOrEmpty(label) ? FormatMethodName(methodName) : label;
                float calculatedWidth = GUI.skin.button.CalcSize(new GUIContent(displayLabel)).x;
                return Mathf.Clamp(calculatedWidth, MinButtonWidth, DefaultButtonWidth);
            }

            return DefaultButtonWidth;
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

            if (result.Length > 0)
            {
                result = char.ToUpper(result[0]) + result.Substring(1);
            }

            return result;
        }
    }
}
#endif
