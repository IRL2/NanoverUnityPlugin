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
    public class ControllerCursor : ControllerPivot
    {
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