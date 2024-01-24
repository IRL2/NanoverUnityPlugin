using System;
using NanoVer.Visualisation.Node.Protein;
using NanoVer.Visualisation.Properties.Collections;

namespace NanoVer.Visualisation.Node.Input
{
    /// <summary>
    /// Input for the visualisation system that provides a <see cref="SecondaryStructureAssignment" /> value.
    /// </summary>
    [Serializable]
    public class SecondaryStructureArrayInputNode : InputNode<SecondaryStructureArrayProperty>
    {
    }
}