using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Session;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Part of the <see cref="MultiplayerSession"/> who is in charge of managing multiplayer
    /// avatars.
    /// </summary>
    public class MultiplayerAvatars
    {
        private MultiplayerSession multiplayer;

        internal MultiplayerAvatars(MultiplayerSession session)
        {
            multiplayer = session;
            multiplayer.SharedStateDictionaryKeyUpdated += OnKeyUpdated;
            multiplayer.SharedStateDictionaryKeyRemoved += OnKeyRemoved;
            multiplayer.MultiplayerJoined += OnMultiplayerJoined;
        }

        private void OnMultiplayerJoined()
        {
            LocalAvatar = new MultiplayerAvatar()
            {
                ID = multiplayer.PlayerId
            };
        }

        private void OnKeyUpdated(string key, object value)
        {
            if (IsAvatarKey(key, out var id) && value is Dictionary<string, object> dict)
            {
                CreateOrUpdateAvatar(id, dict);
            }
        }

        private void OnKeyRemoved(string key)
        {
            if(IsAvatarKey(key, out var id))
                RemoveAvatar(id);
        }


        private static string GetAvatarKey(string playerId)
        {
            return $"avatar.{playerId}";
        }

        private static bool IsAvatarKey(string key, out string playerId)
        {
            if (key.StartsWith("avatar."))
            {
               playerId = key.Substring(7);
               return true;
            }

            playerId = default;
            return false;
        }
        
        private void CreateOrUpdateAvatar(string id, Dictionary<string, object> value = null)
        {
            if(!avatars.ContainsKey(id))
                avatars.Add(id, new MultiplayerAvatar()
                {
                    ID = id
                });
            avatars[id].FromData(value);
        }

        private void RemoveAvatar(string id)
        {
            avatars.Remove(id);
        }
        
        private Dictionary<string, MultiplayerAvatar> avatars = new Dictionary<string, MultiplayerAvatar>();

        /// <summary>
        /// A list of <see cref="MultiplayerAvatar"/> which are not the current player.
        /// </summary>
        public IEnumerable<MultiplayerAvatar> OtherPlayerAvatars =>
            avatars.Values.Where(avatar => avatar.ID != multiplayer.PlayerId);

        /// <summary>
        /// The <see cref="MultiplayerAvatar"/> which is the local player, and hence
        /// not controlled by the shared state dictionary.
        /// </summary>
        public MultiplayerAvatar LocalAvatar = new MultiplayerAvatar();

        /// <summary>
        /// Add your local avatar to the shared state dictionary
        /// </summary>
        public void FlushLocalAvatar()
        {
            multiplayer.SetSharedState(GetAvatarKey(LocalAvatar.ID), LocalAvatar.ToData());
        }

        internal void CloseClient()
        {
            // Remove the avatar from multiplayer
            multiplayer.RemoveSharedStateKey(GetAvatarKey(multiplayer.PlayerId));
        }
    }
}
