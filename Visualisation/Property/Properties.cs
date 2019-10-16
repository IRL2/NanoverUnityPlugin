// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Visualisation.Node.Color;
using UnityEngine;

namespace Narupa.Visualisation.Property
{
    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="Material" /> value.
    /// </summary>
    [Serializable]
    public class MaterialProperty : Property<Material>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="Mesh" /> value.
    /// </summary>
    [Serializable]
    public class MeshProperty : Property<Mesh>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="ElementColorMapping" />
    /// value.
    /// </summary>
    [Serializable]
    public class ElementColorMappingProperty : Property<ElementColorMapping>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="Color" /> value.
    /// </summary>
    [Serializable]
    public class ColorProperty : Property<Color>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="string" /> value.
    /// </summary>
    [Serializable]
    public class StringProperty : Property<string>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="Color" />
    /// values.
    /// </summary>
    [Serializable]
    public class ColorArrayProperty : ArrayProperty<Color>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="Element" />
    /// values.
    /// </summary>
    [Serializable]
    public class ElementArrayProperty : ArrayProperty<Element>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="string" />
    /// values.
    /// </summary>
    [Serializable]
    public class StringArrayProperty : ArrayProperty<string>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="float" />
    /// values.
    /// </summary>
    [Serializable]
    public class FloatArrayProperty : ArrayProperty<float>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="float" /> value.
    /// </summary>
    [Serializable]
    public class FloatProperty : Property<float>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="float" /> value.
    /// </summary>
    [Serializable]
    public class BoolProperty : Property<bool>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="Vector3" /> value.
    /// </summary>
    [Serializable]
    public class Vector3Property : Property<Vector3>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an <see cref="int" /> value.
    /// </summary>
    [Serializable]
    public class IntProperty : Property<int>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="int" /> values.
    /// </summary>
    [Serializable]
    public class IntArrayProperty : ArrayProperty<int>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="Gradient" /> value.
    /// </summary>
    [Serializable]
    public class GradientProperty : Property<Gradient>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="Vector3" />
    /// values.
    /// </summary>
    [Serializable]
    public class Vector3ArrayProperty : ArrayProperty<Vector3>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="BondPair" />
    /// values.
    /// </summary>
    [Serializable]
    public class BondArrayProperty : ArrayProperty<BondPair>
    {
    }

    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="TValue" />
    /// values;
    /// </summary>
    [Serializable]
    public abstract class ArrayProperty<TValue> : Property<TValue[]>
    {
    }
}