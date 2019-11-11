// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// An object which can provide visualisation properties.
    /// </summary>
    public interface IPropertyProvider
    {
        /// <summary>
        /// Get a list of potential properties, some of which may already exist.
        /// </summary>
        /// <remarks>
        /// A returned item of this method indicates that
        /// <see cref="GetOrCreateProperty{T}" /> will be successful with the given name
        /// and type. However, that method may also support arbitary names/types
        /// depending on the implementation.
        /// </remarks>
        IEnumerable<(string name, Type type)> GetPotentialProperties();

        /// <summary>
        /// Get a property which exists with the given name.
        /// </summary>
        IReadOnlyProperty GetProperty(string name);

        /// <summary>
        /// Get all exisiting properties.
        /// </summary>
        IEnumerable<(string name, IReadOnlyProperty property)> GetProperties();

        /// <summary>
        /// Get an existing property, or attempt to dynamically create one with the given
        /// type.
        /// </summary>
        IReadOnlyProperty<T> GetOrCreateProperty<T>(string name);
    }
}