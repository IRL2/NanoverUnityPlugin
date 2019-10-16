// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Narupa.Core;
using Narupa.Visualisation.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Narupa.Visualisation.Editor
{
    /// <summary>
    /// Property drawer override for drawing <see cref="Property.Property" /> fields
    /// which belong to a <see cref="VisualisationComponent" />.
    /// </summary>
    [InitializeOnLoad]
    public static class VisualiserComponentsPropertyDrawer
    {
        static VisualiserComponentsPropertyDrawer()
        {
            VisualisationPropertyDrawer.AddOverride(OnGUI);
        }

        private static bool OnGUI(ref Rect rect, SerializedProperty property, GUIContent label)
        {
            var collection = property.serializedObject.FindProperty("inputLinkCollection");

            if (collection == null) return false;

            var link = FindVisualisationLink(collection, property.name);

            var removeRect = new Rect(rect);
            removeRect.width = 40;
            removeRect.x = rect.xMax - removeRect.width;

            rect.width -= 44;

            var isLinked = link.HasValue;

            if (GUI.Button(removeRect, isLinked ? "Unlink" : "Link", EditorStyles.miniButton))
            {
                if (isLinked)
                {
                    collection.DeleteArrayElementAtIndex(link.Value.Index);
                    isLinked = false;
                }
                else
                {
                    var index = collection.arraySize;
                    collection.InsertArrayElementAtIndex(index);
                    var linkProperty = collection.GetArrayElementAtIndex(index);
                    var destProperty = linkProperty.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.destinationFieldName));
                    destProperty.stringValue = property.name;
                }
            }

            if (isLinked)
            {
                var linkRect = new Rect(rect);
                linkRect.yMin = linkRect.yMax - 16f;
                DrawSourceForLinkedProperty(linkRect, link.Value);
                rect.height -= 16;
            }
            else
            {
                return false;
            }

            return true;
        }

        private static SerializedInputLink? FindVisualisationLink(
            SerializedProperty linkCollectionSerProp,
            string name)
        {
            for (var i = 0; i < linkCollectionSerProp.arraySize; i++)
            {
                var currentLinkSerProp = linkCollectionSerProp.GetArrayElementAtIndex(i);
                var link = new SerializedInputLink
                {
                    Index = i,
                    DestinationProperty = currentLinkSerProp.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.destinationFieldName))
                };

                if (name != link.DestinationProperty.stringValue)
                    continue;

                link.SourceProperty =
                    currentLinkSerProp.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.sourceFieldName));
                link.SourceComponent =
                    currentLinkSerProp.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.sourceComponent));

                return link;
            }

            return null;
        }

        private struct SerializedInputLink
        {
            public int Index;

            public SerializedProperty SourceComponent;

            public SerializedProperty SourceProperty;

            public SerializedProperty DestinationProperty;
        }

        private static void DrawSourceForLinkedProperty(Rect rect, SerializedInputLink link)
        {
            var sourceFieldWidth = 96;
            var sourceComponentRect = new Rect(rect.x,
                                               rect.y,
                                               rect.width - sourceFieldWidth - 4,
                                               rect.height);
            var sourceFieldRect = new Rect(rect.xMax - sourceFieldWidth - 4,
                                           rect.y,
                                           sourceFieldWidth,
                                           rect.height);

            var destinationObject =
                GetVisualisationBaseObject(link.DestinationProperty.serializedObject.targetObject);

            var destinationValue = destinationObject
                                   .GetType()
                                   .GetFieldInSelfOrParents(link.DestinationProperty.stringValue,
                                                            BindingFlags.Instance
                                                          | BindingFlags.Public
                                                          | BindingFlags.NonPublic)
                                   .GetValue(destinationObject);

            var destinationType = GetPropertyType(destinationValue);

            EditorGUI.PropertyField(sourceComponentRect, link.SourceComponent, GUIContent.none);

            var srcObject = GetVisualisationBaseObject(link.SourceComponent.objectReferenceValue);

            if (srcObject != null)
            {
                var fields = srcObject.GetType()
                                      .GetFieldsInSelfOrParents(BindingFlags.Instance
                                                              | BindingFlags.NonPublic
                                                              | BindingFlags.Public)
                                      .Where(field => GetPropertyType(field.GetValue(srcObject)) ==
                                                      destinationType)
                                      .Select(field => field.Name)
                                      .ToArray();


                if (fields.Length > 0)
                {
                    var fieldNames = fields.Select(ObjectNames.NicifyVariableName).ToArray();

                    if (fields.Length == 1)
                    {
                        EditorGUI.LabelField(sourceFieldRect,
                                             fieldNames[0]);
                        if (link.SourceProperty.stringValue != fields[0])
                            link.SourceProperty.stringValue = fields[0];
                    }
                    else
                    {
                        var currentIndex = Array.IndexOf(fields, link.SourceProperty.stringValue);

                        var newIndex = EditorGUI.Popup(sourceFieldRect,
                                                       currentIndex,
                                                       fieldNames);

                        if (newIndex != currentIndex)
                            link.SourceProperty.stringValue = fields[newIndex];
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(link.SourceProperty.stringValue))
                        link.SourceProperty.stringValue = "";
                }
            }
        }

        [CanBeNull]
        private static Type GetPropertyType(object obj)
        {
            return obj is Property.Property property ? property.PropertyType : null;
        }

        private static object GetVisualisationBaseObject(Object src)
        {
            if (src is VisualisationComponent component)
                return component.GetWrappedVisualisationNode();
            return src;
        }
    }
}