// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Math;
using Narupa.Frontend.Input;
using UnityEngine;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Component to indicate a part of the controller with a position and radius.
    /// </summary>
    public class ControllerPivot : MonoBehaviour, IPosedObject
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
            Gizmos.DrawSphere(transform.position, 0.01f);
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                PoseChanged?.Invoke();
                transform.hasChanged = false;
            }
        }

        /// <inheritdoc cref="IPosedObject.Pose" />
        public Transformation? Pose => Transformation.FromTransform(transform);

        /// <inheritdoc cref="IPosedObject.PoseChanged" />
        public event Action PoseChanged;
    }
}