using System.Collections.Generic;
using Narupa.Grpc.Multiplayer;

namespace Narupa.Grpc.Interactive
{
    /// <summary>
    /// A collection of interactions involved in iMD.
    /// </summary>
    public class Interactions : MultiplayerCollection<Interaction>
    {
        public Interactions(MultiplayerSession session) : base(session)
        {
        }

        /// <inheritdoc cref="MultiplayerCollection{TItem}.KeyPrefix"/>
        protected override string KeyPrefix => "interaction.";
        
        /// <inheritdoc cref="MultiplayerCollection{TItem}.ParseItem"/>
        protected override bool ParseItem(string key, object value, out Interaction parsed)
        {
            if (value is Dictionary<string, object> dict)
            {
                parsed = Serialization.Serialization.FromDataStructure<Interaction>(dict);
                return true;
            }

            parsed = default;
            return false;
        }
        
        /// <inheritdoc cref="MultiplayerCollection{TItem}.SerializeItem"/>
        protected override object SerializeItem(Interaction item)
        {
            return Serialization.Serialization.ToDataStructure(item);
        }
    }
}