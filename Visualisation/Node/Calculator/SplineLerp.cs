using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class SplineLerp
    {
        [SerializeField]
        private SplineArrayProperty inputSplines = new SplineArrayProperty();

        private SplineArrayProperty outputSplines = new SplineArrayProperty();

        private SplineSegment[] cachedSplines = new SplineSegment[0];

        public void Refresh()
        {
            if (inputSplines.HasValue)
            {
                var input = inputSplines.Value;
                if (cachedSplines.Length != input.Length)
                {
                    Array.Resize(ref cachedSplines, input.Length);
                    for (var i = 0; i < input.Length; i++)
                    {
                        cachedSplines[i] = input[i];
                    }
                }
                else
                {
                    for (var i = 0; i < input.Length; i++)
                    {
                        cachedSplines[i].StartPoint = input[i].StartPoint;
                        cachedSplines[i].EndPoint = input[i].EndPoint;
                        cachedSplines[i].StartNormal =
                            SmoothNormal(cachedSplines[i].StartNormal, input[i].StartNormal);
                        cachedSplines[i].EndNormal =
                            SmoothNormal(cachedSplines[i].EndNormal, input[i].EndNormal);
                        cachedSplines[i].StartTangent = input[i].StartTangent;
                        cachedSplines[i].EndTangent = input[i].EndTangent;
                        cachedSplines[i].StartColor =
                            SmoothColor(cachedSplines[i].StartColor, input[i].StartColor);
                        cachedSplines[i].EndColor =
                            SmoothColor(cachedSplines[i].EndColor, input[i].EndColor);
                        cachedSplines[i].StartScale =
                            SmoothScale(cachedSplines[i].StartScale, input[i].StartScale);
                        cachedSplines[i].EndScale =
                            SmoothScale(cachedSplines[i].EndScale, input[i].EndScale);
                    }
                }

                outputSplines.Value = cachedSplines;
            }
        }

        [SerializeField]
        private FloatProperty colorSpeed = new FloatProperty() { Value = 0.1f };

        [SerializeField]
        private FloatProperty scaleSpeed = new FloatProperty() { Value = 0.1f };

        [SerializeField]
        private FloatProperty normalSpeed = new FloatProperty() { Value = 0.1f };

        private UnityEngine.Color SmoothColor(UnityEngine.Color current, UnityEngine.Color next)
        {
            return Vector4.MoveTowards(current, next, colorSpeed.Value);
        }

        private Vector2 SmoothScale(Vector2 current, Vector2 next)
        {
            return Vector2.MoveTowards(current, next, scaleSpeed.Value);
        }

        private Vector3 SmoothNormal(Vector3 current, Vector3 next)
        {
            return Vector3.MoveTowards(current, next, normalSpeed.Value);
        }
    }
}