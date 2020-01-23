// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Components
{
    /// <summary>
    /// An object which can provide visualisation properties.
    /// </summary>
    public interface IPropertyProvider
    {
        /// <summary>
        /// Get a property which exists with the given name. Returns null if the property with a given name is null.
        /// </summary>
        IReadOnlyProperty GetProperty(string name);

        /// <summary>
        /// Get all properties of this object.
        /// </summary>
        IEnumerable<(string name, IReadOnlyProperty property)> GetProperties();
    }
}