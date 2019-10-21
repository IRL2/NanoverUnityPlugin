// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Math;
using Narupa.Frontend.Input;
using UnityEngine;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Component to indicate the point off the end of the controller where it is
    /// considered to be for use as a tool. Gizmos should be centered on this object,
    /// and added using <see cref="SetGizmo" />.
    /// </summary>
    public class ControllerCursor : MonoBehaviour, IPosedObject
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

        private GameObject currentGizmo;

        /// <summary>
        /// Set an object to be a gizmo that appears at the end of the tool. This will
        /// delete any existing gizmo.
        /// </summary>
        public void SetGizmo(GameObject gizmo)
        {
            if (currentGizmo != null)
                Destroy(currentGizmo);
            currentGizmo = gizmo;
        }
    }
}