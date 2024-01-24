using System;
using NanoVer.Core.Science;
using NanoVer.Visualisation.Property;

namespace NanoVer.Visualisation.Properties.Collections
{
    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="Element" />
    /// values.
    /// </summary>
    [Serializable]
    public class ElementArrayProperty : ArrayProperty<Element>
    {
    }
}