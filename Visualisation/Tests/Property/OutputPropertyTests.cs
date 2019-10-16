// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Tests.Property
{
    internal class IntPropertyTests : PropertyTests<IntProperty, int>
    {
        protected override int ExampleNonNullValue => 1;
        protected override int DifferentNonNullValue => -4;
    }

    internal class FloatPropertyTests : PropertyTests<FloatProperty, float>
    {
        protected override float ExampleNonNullValue => 1.1f;
        protected override float DifferentNonNullValue => -22013.3223f;
    }

    internal class Vector3PropertyTests : PropertyTests<Vector3Property, Vector3>
    {
        protected override Vector3 ExampleNonNullValue => Vector3.zero;
        protected override Vector3 DifferentNonNullValue => Vector3.left * 2f;
    }

    internal class ColorPropertyTests : PropertyTests<ColorProperty, Color>
    {
        protected override Color ExampleNonNullValue => Color.red;
        protected override Color DifferentNonNullValue => Color.black;
    }
}