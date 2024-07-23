using System;
using Nanover.Visualisation.Properties;
using Nanover.Visualisation.Properties.Collections;
using Nanover.Visualisation.Property;
using UnityEngine;

namespace Nanover.Visualisation.Node.Color
{
    /// <summary>
    /// Colours protein residues using a heat-map according to some abstract metric.
    ///
    /// This will take the a float array output node which stores which stores a normalised metric
    /// value for each residue in the protein. These metric values are then used in conjunction with
    /// a colour gradient object to determine the colour of each residue.
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
        /// Residue index values.
        /// </summary>
        /// <remarks>
        /// This specifies the indices of the residues to which each normalised metric value is
        /// associated. Commonly this is just an array over the range [0, n], however this is
        /// included to help deal with cases where the order of the normalised metric values might
        /// not correspond to the order in which the residues appear in the structure. 
        /// </remarks>
        [SerializeField]
        private IntArrayProperty residueIndices;
        
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

        /// <summary>
        /// Returns a boolean indicating the validity of the inputs.
        /// </summary>
        protected override bool IsInputValid =>
            residueNormalisedMetricColour.HasNonNullValue() &&
            residueIndices.HasNonNullValue() &&
            residueNormalisedMetricColour.Value.Length == residueIndices.Value.Length;

        /// <summary>
        /// Returns a boolean indicating if the input fields have updated.
        /// </summary>
        protected override bool IsInputDirty => residueNormalisedMetricColour.IsDirty || residueIndices.IsDirty;

        /// <summary>
        /// Clear <c>IsDirtry</c> flag of the attached input fields. 
        /// </summary>
        protected override void ClearDirty()
        {
            residueNormalisedMetricColour.IsDirty = false;
            residueIndices.IsDirty = false;
        }

        /// <summary>
        /// Purge output fields.
        /// </summary>
        protected override void ClearOutput() => colors.UndefineValue();

        /// <summary>
        /// Update output fields.
        /// </summary>
        protected override void UpdateOutput()
        {
            // Retrieve the residue id and metric values.
            var residues = this.residueIndices.Value;
            var metrics = residueNormalisedMetricColour.Value;

            // Ensure that the output array is allocated.
            var colorArray = colors.HasValue ? colors.Value : Array.Empty<UnityEngine.Color>();

            // Verify that the output array is of anticipated length.
            if (colorArray.Length != metrics.Length)
                Array.Resize(ref colorArray, metrics.Length);

            // Linearly interpolate the normalised metric values over the colour gradient.
            for (var i = 0; i < metrics.Length; i++)
                colorArray[i] = gradient.Evaluate(metrics[residues[i]]);

            // Assign the results to the output array.
            colors.Value = colorArray;
        }
    }
}