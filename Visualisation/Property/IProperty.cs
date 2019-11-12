// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

namespace Narupa.Visualisation.Property
{
    /// <summary>
    /// Typeless version of <see cref="IProperty{TValue}"/>
    /// </summary>
    public interface IProperty : IReadOnlyProperty
    {
        /// <summary>
        /// Remove the value from this property.
        /// </summary>
        void UndefineValue();

        /// <summary>
        /// Is this property dirty?
        /// </summary>
        bool IsDirty { get; set; }
        
        /// <summary>
        /// Is this property linked to another?
        /// </summary>
        bool HasLinkedProperty { get; }
    }
    
    /// <summary>
    /// Extension of an <see cref="IReadOnlyProperty{TValue}" /> which can have its
    /// value altered.
    /// </summary>
    public interface IProperty<TValue> : IReadOnlyProperty<TValue>, IProperty
    {
        /// <inheritdoc cref="IReadOnlyProperty{TValue}.Value" />
        new TValue Value { get; set; }

        /// <summary>
        /// Linked property that will override this value.
        /// </summary>
        IReadOnlyProperty<TValue> LinkedProperty { get; set; }
    }
}