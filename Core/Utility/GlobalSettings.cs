using System;

namespace Nanover.Core.Utility
{
    public static class GlobalSettings
    {
        /// <summary>
        /// Represents a globally unique identifier (GUID) for the current application instance.
        /// </summary>
        /// <remarks>
        /// This identifier is generated once at the start of the application and remains constant
        /// throughout the application's lifecycle. It can be used for logging, tracking instances
        /// across distributed systems, generating keys for shared resources in a way that allows
        /// them to be uniquely tied to a specific user, and any scenario where a unique identifier
        /// for the application run is required. The ID is read-only and generated via a call to
        /// <see cref="Guid.NewGuid"/>.
        /// </remarks>
        /// <example>
        /// How to use the ApplicationGUID:
        /// <code>
        /// var currentAppId = GlobalSettings.ApplicationGUID;
        /// Console.WriteLine($"Current Application ID: {currentAppId}");
        /// </code>
        /// </example>1
        public static readonly Guid ApplicationGUID = Guid.NewGuid();
    }
}