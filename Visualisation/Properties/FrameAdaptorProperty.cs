using System;
using NanoVer.Visualisation.Components;
using NanoVer.Visualisation.Components.Adaptor;
using NanoVer.Visualisation.Property;

namespace NanoVer.Visualisation.Properties
{
    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="IDynamicPropertyProvider" /> value.
    /// </summary>
    [Serializable]
    public class FrameAdaptorProperty : InterfaceProperty<IDynamicPropertyProvider>
    {
    }
}