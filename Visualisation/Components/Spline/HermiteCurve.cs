using System;
using NanoVer.Visualisation.Node.Spline;
using UnityEngine;

namespace NanoVer.Visualisation.Components.Spline
{
    /// <inheritdoc cref="HermiteCurveNode" />
    public class HermiteCurve : VisualisationComponent<HermiteCurveNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}