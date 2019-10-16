// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Scale
{
    /// <summary>
    /// Base code for visualiser node which generates a set of scales.
    /// </summary>
    [Serializable]
    public abstract class VisualiserScale
    {
        protected readonly FloatArrayProperty scales = new FloatArrayProperty();

        /// <summary>
        /// Scale array output.
        /// </summary>
        public IReadOnlyProperty<float[]> Scales => scales;

        /// <summary>
        /// Refresh the scales calculated by this node.
        /// </summary>
        public abstract void Refresh();
    }
}