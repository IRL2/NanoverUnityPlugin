using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using Narupa.Grpc.Multiplayer;
using Narupa.Protocol.Imd;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Narupa.Grpc.Interactive
{
    public class Interactions : MultiplayerCollection<Interaction>
    {
        public Interactions(MultiplayerSession session) : base(session)
        {
        }

        protected override string KeyPrefix => "interaction.";
        
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

        protected override object SerializeItem(Interaction item)
        {
            return Serialization.Serialization.ToDataStructure(item);
        }
    }
}