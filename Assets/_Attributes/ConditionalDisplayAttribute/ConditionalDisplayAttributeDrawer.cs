#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AttributeKit.Editor
{
    /// <summary>
    /// Custom property drawer for ConditionalDisplayAttribute.
    /// Conditionally shows/hides fields based on other field values with various comparison operators.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConditionalDisplayAttribute))]
    public class ConditionalDisplayAttributeDrawer : PropertyDrawer
    {
        private struct MemberCache
        {
            public MemberInfo Member;
            public Type ValueType;
        }

        private static readonly Dictionary<string, MemberCache> _memberCache = new Dictionary<string, MemberCache>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldDisplay(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldDisplay(property)
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : -EditorGUIUtility.standardVerticalSpacing; // Negative spacing to remove gap
        }

        private bool ShouldDisplay(SerializedProperty property)
        {
            var attribute = this.attribute as ConditionalDisplayAttribute;
            if (attribute == null)
                return true;

            try
            {
                object conditionValue = GetConditionValue(property, attribute.ConditionName);
                if (conditionValue == null)
                {
                    // Condition not found, default to showing to prevent data loss
                    return true;
                }

                bool result = EvaluateCondition(conditionValue, attribute.Comparison, attribute.ComparisonValue);
                return attribute.Invert ? !result : result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ConditionalDisplay] Error evaluating condition '{attribute.ConditionName}': {ex.Message}");
                return true; // Default to showing on error
            }
        }

        private object GetConditionValue(SerializedProperty property, string conditionName)
        {
            // Try SerializedProperty first (fastest)
            var conditionProperty = FindRelativeProperty(property, conditionName);
            if (conditionProperty != null)
            {
                return GetSerializedPropertyValue(conditionProperty);
            }

            // Fallback to reflection for properties and methods
            object target = property.serializedObject.targetObject;
            Type targetType = target.GetType();
            string cacheKey = $"{targetType.FullName}.{conditionName}";

            if (!_memberCache.TryGetValue(cacheKey, out var cached))
            {
                var member = targetType.GetMember(conditionName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();

                if (member == null)
                {
                    Debug.LogWarning($"[ConditionalDisplay] Condition '{conditionName}' not found in {targetType.Name}.");
                    return null;
                }

                Type valueType = member switch
                {
                    FieldInfo field => field.FieldType,
                    PropertyInfo prop => prop.PropertyType,
                    MethodInfo method => method.ReturnType,
                    _ => null
                };

                cached = new MemberCache { Member = member, ValueType = valueType };
                _memberCache[cacheKey] = cached;
            }

            return cached.Member switch
            {
                FieldInfo field => field.GetValue(target),
                PropertyInfo prop => prop.GetValue(target),
                MethodInfo method when method.GetParameters().Length == 0 => method.Invoke(target, null),
                _ => null
            };
        }

        private SerializedProperty FindRelativeProperty(SerializedProperty property, string propertyName)
        {
            // Handle nested properties
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');

            if (lastDot >= 0)
            {
                // Try relative path first
                string relativePath = path.Substring(0, lastDot + 1) + propertyName;
                var relative = property.serializedObject.FindProperty(relativePath);
                if (relative != null)
                    return relative;
            }

            // Try root level
            return property.serializedObject.FindProperty(propertyName);
        }

        private object GetSerializedPropertyValue(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Boolean => property.boolValue,
                SerializedPropertyType.Integer => property.intValue,
                SerializedPropertyType.Float => property.floatValue,
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.Enum => property.enumValueIndex,
                SerializedPropertyType.ObjectReference => property.objectReferenceValue,
                _ => null
            };
        }

        private bool EvaluateCondition(object currentValue, ComparisonType comparison, object comparisonValue)
        {
            if (currentValue == null && comparisonValue == null)
                return comparison == ComparisonType.Equals;

            if (currentValue == null || comparisonValue == null)
                return comparison == ComparisonType.NotEquals;

            // Handle enum comparisons
            if (currentValue.GetType().IsEnum || comparisonValue.GetType().IsEnum)
            {
                return EvaluateEnumCondition(currentValue, comparison, comparisonValue);
            }

            // Handle bool comparisons
            if (currentValue is bool boolValue && comparisonValue is bool boolComparison)
            {
                return comparison switch
                {
                    ComparisonType.Equals => boolValue == boolComparison,
                    ComparisonType.NotEquals => boolValue != boolComparison,
                    _ => false
                };
            }

            // Handle numeric comparisons
            if (IsNumeric(currentValue) && IsNumeric(comparisonValue))
            {
                return EvaluateNumericCondition(
                    Convert.ToDouble(currentValue),
                    comparison,
                    Convert.ToDouble(comparisonValue));
            }

            // Handle string comparisons
            if (currentValue is string strValue && comparisonValue is string strComparison)
            {
                return EvaluateStringCondition(strValue, comparison, strComparison);
            }

            // Handle object reference comparisons
            if (currentValue is UnityEngine.Object unityObj && comparisonValue is UnityEngine.Object unityComparison)
            {
                return comparison switch
                {
                    ComparisonType.Equals => unityObj == unityComparison,
                    ComparisonType.NotEquals => unityObj != unityComparison,
                    _ => false
                };
            }

            // Fallback: use Equals
            bool equals = currentValue.Equals(comparisonValue);
            return comparison == ComparisonType.Equals ? equals : !equals;
        }

        private bool EvaluateEnumCondition(object currentValue, ComparisonType comparison, object comparisonValue)
        {
            int currentInt = currentValue is int intVal ? intVal : Convert.ToInt32(currentValue);
            int comparisonInt = comparisonValue is int compInt ? compInt : Convert.ToInt32(comparisonValue);

            return comparison switch
            {
                ComparisonType.Equals => currentInt == comparisonInt,
                ComparisonType.NotEquals => currentInt != comparisonInt,
                ComparisonType.GreaterThan => currentInt > comparisonInt,
                ComparisonType.LessThan => currentInt < comparisonInt,
                ComparisonType.GreaterOrEqual => currentInt >= comparisonInt,
                ComparisonType.LessOrEqual => currentInt <= comparisonInt,
                _ => false
            };
        }

        private bool EvaluateNumericCondition(double currentValue, ComparisonType comparison, double comparisonValue)
        {
            return comparison switch
            {
                ComparisonType.Equals => Math.Abs(currentValue - comparisonValue) < double.Epsilon,
                ComparisonType.NotEquals => Math.Abs(currentValue - comparisonValue) >= double.Epsilon,
                ComparisonType.GreaterThan => currentValue > comparisonValue,
                ComparisonType.LessThan => currentValue < comparisonValue,
                ComparisonType.GreaterOrEqual => currentValue >= comparisonValue,
                ComparisonType.LessOrEqual => currentValue <= comparisonValue,
                _ => false
            };
        }

        private bool EvaluateStringCondition(string currentValue, ComparisonType comparison, string comparisonValue)
        {
            return comparison switch
            {
                ComparisonType.Equals => currentValue == comparisonValue,
                ComparisonType.NotEquals => currentValue != comparisonValue,
                _ => false
            };
        }

        private bool IsNumeric(object value)
        {
            return value is sbyte or byte or short or ushort or int or uint
                   or long or ulong or float or double or decimal;
        }
    }
}
#endif