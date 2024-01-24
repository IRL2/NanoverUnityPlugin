using System;
using NanoVer.Frame;
using NanoVer.Visualisation.Property;

namespace NanoVer.Visualisation.Properties.Collections
{
    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="BondPair" />
    /// values.
    /// </summary>
    [Serializable]
    public class BondArrayProperty : ArrayProperty<BondPair>
    {
    }
}