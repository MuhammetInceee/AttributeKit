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
    /// Custom editor for MonoBehaviour to display InspectorButton attributes.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class ButtonAttributeDrawerForMonoBehaviour : UnityEditor.Editor
    {
        private ButtonDrawerHelper _helper;

        private void OnEnable()
        {
            _helper = new ButtonDrawerHelper();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _helper?.DrawButtons(targets);
        }
    }

    /// <summary>
    /// Custom editor for ScriptableObject to display InspectorButton attributes.
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class ButtonAttributeDrawerForScriptableObject : UnityEditor.Editor
    {
        private ButtonDrawerHelper _helper;

        private void OnEnable()
        {
            _helper = new ButtonDrawerHelper();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _helper?.DrawButtons(targets);
        }
    }

    /// <summary>
    /// Helper class for drawing inspector buttons with caching and error handling.
    /// </summary>
    public class ButtonDrawerHelper
    {
        private struct MethodData
        {
            public MethodInfo Method;
            public InspectorButtonAttribute Attribute;
            public string Label;
            public bool HasParameters;
            public ParameterInfo[] Parameters;
        }

        private readonly Dictionary<Type, List<MethodData>> _methodCache = new Dictionary<Type, List<MethodData>>();

        /// <summary>
        /// Draws buttons for all methods with InspectorButton attributes.
        /// </summary>
        /// <param name="targets">Target objects to draw buttons for.</param>
        public void DrawButtons(UnityEngine.Object[] targets)
        {
            if (targets == null || targets.Length == 0)
                return;

            foreach (var targetObject in targets)
            {
                if (targetObject == null)
                    continue;

                var type = targetObject.GetType();
                var methods = GetCachedMethods(type);

                if (methods.Count == 0)
                    continue;

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Inspector Buttons", EditorStyles.boldLabel);

                foreach (var methodData in methods)
                {
                    DrawButton(targetObject, methodData);
                }
            }
        }

        private List<MethodData> GetCachedMethods(Type type)
        {
            if (_methodCache.TryGetValue(type, out var cachedMethods))
                return cachedMethods;

            var methods = new List<MethodData>();
            var allMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in allMethods)
            {
                var attrs = method.GetCustomAttributes<InspectorButtonAttribute>(true).ToArray();

                foreach (var attr in attrs)
                {
                    var parameters = method.GetParameters();
                    var hasValidParameters = ValidateMethodParameters(method, parameters);

                    if (!hasValidParameters)
                    {
                        Debug.LogWarning($"[InspectorButton] Method '{method.Name}' in '{type.Name}' has unsupported parameters. " +
                                       "Only parameterless methods or methods with a single UnityEngine.Object parameter are supported.");
                        continue;
                    }

                    var methodData = new MethodData
                    {
                        Method = method,
                        Attribute = attr,
                        Label = string.IsNullOrEmpty(attr.ButtonLabel) ? FormatMethodName(method.Name) : attr.ButtonLabel,
                        HasParameters = parameters.Length > 0,
                        Parameters = parameters
                    };

                    methods.Add(methodData);
                }
            }

            _methodCache[type] = methods;
            return methods;
        }

        private bool ValidateMethodParameters(MethodInfo method, ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
                return true;

            if (parameters.Length == 1 && typeof(UnityEngine.Object).IsAssignableFrom(parameters[0].ParameterType))
                return true;

            return false;
        }

        private void DrawButton(UnityEngine.Object targetObject, MethodData methodData)
        {
            if (GUILayout.Button(methodData.Label, GUILayout.Height(25)))
            {
                InvokeMethod(targetObject, methodData);
            }
        }

        private void InvokeMethod(UnityEngine.Object targetObject, MethodData methodData)
        {
            try
            {
                object result;

                if (methodData.HasParameters)
                {
                    result = methodData.Method.Invoke(targetObject, new object[] { targetObject });
                }
                else
                {
                    result = methodData.Method.Invoke(targetObject, null);
                }

                // Handle coroutines
                if (result is IEnumerator coroutine && targetObject is MonoBehaviour monoBehaviour)
                {
                    if (Application.isPlaying)
                    {
                        monoBehaviour.StartCoroutine(coroutine);
                    }
                    else
                    {
                        Debug.LogWarning($"[InspectorButton] Cannot start coroutine '{methodData.Method.Name}' in edit mode.");
                    }
                }

                HandleDirtyMarking(targetObject, methodData.Attribute);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InspectorButton] Error invoking method '{methodData.Method.Name}' on '{targetObject.name}': {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void HandleDirtyMarking(UnityEngine.Object targetObject, InspectorButtonAttribute attribute)
        {
            if (attribute.MarkDirty)
            {
                EditorUtility.SetDirty(targetObject);
            }

            if (!string.IsNullOrEmpty(attribute.FieldToMarkDirty))
            {
                MarkFieldDirty(targetObject, attribute.FieldToMarkDirty);
            }
        }

        private void MarkFieldDirty(UnityEngine.Object targetObject, string fieldName)
        {
            try
            {
                var field = targetObject.GetType().GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field == null)
                {
                    Debug.LogWarning($"[InspectorButton] Field '{fieldName}' not found on '{targetObject.GetType().Name}'.");
                    return;
                }

                var fieldValue = field.GetValue(targetObject);

                if (fieldValue == null)
                    return;

                if (fieldValue is IEnumerable enumerable and not string)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is UnityEngine.Object unityObj && unityObj != null)
                        {
                            EditorUtility.SetDirty(unityObj);
                        }
                    }
                }
                else if (fieldValue is UnityEngine.Object singleUnityObj && singleUnityObj != null)
                {
                    EditorUtility.SetDirty(singleUnityObj);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InspectorButton] Error marking field '{fieldName}' dirty: {ex.Message}");
            }
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

            return char.ToUpper(result[0]) + result.Substring(1);
        }
    }
}
#endif
