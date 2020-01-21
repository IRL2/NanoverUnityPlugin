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
        
        /// <summary>
        /// Attempt to set the value without knowing the types involved.
        /// </summary>
        void TrySetValue(object value);

        /// <summary>
        /// Attempt to set the linked property without knowing the types involved.
        /// </summary>
        void TrySetLinkedProperty(object property);
        
        IReadOnlyProperty LinkedProperty { get; }
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
        new IReadOnlyProperty<TValue> LinkedProperty { get; set; }
    }
}