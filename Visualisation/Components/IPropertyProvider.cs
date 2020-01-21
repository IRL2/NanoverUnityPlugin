// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Components
{
    /// <summary>
    /// An object which can provide visualisation properties.
    /// </summary>
    public interface IPropertyProvider : IDynamicPropertyProvider
    {
        /// <summary>
        /// Get a list of potential properties, some of which may already exist.
        /// </summary>
        /// <remarks>
        /// A returned item of this method indicates that
        /// <see cref="IDynamicPropertyProvider.GetOrCreateProperty{T}" /> will be successful with the given name
        /// and type. However, that method may also support arbitary names/types
        /// depending on the implementation.
        /// </remarks>
        IEnumerable<(string name, Type type)> GetPotentialProperties();
        
        /// <summary>
        /// Get all exisiting properties.
        /// </summary>
        IEnumerable<(string name, IReadOnlyProperty property)> GetProperties();

        /// <summary>
        /// Could this provider give a property on a call to
        /// <see cref="IDynamicPropertyProvider.GetOrCreateProperty{T}" />?.
        /// </summary>
        bool CanProvideProperty<T>(string name);
    }
}