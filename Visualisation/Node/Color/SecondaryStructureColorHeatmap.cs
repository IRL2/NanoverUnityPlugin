using System;
using Nanover.Visualisation.Properties.Collections;
using Nanover.Visualisation.Property;
using UnityEngine;

namespace Nanover.Visualisation.Node.Color
{
    /// <summary>
    /// Colours protein residues using a heat-map according to some abstract metric.
    ///
    /// This will search the graph for a float array output node named "residue.normalised_metric_colour",
    /// which stores a normalised metric value for each residue in the protein. These metric values
    /// are then used in conjunction with a colour gradient object to determine the colour of each
    /// residue. Note, it is imperative that the order of elements within the normalised metric
    /// array matches the order in which residues are stored in the graph. This is normally not an
    /// issue as the cartoon graphs always render all residues and, so far, maintain residue order.
    /// </summary>
    [Serializable]
    public class SecondaryStructureColorHeatmap: VisualiserColorNode
    {
        /// <summary>
        /// Array of normalised metric values for the residues.
        ///
        /// These abstract normalised metric values are used in conjunction with a heat-map to define
        /// the colour for each of the residues in the protein. There should be one, and only one,
        /// value for each and every residue. Such values should be normalised so that they roughly
        /// span the domain [0, 1]. The metric values within this array should be ordered so that
        /// they mach up with the order in which their associated carbon-alpha atoms appear in the
        /// base structure.
        /// </summary>
        [SerializeField]
        private FloatArrayProperty residueNormalisedMetricColour;


        /// <summary>
        /// The gradient used for the heat-map.
        /// </summary>
        /// <remarks>
        /// This will default to Viridis.
        /// </remarks>
        [SerializeField]
        private Gradient gradient = CreateDefaultGradient();


        /// <summary>
        /// Returns the default Viridis heat-map gradient.
        /// </summary>
        /// <returns>
        /// Viridis colour gradient.
        /// </returns>
        private static Gradient CreateDefaultGradient()
        {
            return new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(new UnityEngine.Color(0.99f, 0.91f, 0.15f, 1f), 0.00f),
                    new GradientColorKey(new UnityEngine.Color(0.37f, 0.79f, 0.38f, 1f), 0.25f),
                    new GradientColorKey(new UnityEngine.Color(0.13f, 0.57f, 0.55f, 1f), 0.50f),
                    new GradientColorKey(new UnityEngine.Color(0.23f, 0.32f, 0.55f, 1f), 0.75f),
                    new GradientColorKey(new UnityEngine.Color(0.27f, 0.00f, 0.33f, 1f), 1.00f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            };
        }

        protected override bool IsInputValid => residueNormalisedMetricColour.HasNonNullValue();
        protected override bool IsInputDirty => residueNormalisedMetricColour.IsDirty;
        protected override void ClearDirty() => residueNormalisedMetricColour.IsDirty = false;
        protected override void ClearOutput() => colors.UndefineValue();
        
        protected override void UpdateOutput()
        {
            var metrics = residueNormalisedMetricColour.Value;
            var colorArray = colors.HasValue ? colors.Value : Array.Empty<UnityEngine.Color>();
            Array.Resize(ref colorArray, metrics.Length);

            for (var i = 0; i < metrics.Length; i++)
                colorArray[i] = gradient.Evaluate(metrics[i]);

            colors.Value = colorArray;
        }
    }
}