using System;
using NanoVer.Visualisation.Node.Spline;
using NanoVer.Visualisation.Property;

namespace NanoVer.Visualisation.Properties.Collections
{
    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="SplineSegment" /> values.
    /// </summary>
    [Serializable]
    public class SplineArrayProperty : ArrayProperty<SplineSegment>
    {
    }
}