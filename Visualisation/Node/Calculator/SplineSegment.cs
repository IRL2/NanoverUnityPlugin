using System;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public struct SplineSegment
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;
        public Vector3 StartTangent;
        public Vector3 EndTangent;
        public Vector3 StartNormal;
        public Vector3 EndNormal;

        public UnityEngine.Color StartColor;
        public UnityEngine.Color EndColor;
        public Vector3 StartScale;
        public Vector3 EndScale;
    }
}