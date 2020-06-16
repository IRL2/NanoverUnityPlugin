using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Part of the <see cref="MultiplayerSession"/> who is in charge of managing multiplayer
    /// avatars.
    /// </summary>
    public class MultiplayerAvatars : MultiplayerCollection<MultiplayerAvatar>
    {
        internal MultiplayerAvatars(MultiplayerSession session) : base(session)
        {
            Multiplayer.MultiplayerJoined += OnMultiplayerJoined;
        }

        private void OnMultiplayerJoined()
        {
            LocalAvatar = new MultiplayerAvatar()
            {
                ID = Multiplayer.AccessToken
            };
        }

        protected override string KeyPrefix => "avatar.";
        
        protected override bool ParseItem(string key, object value, out MultiplayerAvatar parsed)
        {
            if (value is Dictionary<string, object> dict)
            {
                parsed = Serialization.Serialization.FromDataStructure<MultiplayerAvatar>(dict);
                parsed.ID = key.Remove(0, KeyPrefix.Length);
                return true;
            }

            parsed = default;
            return false;
        }

        protected override object SerializeItem(MultiplayerAvatar item)
        {
            return Serialization.Serialization.ToDataStructure(item);
        }
        
        /// <summary>
        /// A list of <see cref="MultiplayerAvatar"/> which are not the current player.
        /// </summary>
        public IEnumerable<MultiplayerAvatar> OtherPlayerAvatars =>
            Values.Where(avatar => avatar.ID != Multiplayer.AccessToken);

        /// <summary>
        /// The <see cref="MultiplayerAvatar"/> which is the local player, and hence
        /// not controlled by the shared state dictionary.
        /// </summary>
        public MultiplayerAvatar LocalAvatar = new MultiplayerAvatar();

        private string LocalAvatarId => $"avatar.{Multiplayer.AccessToken}";
        
        /// <summary>
        /// Add your local avatar to the shared state dictionary
        /// </summary>
        public void FlushLocalAvatar()
        {
            UpdateValue(LocalAvatarId, LocalAvatar);
        }

        internal void CloseClient()
        {
            RemoveValue(LocalAvatarId);
        }
    }
}
