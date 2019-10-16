// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Core.Math;
using UnityEngine;

namespace Narupa.Frontend.Manipulation
{
    /// <summary>
    /// Gesture that ties an object's position and orientation to a control
    /// point's  position and orientation. Updating the control point
    /// transformation yields an updated object transformation such that the
    /// object has moved to preserve the relative position and orientation of
    /// the object and control point.
    /// Example usage: A single hand grabbing a box, the box then moves and
    /// rotates with the motion of the hand.
    /// </summary>
    public sealed class OnePointTranslateRotateGesture
    {
        private Matrix4x4 controlPointToObjectMatrix;

        /// <summary>
        /// Begin the gesture with an initial object transform and initial control
        /// point transform. Throughout the gesture the relationship between these
        /// transforms will remain invariant.
        /// </summary>
        public void BeginGesture(Transformation objectTransformation,
                                 Transformation controlTransformation)
        {
            var objectMatrix = objectTransformation.Matrix;
            var controlMatrix = controlTransformation.Matrix;

            controlPointToObjectMatrix = controlMatrix.GetTransformationTo(objectMatrix);
        }

        /// <summary>
        /// Return an object transform that is the initial object transform
        /// altered in the same manner as the gesture transformation since the
        /// gesture began.
        /// The relative coordinates of the gesture within the object space
        /// remains invariant.
        /// </summary>
        public Transformation UpdateControlPoint(Transformation controlTransformation)
        {
            var controlMatrix = controlTransformation.Matrix;
            var objectMatrix = controlMatrix.TransformedBy(controlPointToObjectMatrix);

            return Transformation.FromMatrix(objectMatrix);
        }
    }

    /// <summary>
    /// Gesture that ties an object's position, orientation, and scale to the
    /// positions of two control points. Updating the control point matrices yields
    /// an updated object matrix such that the object has moved to preserve the
    /// local positions of the control points within the object.
    /// Example usage: Two hands grab corners of a box, and as the hands move
    /// the box stretches and pivots with the motion of the hands.
    /// </summary>
    public sealed class TwoPointTranslateRotateScaleGesture
    {
        private readonly OnePointTranslateRotateGesture onePointGesture =
            new OnePointTranslateRotateGesture();

        /// <summary>
        /// Begin the gesture with an initial object transformation and initial
        /// transformations for the two control points. Throughout the gesture
        /// the positions of the control points within object space will remain
        /// invariant.
        /// </summary>
        public void BeginGesture(Transformation objectTransformation,
                                 Transformation controlTransformation1,
                                 Transformation controlTransformation2)
        {
            var midpointTransformation = ComputeMidpointTransformation(controlTransformation1,
                                                                       controlTransformation2);
            onePointGesture.BeginGesture(objectTransformation, midpointTransformation);
        }

        /// <summary>
        /// Return an updated object transformation to account for the changes
        /// in the control point transformations.
        /// </summary>
        public Transformation UpdateControlPoints(Transformation controlTransformation1,
                                                  Transformation controlTransformation2)
        {
            var midpointTransformation = ComputeMidpointTransformation(controlTransformation1,
                                                                       controlTransformation2);
            var objectTransformation = onePointGesture.UpdateControlPoint(midpointTransformation);

            return objectTransformation;
        }

        /// <summary>
        /// Compute a transformation that represents the "average" of two input
        /// transformations.
        /// </summary>
        private static Transformation ComputeMidpointTransformation(
            Transformation controlTransformation1,
            Transformation controlTransformation2)
        {
            // position the origin between the two control points
            var position1 = controlTransformation1.Position;
            var position2 = controlTransformation2.Position;
            var position = Vector3.LerpUnclamped(position1, position2, 0.5f);

            // base the scale on the separation between the two control points
            var scale = Vector3.Distance(position1, position2);

            // choose an orientation where the line between the two control points
            // is one axis, and another axis is roughly the compromise between the
            // two up vectors of the control points
            var rotation1 = controlTransformation1.Rotation;
            var rotation2 = controlTransformation2.Rotation;
            var midRotation = Quaternion.SlerpUnclamped(rotation1, rotation2, 0.5f);

            var up = midRotation * Vector3.forward;
            var right = (position2 - position1).normalized;

            var rotation = Quaternion.LookRotation(right, up);

            return new Transformation(position, rotation, scale * Vector3.one);
        }
    }
}