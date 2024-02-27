// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Nanover.Visualisation.Properties.Collections;
using Nanover.Visualisation.Property;
using UnityEngine;

namespace Nanover.Visualisation.Node.Scale
{
    /// <summary>
    /// Base code for visualiser node which generates a set of scales.
    /// </summary>
    [Serializable]
    public abstract class VisualiserScaleNode : GenericOutputNode
    {
        protected readonly FloatArrayProperty scales = new FloatArrayProperty();

        /// <summary>
        /// Scale array output.
        /// </summary>
        public IReadOnlyProperty<float[]> Scales => scales;
    }
}