// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Math;
using Narupa.Frontend.Input;

namespace Narupa.Frontend.Manipulation
{
    /// <summary>
    /// A function that attempts to start a manipulation given an manipulator pose.
    /// </summary>
    public delegate IActiveManipulation ManipulationAttemptHandler(UnitScaleTransformation manipulatorPose);

    /// <summary>
    /// Represents an input device posed in 3D space (e.g a VR controller) that can
    /// engage in a single manipulation.
    /// </summary>
    public class Manipulator : IPosedObject
    {
        public Transformation? Pose => posedObject.Pose;
        public event Action PoseChanged;

        private readonly IPosedObject posedObject;
        private IActiveManipulation activeManipulation;

        public Manipulator(IPosedObject posedObject)
        {
            this.posedObject = posedObject;

            posedObject.PoseChanged += UpdatePose;
        }

        /// <summary>
        /// Add listeners such that when the given button is pressed, this manipulator
        /// will attempt to begin a manipulation using the given handler, and when
        /// released the active manipulation will be ended.
        /// </summary>
        public void BindButtonToManipulation(IButton button,
                                             ManipulationAttemptHandler manipulationAttempt)
        {
            button.Pressed += () => AttemptManipulation(manipulationAttempt);
            button.Released += EndActiveManipulation;
        }

        /// <summary>
        /// Set the active manipulation, ending any existing manipulation first.
        /// </summary>
        public void SetActiveManipulation(IActiveManipulation manipulation)
        {
            EndActiveManipulation();

            activeManipulation = manipulation;
        }

        /// <summary>
        /// End the active manipulation if there is any.
        /// </summary>
        public void EndActiveManipulation()
        {
            activeManipulation?.EndManipulation();
            activeManipulation = null;
        }

        private void UpdatePose()
        {
            if (posedObject.Pose is Transformation pose)
            {
                activeManipulation?.UpdateManipulatorPose(pose.AsUnitTransformWithoutScale());
            }
            else
            {
                EndActiveManipulation();
            }

            PoseChanged?.Invoke();
        }

        private void AttemptManipulation(ManipulationAttemptHandler manipulationAttempt)
        {
            if (Pose is Transformation pose
             && manipulationAttempt(pose.AsUnitTransformWithoutScale()) is IActiveManipulation manipulation)
            {
                SetActiveManipulation(manipulation);
            }
        }
    }
}