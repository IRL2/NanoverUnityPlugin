using System;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Applies an oscillating color change to a subset of particles.
    /// </summary>
    [Serializable]
    public class ColorPulserNode
    {
        [SerializeField]
        private ColorArrayProperty inputColors = new ColorArrayProperty();

        private ColorArrayProperty outputColors = new ColorArrayProperty();

        [SerializeField]
        private IntArrayProperty highlightFilter = new IntArrayProperty();

        [SerializeField]
        private float speed = 6f;

        [SerializeField]
        private float maximum = 1f;

        [SerializeField]
        private float minimum = 0f;

        private bool isFilterNonZero = false;
        private bool areInputColorsValid = false;

        public bool IsInputDirty => inputColors.IsDirty
                                 || highlightFilter.IsDirty;

        private float strength = 1f;
        private float targetStrength = 1f;

        private float darkenAmount = 0.8f;

        public void Refresh()
        {
            if (IsInputDirty)
            {
                isFilterNonZero = highlightFilter.HasNonEmptyValue();
                areInputColorsValid = inputColors.HasNonNullValue();

                if (areInputColorsValid)
                {
                    var arr = new UnityEngine.Color[inputColors.Value.Length];
                    outputColors.Value = arr;
                    Array.Copy(inputColors.Value, outputColors.Value, inputColors.Value.Length);
                    outputColors.MarkValueAsChanged();
                }
                else
                {
                    outputColors.UndefineValue();
                }

                highlightFilter.IsDirty = false;
                inputColors.IsDirty = false;
            }

            targetStrength = isFilterNonZero ? 1 : 0;

            if (areInputColorsValid && (strength != targetStrength || isFilterNonZero))
            {
                var darken = Mathf.Lerp(1f, darkenAmount, targetStrength);
                var intensity = Mathf.Lerp(minimum, maximum,
                                           targetStrength *
                                           (0.5f + 0.5f * Mathf.Sin(Time.deltaTime)));

                for (var i = 0; i < inputColors.Value.Length; i++)
                    outputColors.Value[i] *= darken;

                if (isFilterNonZero)
                {
                    var filter = highlightFilter.Value;
                    foreach (var i in filter)
                    {
                        outputColors.Value[i] += intensity * UnityEngine.Color.white;
                    }
                }

                outputColors.MarkValueAsChanged();
            }
        }
    }
}