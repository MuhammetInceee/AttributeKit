using System;
using UnityEngine;

namespace AttributeKit
{
    /// <summary>
    /// Time units that can be displayed in the inspector.
    /// </summary>
    [Flags]
    public enum TimeUnit
    {
        None = 0,
        Second = 1 << 0,
        Minute = 1 << 1,
        Hour = 1 << 2,
        Day = 1 << 3,
        Month = 1 << 4
    }

    /// <summary>
    /// Attribute to decompose a time value (float or double) into separate time units in the inspector.
    /// The value is stored in seconds internally.
    /// </summary>
    /// <example>
    /// [TimeDecomposer] // Default: Minutes and Seconds
    /// public float cooldownTime;
    ///
    /// [TimeDecomposer(TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second)]
    /// public float eventDuration;
    ///
    /// [TimeDecomposer(TimeUnit.Month | TimeUnit.Day | TimeUnit.Hour)]
    /// public double totalPlayTime;
    /// </example>
    public class TimeDecomposerAttribute : PropertyAttribute
    {
        /// <summary>
        /// The time units to display in the inspector.
        /// </summary>
        public TimeUnit Units { get; }

        /// <summary>
        /// Creates a TimeDecomposer attribute with default units (Minute and Second).
        /// </summary>
        public TimeDecomposerAttribute()
        {
            Units = TimeUnit.Minute | TimeUnit.Second;
        }

        /// <summary>
        /// Creates a TimeDecomposer attribute with specified time units.
        /// </summary>
        /// <param name="units">The time units to display (can be combined with | operator).</param>
        public TimeDecomposerAttribute(TimeUnit units)
        {
            // If no units specified, use default
            Units = units == TimeUnit.None ? TimeUnit.Minute | TimeUnit.Second : units;
        }
    }
}
