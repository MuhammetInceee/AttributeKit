#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AttributeKit.Editor
{
    /// <summary>
    /// Custom property drawer for TimeDecomposerAttribute.
    /// Displays time value as separate fields for months, days, hours, minutes, and seconds.
    /// </summary>
    [CustomPropertyDrawer(typeof(TimeDecomposerAttribute))]
    public class TimeDecomposerAttributeDrawer : PropertyDrawer
    {
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 3600;
        private const int SecondsPerDay = 86400; // 24 * 3600
        private const int SecondsPerMonth = 2592000; // 30 * 24 * 3600

        private struct TimeUnitInfo
        {
            public TimeUnit Unit;
            public string ShortLabel;
            public string LongLabel;
            public int SecondsPerUnit;
            public int Value;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Validate property type
            if (property.propertyType != SerializedPropertyType.Float &&
                property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use [TimeDecomposer] with float, double, or int.");
                return;
            }

            var attribute = this.attribute as TimeDecomposerAttribute;
            if (attribute == null)
                return;

            EditorGUI.BeginProperty(position, label, property);

            // Get total seconds from property
            double totalSeconds = GetTotalSeconds(property);
            totalSeconds = Math.Max(0, totalSeconds);

            // Calculate time units
            var timeUnits = CalculateTimeUnits(attribute.Units, totalSeconds);

            // Draw fields
            DrawTimeFields(position, label, timeUnits, out var newTotalSeconds);

            // Update property value
            SetTotalSeconds(property, newTotalSeconds);

            EditorGUI.EndProperty();
        }

        private double GetTotalSeconds(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Float => property.floatValue,
                SerializedPropertyType.Integer => property.intValue,
                _ => 0
            };
        }

        private void SetTotalSeconds(SerializedProperty property, double totalSeconds)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = (float)totalSeconds;
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = (int)Math.Round(totalSeconds);
                    break;
            }
        }

        private List<TimeUnitInfo> CalculateTimeUnits(TimeUnit units, double totalSeconds)
        {
            var timeUnits = new List<TimeUnitInfo>();
            int remainingSeconds = (int)Math.Round(totalSeconds);

            // Process in order: Month -> Day -> Hour -> Minute -> Second
            if (units.HasFlag(TimeUnit.Month))
            {
                int months = remainingSeconds / SecondsPerMonth;
                remainingSeconds %= SecondsPerMonth;
                timeUnits.Add(new TimeUnitInfo
                {
                    Unit = TimeUnit.Month,
                    ShortLabel = "Mo",
                    LongLabel = "Months",
                    SecondsPerUnit = SecondsPerMonth,
                    Value = months
                });
            }

            if (units.HasFlag(TimeUnit.Day))
            {
                int days = remainingSeconds / SecondsPerDay;
                remainingSeconds %= SecondsPerDay;
                timeUnits.Add(new TimeUnitInfo
                {
                    Unit = TimeUnit.Day,
                    ShortLabel = "D",
                    LongLabel = "Days",
                    SecondsPerUnit = SecondsPerDay,
                    Value = days
                });
            }

            if (units.HasFlag(TimeUnit.Hour))
            {
                int hours = remainingSeconds / SecondsPerHour;
                remainingSeconds %= SecondsPerHour;
                timeUnits.Add(new TimeUnitInfo
                {
                    Unit = TimeUnit.Hour,
                    ShortLabel = "Hr",
                    LongLabel = "Hours",
                    SecondsPerUnit = SecondsPerHour,
                    Value = hours
                });
            }

            if (units.HasFlag(TimeUnit.Minute))
            {
                int minutes = remainingSeconds / SecondsPerMinute;
                remainingSeconds %= SecondsPerMinute;
                timeUnits.Add(new TimeUnitInfo
                {
                    Unit = TimeUnit.Minute,
                    ShortLabel = "Min",
                    LongLabel = "Minutes",
                    SecondsPerUnit = SecondsPerMinute,
                    Value = minutes
                });
            }

            if (units.HasFlag(TimeUnit.Second))
            {
                timeUnits.Add(new TimeUnitInfo
                {
                    Unit = TimeUnit.Second,
                    ShortLabel = "Sec",
                    LongLabel = "Seconds",
                    SecondsPerUnit = 1,
                    Value = remainingSeconds
                });
            }

            return timeUnits;
        }

        private void DrawTimeFields(Rect position, GUIContent label, List<TimeUnitInfo> timeUnits, out double newTotalSeconds)
        {
            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            float lineHeight = EditorGUIUtility.singleLineHeight;

            int unitCount = timeUnits.Count;
            if (unitCount == 0)
            {
                EditorGUI.LabelField(contentRect, "No time units specified");
                newTotalSeconds = 0;
                return;
            }

            // Calculate layout
            float gap = 4f;
            float labelSpacing = 2f;
            float unitLabelWidth = 50f;
            float availableWidth = contentRect.width;
            float totalGapWidth = gap * (unitCount - 1);
            float totalLabelWidth = unitLabelWidth * unitCount;
            float totalFieldWidth = availableWidth - totalGapWidth - totalLabelWidth - (labelSpacing * unitCount);
            float fieldWidth = totalFieldWidth / unitCount;

            // Adjust for narrow inspectors
            if (contentRect.width < 300f)
            {
                unitLabelWidth = 30f;
                totalLabelWidth = unitLabelWidth * unitCount;
                totalFieldWidth = availableWidth - totalGapWidth - totalLabelWidth - (labelSpacing * unitCount);
                fieldWidth = totalFieldWidth / unitCount;
            }

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw fields
            float currentX = contentRect.x;
            var newValues = new int[unitCount];

            for (int i = 0; i < unitCount; i++)
            {
                var unit = timeUnits[i];

                Rect fieldRect = new Rect(currentX, contentRect.y, fieldWidth, lineHeight);
                Rect labelRect = new Rect(fieldRect.xMax + labelSpacing, contentRect.y, unitLabelWidth, lineHeight);

                string labelText = contentRect.width < 300f ? unit.ShortLabel : unit.LongLabel;

                newValues[i] = EditorGUI.IntField(fieldRect, unit.Value);
                EditorGUI.LabelField(labelRect, labelText);

                currentX = labelRect.xMax + gap;
            }

            EditorGUI.indentLevel = oldIndent;

            // Handle overflow and underflow
            NormalizeValues(timeUnits, newValues);

            // Calculate new total seconds
            newTotalSeconds = 0;
            for (int i = 0; i < unitCount; i++)
            {
                newTotalSeconds += newValues[i] * timeUnits[i].SecondsPerUnit;
            }

            newTotalSeconds = Math.Max(0, newTotalSeconds);
        }

        private void NormalizeValues(List<TimeUnitInfo> timeUnits, int[] values)
        {
            // Process from smallest to largest unit
            for (int i = values.Length - 1; i >= 0; i--)
            {
                if (values[i] < 0)
                {
                    // Borrow from next larger unit
                    if (i > 0)
                    {
                        int multiplier = timeUnits[i].SecondsPerUnit / timeUnits[i - 1].SecondsPerUnit;
                        if (multiplier > 0)
                        {
                            int borrow = (-values[i] + multiplier - 1) / multiplier;
                            values[i - 1] -= borrow;
                            values[i] += borrow * multiplier;
                        }
                    }
                    else
                    {
                        // First unit, clamp to 0
                        values[i] = 0;
                    }
                }

                if (i < values.Length - 1 && values[i] > 0)
                {
                    // Check if we need to carry to next larger unit
                    int nextMultiplier = timeUnits[i + 1].SecondsPerUnit / timeUnits[i].SecondsPerUnit;
                    if (nextMultiplier > 0 && values[i + 1] >= nextMultiplier)
                    {
                        values[i] += values[i + 1] / nextMultiplier;
                        values[i + 1] %= nextMultiplier;
                    }
                }
            }

            // Final clamp to ensure all values are non-negative
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < 0)
                {
                    // Set all to 0 if we still have negative values
                    for (int j = 0; j < values.Length; j++)
                        values[j] = 0;
                    break;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif