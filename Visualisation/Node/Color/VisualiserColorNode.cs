// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using NanoVer.Visualisation.Properties.Collections;
using NanoVer.Visualisation.Property;
using UnityEngine;

namespace NanoVer.Visualisation.Node.Color
{
    /// <summary>
    /// Base code for a visualiser node which generates a set of colors.
    /// </summary>
    [Serializable]
    public abstract class VisualiserColorNode : GenericOutputNode
    {
        protected readonly ColorArrayProperty colors = new ColorArrayProperty();

        /// <summary>
        /// Color array output.
        /// </summary>
        public IReadOnlyProperty<UnityEngine.Color[]> Colors => colors;
    }
}