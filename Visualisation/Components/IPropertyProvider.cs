using System;
using System.Collections.Generic;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Node.Adaptor
{
    public interface IPropertyProvider
    {
        IEnumerable<(string name, Type type)> GetPotentialProperties();

        Property.Property GetProperty(string name);

        IEnumerable<(string name, Property.Property property)> GetProperties();

        IReadOnlyProperty<T> GetOrCreateProperty<T>(string name);
    }
}