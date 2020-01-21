using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Components
{
    public interface IDynamicPropertyProvider
    {
        /// <summary>
        /// Get an existing property, or attempt to dynamically create one with the given
        /// type.
        /// </summary>
        IReadOnlyProperty<T> GetOrCreateProperty<T>(string name);
        
        /// <summary>
        /// Get a property which exists with the given name.
        /// </summary>
        IReadOnlyProperty GetProperty(string name);

    }
}