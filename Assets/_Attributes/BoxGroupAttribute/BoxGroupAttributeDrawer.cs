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
    /// Custom editor for MonoBehaviour to handle BoxGroup attributes.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class BoxGroupMonoBehaviourEditor : BoxGroupBaseEditor
    {
    }

    /// <summary>
    /// Custom editor for ScriptableObject to handle BoxGroup attributes.
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class BoxGroupScriptableObjectEditor : BoxGroupBaseEditor
    {
    }

    /// <summary>
    /// Base editor class that handles BoxGroup attribute rendering.
    /// </summary>
    public abstract class BoxGroupBaseEditor : UnityEditor.Editor
    {
        private class FieldGroup
        {
            public BoxGroupAttribute Attribute;
            public List<SerializedProperty> Properties = new List<SerializedProperty>();
            public List<FieldInfo> Fields = new List<FieldInfo>();
        }

        private Dictionary<string, FieldGroup> _groups;
        private List<SerializedProperty> _ungroupedProperties;
        private static Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

        private const float BoxPadding = 4f;
        private const float BoxMargin = 2f;
        private const float HeaderHeight = 22f;

        private void OnEnable()
        {
            CacheFieldGroups();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw script field for MonoBehaviour
            if (target is MonoBehaviour)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
                GUI.enabled = true;
            }
            else if (target is ScriptableObject)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((ScriptableObject)target), typeof(MonoScript), false);
                GUI.enabled = true;
            }

            // Draw grouped fields
            if (_groups != null && _groups.Count > 0)
            {
                var sortedGroups = _groups.Values.OrderBy(g => g.Attribute.Order).ToList();

                foreach (var group in sortedGroups)
                {
                    DrawBoxGroup(group);
                }
            }

            // Draw ungrouped fields
            if (_ungroupedProperties != null)
            {
                foreach (var property in _ungroupedProperties)
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CacheFieldGroups()
        {
            _groups = new Dictionary<string, FieldGroup>();
            _ungroupedProperties = new List<SerializedProperty>();

            var targetType = target.GetType();
            var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                .ToList();

            foreach (var field in fields)
            {
                var boxGroupAttr = field.GetCustomAttribute<BoxGroupAttribute>();

                if (boxGroupAttr != null)
                {
                    if (!_groups.ContainsKey(boxGroupAttr.GroupId))
                    {
                        _groups[boxGroupAttr.GroupId] = new FieldGroup
                        {
                            Attribute = boxGroupAttr
                        };
                    }

                    var property = serializedObject.FindProperty(field.Name);
                    if (property != null)
                    {
                        _groups[boxGroupAttr.GroupId].Properties.Add(property.Copy());
                        _groups[boxGroupAttr.GroupId].Fields.Add(field);
                    }
                }
                else
                {
                    // Skip m_Script field
                    if (field.Name == "m_Script")
                        continue;

                    var property = serializedObject.FindProperty(field.Name);
                    if (property != null)
                    {
                        _ungroupedProperties.Add(property.Copy());
                    }
                }
            }
        }

        private void DrawBoxGroup(FieldGroup group)
        {
            if (group.Properties.Count == 0)
                return;

            var attribute = group.Attribute;
            string foldoutKey = $"{target.GetInstanceID()}_{attribute.GroupId}";

            // Initialize foldout state
            if (!_foldoutStates.ContainsKey(foldoutKey))
            {
                _foldoutStates[foldoutKey] = attribute.ExpandedByDefault;
            }

            EditorGUILayout.Space(BoxMargin);

            // Draw box background
            Rect boxRect = EditorGUILayout.BeginVertical();
            DrawBoxBackground(boxRect, attribute);

            GUILayout.Space(BoxPadding);

            // Draw header with foldout if applicable
            if (attribute.ShowLabel)
            {
                if (attribute.Foldable)
                {
                    DrawFoldoutHeader(attribute, foldoutKey);
                }
                else
                {
                    DrawStaticHeader(attribute);
                }
            }

            // Draw properties if expanded
            bool isExpanded = !attribute.Foldable || _foldoutStates[foldoutKey];

            if (isExpanded)
            {
                EditorGUI.indentLevel++;

                foreach (var property in group.Properties)
                {
                    EditorGUILayout.PropertyField(property, true);
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(BoxPadding);

            EditorGUILayout.EndVertical();

            // Draw border
            DrawBoxBorder(boxRect, attribute);

            EditorGUILayout.Space(BoxMargin);
        }

        private void DrawBoxBackground(Rect rect, BoxGroupAttribute attribute)
        {
            if (rect.width <= 1 || rect.height <= 1)
                return;

            Color backgroundColor = GetBackgroundColor(attribute);

            switch (attribute.Style)
            {
                case BoxStyle.Default:
                case BoxStyle.Rounded:
                    EditorGUI.DrawRect(rect, backgroundColor);
                    break;

                case BoxStyle.Flat:
                    Color flatColor = backgroundColor;
                    flatColor.a *= 0.3f;
                    EditorGUI.DrawRect(rect, flatColor);
                    break;

                case BoxStyle.HelpBox:
                    // HelpBox style uses Unity's built-in style
                    GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
                    break;
            }
        }

        private void DrawBoxBorder(Rect rect, BoxGroupAttribute attribute)
        {
            if (rect.width <= 1 || rect.height <= 1)
                return;

            if (attribute.Style == BoxStyle.Flat || attribute.Style == BoxStyle.HelpBox)
                return;

            Color borderColor = EditorGUIUtility.isProSkin
                ? new Color(0.15f, 0.15f, 0.15f, 1f)
                : new Color(0.5f, 0.5f, 0.5f, 1f);

            float borderWidth = 1f;

            // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, borderWidth), borderColor);
            // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - borderWidth, rect.width, borderWidth), borderColor);
            // Left
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, borderWidth, rect.height), borderColor);
            // Right
            EditorGUI.DrawRect(new Rect(rect.xMax - borderWidth, rect.y, borderWidth, rect.height), borderColor);
        }

        private void DrawFoldoutHeader(BoxGroupAttribute attribute, string foldoutKey)
        {
            Color headerColor = GetHeaderColor(attribute);

            if (headerColor != Color.clear)
            {
                Rect headerRect = EditorGUILayout.GetControlRect(false, HeaderHeight);
                EditorGUI.DrawRect(headerRect, headerColor);

                _foldoutStates[foldoutKey] = EditorGUI.Foldout(
                    headerRect,
                    _foldoutStates[foldoutKey],
                    attribute.GroupTitle,
                    true,
                    EditorStyles.foldoutHeader);
            }
            else
            {
                _foldoutStates[foldoutKey] = EditorGUILayout.Foldout(
                    _foldoutStates[foldoutKey],
                    attribute.GroupTitle,
                    true,
                    EditorStyles.foldoutHeader);
            }
        }

        private void DrawStaticHeader(BoxGroupAttribute attribute)
        {
            Color headerColor = GetHeaderColor(attribute);

            if (headerColor != Color.clear)
            {
                Rect headerRect = EditorGUILayout.GetControlRect(false, HeaderHeight);
                EditorGUI.DrawRect(headerRect, headerColor);

                GUIStyle boldStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(15, 0, 0, 0)
                };

                EditorGUI.LabelField(headerRect, attribute.GroupTitle, boldStyle);
            }
            else
            {
                EditorGUILayout.LabelField(attribute.GroupTitle, EditorStyles.boldLabel);
            }
        }

        private Color GetBackgroundColor(BoxGroupAttribute attribute)
        {
            if (attribute.BackgroundColor != Color.clear)
                return attribute.BackgroundColor;

            return EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f, 1f)
                : new Color(0.8f, 0.8f, 0.8f, 1f);
        }

        private Color GetHeaderColor(BoxGroupAttribute attribute)
        {
            if (attribute.HeaderColor != Color.clear)
                return attribute.HeaderColor;

            if (!attribute.Foldable)
                return Color.clear;

            return EditorGUIUtility.isProSkin
                ? new Color(0.25f, 0.25f, 0.25f, 1f)
                : new Color(0.75f, 0.75f, 0.75f, 1f);
        }
    }
}
#endif
