// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Base code for a Visualiser node which generates colors based upon atomic
    /// elements.
    /// </summary>
    [Serializable]
    public abstract class PerElementColor : VisualiserColor
    {
        private readonly ElementArrayProperty elements = new ElementArrayProperty();

        /// <summary>
        /// Atomic element array input.
        /// </summary>
        public IProperty<Element[]> Elements => elements;

        public override void Refresh()
        {
            if (elements.IsDirty && elements.HasValue)
            {
                RefreshColors();
                elements.IsDirty = false;
            }
        }

        private void RefreshColors()
        {
            var elementArray = elements.Value;
            var colorArray = colors.HasValue ? colors.Value : new UnityEngine.Color[0];
            Array.Resize(ref colorArray, elements.Value.Length);
            for (var i = 0; i < elements.Value.Length; i++)
                colorArray[i] = GetColor(elementArray[i]);

            colors.Value = colorArray;
        }

        /// <summary>
        /// Get the color for the given atomic element.
        /// </summary>
        protected abstract UnityEngine.Color GetColor(Element element);
    }
}