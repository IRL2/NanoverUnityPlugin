using System;
using NanoVer.Core;
using NanoVer.Core.Science;
using NanoVer.Visualisation.Node.Color;
using NanoVer.Visualisation.Property;
using UnityEngine;

namespace NanoVer.Visualisation.Properties
{
    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="ElementColorMapping" />
    /// value.
    /// </summary>
    [Serializable]
    public class ElementColorMappingProperty : InterfaceProperty<IMapping<Element, Color>>
    {
    }
}