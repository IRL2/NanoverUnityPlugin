using System;
using NanoVer.Visualisation.Node.Protein;
using NanoVer.Visualisation.Property;

namespace NanoVer.Visualisation.Properties.Collections
{
    /// <summary>
    /// Serializable <see cref="Property" /> for an array of <see cref="SecondaryStructureAssignment" /> values.
    /// </summary>
    [Serializable]
    public class SecondaryStructureArrayProperty : ArrayProperty<SecondaryStructureAssignment>
    {
    }
}