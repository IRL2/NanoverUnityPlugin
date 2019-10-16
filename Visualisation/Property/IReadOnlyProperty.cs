// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;

namespace Narupa.Visualisation.Property
{
    /// <remarks>
    /// Properties allow the linking together of inputs and outputs of various objects
    /// in an observer pattern. Read-only properties are value providers, whilst
    /// <see cref="Property" /> can also be linked to other properties to
    /// obtain their values.
    /// </remarks>
    public interface IReadOnlyProperty<out TValue>
    {
        /// <summary>
        /// The current value of the property.
        /// </summary>
        TValue Value { get; }

        /// <summary>
        /// Does the property currently have a value?
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// Callback for when the value is changed.
        /// </summary>
        event Action ValueChanged;
    }
}