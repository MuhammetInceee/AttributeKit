#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AttributeKit.Editor
{
    /// <summary>
    /// Custom property drawer for ExpandableAttribute.
    /// Displays ScriptableObject properties inline in the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
        private static readonly Dictionary<string, UnityEditor.Editor> _editorCache = new Dictionary<string, UnityEditor.Editor>();

        private const float HeaderHeight = 22f;
        private const float ButtonWidth = 60f;
        private const float ButtonSpacing = 2f;
        private const float Padding = 2f;
        private const float DividerHeight = 1f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Validate property type
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [Expandable] with ScriptableObject fields only.");
                return;
            }

            var attribute = this.attribute as ExpandableAttribute;
            if (attribute == null)
                return;

            EditorGUI.BeginProperty(position, label, property);

            var targetObject = property.objectReferenceValue;
            string propertyPath = property.propertyPath;

            // Initialize foldout state
            if (!_foldoutStates.ContainsKey(propertyPath))
            {
                _foldoutStates[propertyPath] = attribute.IsExpandedByDefault;
            }

            float currentY = position.y;

            // Draw header with object field and buttons
            currentY = DrawHeader(position, property, label, attribute, targetObject, propertyPath, currentY);

            // Draw expanded content if object exists and is expanded
            if (targetObject != null && _foldoutStates[propertyPath])
            {
                currentY = DrawExpandedContent(position, property, attribute, targetObject, propertyPath, currentY);
            }

            // Draw divider
            if (attribute.DrawDivider && targetObject != null)
            {
                DrawDivider(position, currentY);
            }

            EditorGUI.EndProperty();
        }

        private float DrawHeader(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            ExpandableAttribute attribute,
            UnityEngine.Object targetObject,
            string propertyPath,
            float yPosition)
        {
            Rect headerRect = new Rect(position.x, yPosition, position.width, HeaderHeight);

            // Draw header background based on style
            DrawHeaderBackground(headerRect, attribute, targetObject);

            // Calculate button area
            float buttonsWidth = 0f;
            int buttonCount = 0;

            if (targetObject == null && attribute.ShowCreateButton)
            {
                buttonsWidth += ButtonWidth;
                buttonCount++;
            }
            else if (targetObject != null)
            {
                if (attribute.ShowDeleteButton)
                {
                    buttonsWidth += ButtonWidth + ButtonSpacing;
                    buttonCount++;
                }
            }

            // Object field rect
            float objectFieldWidth = position.width - buttonsWidth - (buttonCount > 0 ? 5f : 0f);
            Rect objectFieldRect = new Rect(position.x, yPosition, objectFieldWidth, EditorGUIUtility.singleLineHeight);

            // Get the correct field type
            Type fieldType = GetFieldType();

            // Draw foldout and object field
            if (targetObject != null)
            {
                Rect foldoutRect = new Rect(objectFieldRect.x, objectFieldRect.y, 15f, objectFieldRect.height);
                Rect adjustedFieldRect = new Rect(objectFieldRect.x + 15f, objectFieldRect.y, objectFieldRect.width - 15f, objectFieldRect.height);

                _foldoutStates[propertyPath] = EditorGUI.Foldout(foldoutRect, _foldoutStates[propertyPath], GUIContent.none, true);

                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.ObjectField(adjustedFieldRect, label, targetObject, fieldType, false);
                if (EditorGUI.EndChangeCheck())
                {
                    property.objectReferenceValue = newValue;
                    CleanupEditor(propertyPath);
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.ObjectField(objectFieldRect, label, targetObject, fieldType, false);
                if (EditorGUI.EndChangeCheck())
                {
                    property.objectReferenceValue = newValue;
                }
            }

            // Draw buttons
            float buttonX = objectFieldRect.xMax + 5f;

            if (targetObject == null && attribute.ShowCreateButton)
            {
                Rect createButtonRect = new Rect(buttonX, yPosition, ButtonWidth, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(createButtonRect, new GUIContent("Create", "Create new ScriptableObject asset")))
                {
                    CreateScriptableObject(property);
                }
            }
            else if (targetObject != null && attribute.ShowDeleteButton)
            {
                Rect deleteButtonRect = new Rect(buttonX, yPosition, ButtonWidth, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(deleteButtonRect, new GUIContent("Delete", "Delete ScriptableObject asset")))
                {
                    DeleteScriptableObject(property, propertyPath);
                }
            }

            return yPosition + HeaderHeight + Padding;
        }

        private void DrawHeaderBackground(Rect rect, ExpandableAttribute attribute, UnityEngine.Object targetObject)
        {
            if (targetObject == null)
                return;

            Color backgroundColor = Color.clear;

            switch (attribute.HeaderStyle)
            {
                case ExpandableHeaderStyle.Box:
                    backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(0.2f, 0.2f, 0.2f, 0.5f)
                        : new Color(0.8f, 0.8f, 0.8f, 0.5f);
                    break;

                case ExpandableHeaderStyle.Foldout:
                    backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(0.25f, 0.25f, 0.25f, 1f)
                        : new Color(0.75f, 0.75f, 0.75f, 1f);
                    break;
            }

            // Use custom color if specified
            if (attribute.HeaderColor != Color.clear)
            {
                backgroundColor = attribute.HeaderColor;
            }

            if (backgroundColor != Color.clear)
            {
                EditorGUI.DrawRect(rect, backgroundColor);
            }
        }

        private float DrawExpandedContent(
            Rect position,
            SerializedProperty property,
            ExpandableAttribute attribute,
            UnityEngine.Object targetObject,
            string propertyPath,
            float yPosition)
        {
            // Create box background for content
            float contentHeight = GetExpandedContentHeight(property, targetObject, propertyPath);
            Rect contentRect = new Rect(position.x, yPosition, position.width, contentHeight);

            // Draw background box
            Color boxColor = EditorGUIUtility.isProSkin
                ? new Color(0.18f, 0.18f, 0.18f, 1f)
                : new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUI.DrawRect(contentRect, boxColor);

            // Draw border
            DrawBorder(contentRect);

            // Draw properties
            yPosition += Padding;

            var editor = GetOrCreateEditor(targetObject, propertyPath);
            if (editor != null)
            {
                var serializedObject = editor.serializedObject;
                serializedObject.Update();

                EditorGUI.indentLevel++;

                SerializedProperty prop = serializedObject.GetIterator();
                bool enterChildren = true;

                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    // Skip script reference
                    if (prop.name == "m_Script")
                        continue;

                    float propertyHeight = EditorGUI.GetPropertyHeight(prop, true);
                    Rect propertyRect = new Rect(
                        position.x + 5f,
                        yPosition,
                        position.width - 10f,
                        propertyHeight);

                    EditorGUI.PropertyField(propertyRect, prop, true);
                    yPosition += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel--;

                serializedObject.ApplyModifiedProperties();
            }

            return yPosition + Padding;
        }

        private void DrawBorder(Rect rect)
        {
            Color borderColor = EditorGUIUtility.isProSkin
                ? new Color(0.1f, 0.1f, 0.1f, 1f)
                : new Color(0.6f, 0.6f, 0.6f, 1f);

            // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), borderColor);
            // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), borderColor);
            // Left
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), borderColor);
            // Right
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), borderColor);
        }

        private void DrawDivider(Rect position, float yPosition)
        {
            Color dividerColor = EditorGUIUtility.isProSkin
                ? new Color(0.1f, 0.1f, 0.1f, 1f)
                : new Color(0.5f, 0.5f, 0.5f, 1f);

            Rect dividerRect = new Rect(position.x, yPosition, position.width, DividerHeight);
            EditorGUI.DrawRect(dividerRect, dividerColor);
        }

        private float GetExpandedContentHeight(SerializedProperty property, UnityEngine.Object targetObject, string propertyPath)
        {
            if (targetObject == null)
                return 0f;

            float totalHeight = Padding * 2;

            var editor = GetOrCreateEditor(targetObject, propertyPath);
            if (editor != null)
            {
                var serializedObject = editor.serializedObject;
                serializedObject.Update();

                SerializedProperty prop = serializedObject.GetIterator();
                bool enterChildren = true;

                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if (prop.name == "m_Script")
                        continue;

                    totalHeight += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return totalHeight;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attribute = this.attribute as ExpandableAttribute;
            if (attribute == null)
                return EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return EditorGUIUtility.singleLineHeight;

            float totalHeight = HeaderHeight + Padding;

            var targetObject = property.objectReferenceValue;
            string propertyPath = property.propertyPath;

            if (targetObject != null && _foldoutStates.ContainsKey(propertyPath) && _foldoutStates[propertyPath])
            {
                totalHeight += GetExpandedContentHeight(property, targetObject, propertyPath);
            }

            if (attribute.DrawDivider && targetObject != null)
            {
                totalHeight += DividerHeight + Padding;
            }

            return totalHeight;
        }

        private UnityEditor.Editor GetOrCreateEditor(UnityEngine.Object targetObject, string propertyPath)
        {
            if (targetObject == null)
            {
                CleanupEditor(propertyPath);
                return null;
            }

            if (_editorCache.TryGetValue(propertyPath, out var cachedEditor))
            {
                if (cachedEditor != null && cachedEditor.target == targetObject)
                    return cachedEditor;

                CleanupEditor(propertyPath);
            }

            var editor = UnityEditor.Editor.CreateEditor(targetObject);
            _editorCache[propertyPath] = editor;
            return editor;
        }

        private void CleanupEditor(string propertyPath)
        {
            if (_editorCache.TryGetValue(propertyPath, out var editor))
            {
                if (editor != null)
                {
                    UnityEngine.Object.DestroyImmediate(editor);
                }
                _editorCache.Remove(propertyPath);
            }
        }

        private Type GetFieldType()
        {
            if (fieldInfo == null)
                return typeof(ScriptableObject);

            Type fieldType = fieldInfo.FieldType;

            // Handle arrays
            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }
            // Handle lists
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            // Validate it's a ScriptableObject type
            if (fieldType != null && typeof(ScriptableObject).IsAssignableFrom(fieldType))
            {
                return fieldType;
            }

            // Fallback to generic ScriptableObject
            return typeof(ScriptableObject);
        }

        private void CreateScriptableObject(SerializedProperty property)
        {
            try
            {
                // Get the field type using the helper method
                Type fieldType = GetFieldType();

                // Validate it's a ScriptableObject
                if (fieldType == typeof(ScriptableObject))
                {
                    Debug.LogError("[Expandable] Field type must be a specific ScriptableObject type, not the base class.");
                    return;
                }

                if (!typeof(ScriptableObject).IsAssignableFrom(fieldType))
                {
                    Debug.LogError("[Expandable] Field type must be ScriptableObject or derived from it.");
                    return;
                }

                // Create instance
                var instance = ScriptableObject.CreateInstance(fieldType);
                if (instance == null)
                {
                    Debug.LogError($"[Expandable] Failed to create instance of {fieldType.Name}.");
                    return;
                }

                // Generate file path
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save ScriptableObject",
                    $"New{fieldType.Name}",
                    "asset",
                    $"Create a new {fieldType.Name} asset");

                if (string.IsNullOrEmpty(path))
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                    return;
                }

                // Create asset
                AssetDatabase.CreateAsset(instance, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Assign to property
                property.objectReferenceValue = instance;
                property.serializedObject.ApplyModifiedProperties();

                EditorGUIUtility.PingObject(instance);
                Debug.Log($"[Expandable] Created new {fieldType.Name} at {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Expandable] Error creating ScriptableObject: {ex.Message}");
            }
        }

        private void DeleteScriptableObject(SerializedProperty property, string propertyPath)
        {
            var targetObject = property.objectReferenceValue;
            if (targetObject == null)
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete ScriptableObject",
                $"Are you sure you want to delete '{targetObject.name}'?\n\nThis action cannot be undone.",
                "Delete",
                "Cancel");

            if (confirmed)
            {
                string assetPath = AssetDatabase.GetAssetPath(targetObject);

                // Clear reference first
                property.objectReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();

                // Cleanup editor cache
                CleanupEditor(propertyPath);

                // Delete asset
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"[Expandable] Deleted asset at {assetPath}");
                }
            }
        }
    }
}
#endif
