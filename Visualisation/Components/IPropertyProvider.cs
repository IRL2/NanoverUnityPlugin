// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Narupa.Visualisation.Components
{
    /// <summary>
    /// Indicates a class exposes raw <see cref="Property.Property" /> fields for use
    /// with the Visualisation system.
    /// </summary>
    public interface IPropertyProvider
    {
        /// <summary>
        /// Get a list of name <see cref="Property.Property" /> pairs.
        /// </summary>
        IEnumerable<(string name, Property.Property value)> GetProperties();

        /// <summary>
        /// Get the property with the given name.
        /// </summary>
        Property.Property GetProperty(string name);
    }
}