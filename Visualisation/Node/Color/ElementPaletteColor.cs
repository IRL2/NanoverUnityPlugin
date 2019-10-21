// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Visualiser node that generates atomic colors from atomic positions, based
    /// upon an <see cref="ElementColorMapping" />
    /// </summary>
    [Serializable]
    public class ElementPaletteColor : PerElementColor
    {
        [SerializeField]
        private ElementColorMappingProperty mapping = new ElementColorMappingProperty();

        public IProperty<ElementColorMapping> Mapping => mapping;

        protected override bool IsInputDirty => base.IsInputDirty || mapping.IsDirty;

        protected override bool IsInputValid => base.IsInputValid && mapping.HasNonNullValue();

        protected override void ClearDirty()
        {
            base.ClearDirty();
            mapping.IsDirty = false;
        }

        /// <inheritdoc cref="PerElementColor.GetColor" />
        protected override UnityEngine.Color GetColor(Element element)
        {
            return mapping.HasNonNullValue()
                       ? mapping.Value.GetColor(element)
                       : UnityEngine.Color.white;
        }
    }
}