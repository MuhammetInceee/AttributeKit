using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// Comparison types for conditional display.
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>Equal to (==)</summary>
        Equals,
        /// <summary>Not equal to (!=)</summary>
        NotEquals,
        /// <summary>Greater than (>)</summary>
        GreaterThan,
        /// <summary>Less than (<)</summary>
        LessThan,
        /// <summary>Greater than or equal to (>=)</summary>
        GreaterOrEqual,
        /// <summary>Less than or equal to (<=)</summary>
        LessOrEqual
    }

    /// <summary>
    /// Attribute to conditionally display fields in the inspector based on other field/property values.
    /// Supports bool, int, float, enum, and string comparisons.
    /// </summary>
    /// <example>
    /// // Simple bool check
    /// [ConditionalDisplay("isEnabled")]
    /// public float value;
    ///
    /// // Inverse bool check
    /// [ConditionalDisplay("isEnabled", false)]
    /// public string disabledMessage;
    ///
    /// // Numeric comparison
    /// [ConditionalDisplay("health", ComparisonType.GreaterThan, 50)]
    /// public GameObject healthyEffect;
    ///
    /// // Enum comparison
    /// [ConditionalDisplay("weaponType", ComparisonType.Equals, WeaponType.Sword)]
    /// public float swordDamage;
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ConditionalDisplayAttribute : PropertyAttribute
    {
        /// <summary>
        /// Name of the field, property, or parameterless method to evaluate.
        /// </summary>
        public string ConditionName { get; }

        /// <summary>
        /// Type of comparison to perform.
        /// </summary>
        public ComparisonType Comparison { get; }

        /// <summary>
        /// Value to compare against.
        /// </summary>
        public object ComparisonValue { get; }

        /// <summary>
        /// Whether to invert the condition result.
        /// </summary>
        public bool Invert { get; }

        /// <summary>
        /// Creates a conditional display for bool values (simple version).
        /// </summary>
        /// <param name="conditionName">Name of the bool field/property/method.</param>
        /// <param name="expectedValue">Expected bool value (default: true).</param>
        public ConditionalDisplayAttribute(string conditionName, bool expectedValue = true)
        {
            ConditionName = conditionName;
            Comparison = ComparisonType.Equals;
            ComparisonValue = expectedValue;
            Invert = false;
        }

        /// <summary>
        /// Creates a conditional display with comparison type and value.
        /// </summary>
        /// <param name="conditionName">Name of the field/property/method to evaluate.</param>
        /// <param name="comparison">Type of comparison to perform.</param>
        /// <param name="value">Value to compare against.</param>
        public ConditionalDisplayAttribute(string conditionName, ComparisonType comparison, object value)
        {
            ConditionName = conditionName;
            Comparison = comparison;
            ComparisonValue = value;
            Invert = false;
        }

        /// <summary>
        /// Creates a conditional display with comparison type, value, and inversion option.
        /// </summary>
        /// <param name="conditionName">Name of the field/property/method to evaluate.</param>
        /// <param name="comparison">Type of comparison to perform.</param>
        /// <param name="value">Value to compare against.</param>
        /// <param name="invert">Whether to invert the result.</param>
        public ConditionalDisplayAttribute(string conditionName, ComparisonType comparison, object value, bool invert)
        {
            ConditionName = conditionName;
            Comparison = comparison;
            ComparisonValue = value;
            Invert = invert;
        }
    }
}
