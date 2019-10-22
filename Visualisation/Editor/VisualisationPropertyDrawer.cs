// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Narupa.Core;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Property;
using UnityEditor;
using UnityEngine;

namespace Narupa.Visualisation.Editor
{
    /// <summary>
    /// Draws a <see cref="Property" /> in the Editor.
    /// </summary>
    [CustomPropertyDrawer(typeof(Property.Property), true)]
    public sealed class VisualisationPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Callback for overriding this property drawer. Returns true if this specific
        /// property was overriden.
        /// </summary>
        public delegate bool OnGuiOverride(ref Rect position,
                                           SerializedProperty property,
                                           GUIContent label);

        private static readonly List<OnGuiOverride> overrides = new List<OnGuiOverride>();

        /// <summary>
        /// Add an override for drawing properties, for example to allow linking in the
        /// Editor for Visualisation Components
        /// </summary>
        public static void AddOverride(OnGuiOverride guiOverride)
        {
            overrides.Add(guiOverride);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Prefix the name of the property
            position = EditorGUI.PrefixLabel(position, label);

            // Check if any overrides apply here
            if (overrides.Any(overrideOnGui => overrideOnGui(ref position, property, label)))
            {
                return;
            }

            var valueProperty = GetValueProperty(property);

            if (valueProperty.isArray && valueProperty.type != "string")
            {
                EditorGUI.HelpBox(position, "Set array input from within code.", MessageType.None);
            }
            else
            {
                var valueRect = position;

                var isProvidedSerializedProperty = GetIsProvidedProperty(property);

                valueRect.xMin -= 15f;
                var togglePosition = new Rect(valueRect);
                valueRect.xMin += 20f;

                var isValueProvided = isProvidedSerializedProperty.boolValue;

                if (isValueProvided)
                {
                    DrawValueGui(property, valueRect);
                }
                else
                {
                    DrawMissingValueGui(property, valueRect);
                }

                EditorGUI.PropertyField(togglePosition, isProvidedSerializedProperty,
                                        GUIContent.none);
            }
        }

        /// <summary>
        /// Draw a box indicating a missing value, indicating if the requirement is
        /// required
        /// </summary>
        private void DrawMissingValueGui(SerializedProperty property, Rect rect)
        {
            var field = GetField(property);
            if (field?.GetCustomAttribute<RequiredPropertyAttribute>() != null)
            {
                EditorGUI.HelpBox(rect, "Missing input!", MessageType.Error);
            }
            else
            {
                EditorGUI.HelpBox(rect, "No input", MessageType.None);
            }
        }

        /// <summary>
        /// Draw the value of the property in the given rect
        /// </summary>
        private void DrawValueGui(SerializedProperty property, Rect rect)
        {
            var valueSerializedProperty = GetValueProperty(property);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, valueSerializedProperty, GUIContent.none, true);
            if (EditorGUI.EndChangeCheck())
            {
                var obj = GetVisualisationBaseObject(property.serializedObject.targetObject);

                var field = obj.GetType()
                               .GetFieldInSelfOrParents(property.name, BindingFlags.Instance
                                                                     | BindingFlags.Public
                                                                     | BindingFlags.NonPublic)
                               ?.GetValue(obj) as Property.Property;
                if (field != null)
                    field.IsDirty = true;
            }
        }
        
        private static object GetVisualisationBaseObject(Object src)
        {
            if (src is VisualisationComponent component)
                return component.GetWrappedVisualisationNode();
            return src;
        }

        /// <summary>
        /// For a given <see cref="SerializedProperty" />, get the underlying
        /// <see cref="FieldInfo" />.
        /// </summary>
        private static FieldInfo GetField(SerializedProperty property)
        {
            // TODO: Support nested objects
            return property.serializedObject
                           .targetObject
                           .GetType()
                           .GetFieldInSelfOrParents(property.propertyPath,
                                                    BindingFlags.Public
                                                  | BindingFlags.Instance
                                                  | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty" /> representing the internal input
        /// value.
        /// </summary>
        private SerializedProperty GetValueProperty(SerializedProperty inputProperty)
        {
            return inputProperty.FindPropertyRelative("value");
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty" /> representing the internal input
        /// value.
        /// </summary>
        private SerializedProperty GetIsProvidedProperty(SerializedProperty inputProperty)
        {
            return inputProperty.FindPropertyRelative("isValueProvided");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var provided = GetIsProvidedProperty(property);
            if (provided.boolValue)
            {
                return EditorGUI.GetPropertyHeight(GetValueProperty(property));
            }
            else
            {
                return base.GetPropertyHeight(property, label);
            }
        }
    }
}