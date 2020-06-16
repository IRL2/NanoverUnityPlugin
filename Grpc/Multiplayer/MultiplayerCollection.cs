using System;
using System.Collections.Generic;

namespace Narupa.Grpc.Multiplayer
{
    public abstract class MultiplayerCollection<TItem>
    {
        protected abstract string KeyPrefix { get; }

        protected MultiplayerSession Multiplayer { get; }
        
        protected abstract bool ParseItem(string key, object value, out TItem parsed);

        protected abstract object SerializeItem(TItem item);
        
        private Dictionary<string, TItem> multiplayerState = new Dictionary<string, TItem>();
        
        private Dictionary<string, (int, TItem)> localChanges = new Dictionary<string, (int, TItem)>();
        
        private Dictionary<string, int> localRemovals = new Dictionary<string, int>();

        public event Action<string> KeyUpdated;
        public event Action<string> KeyRemoved;
        
        protected MultiplayerCollection(MultiplayerSession session)
        {
            Multiplayer = session;
            Multiplayer.SharedStateDictionaryKeyUpdated += OnKeyUpdated;
            Multiplayer.SharedStateDictionaryKeyRemoved += OnKeyRemoved;
        }
        
        private void OnKeyUpdated(string key, object value)
        {
            if (key.StartsWith(KeyPrefix) && ParseItem(key, value, out var item))
            {
                CreateOrUpdateItem(key, item);
            }
        }

        private void OnKeyRemoved(string key)
        {
            if (key.StartsWith(KeyPrefix))
            {
                RemoveItem(key);
                KeyRemoved?.Invoke(key);
            }
        }
        
        private void CreateOrUpdateItem(string key, TItem value)
        {
            if (!multiplayerState.ContainsKey(key))
                multiplayerState.Add(key, value);
            else
                multiplayerState[key] = value;
            if (localChanges.ContainsKey(key))
            {
                var sentUpdateIndex = localChanges[key].Item1;
                if (sentUpdateIndex <= Multiplayer.LastReceivedIndex)
                {
                    localChanges.Remove(key);
                    KeyUpdated?.Invoke(key);
                }
            }
            else
            {
                KeyUpdated?.Invoke(key);
            }
        }

        private void RemoveItem(string id)
        {
            multiplayerState.Remove(id);
        }

        public TItem GetValue(string id)
        {
            if (localRemovals.ContainsKey(id))
                throw new KeyNotFoundException($"Key removed: {id}");
            if (localChanges.ContainsKey(id))
                return localChanges[id].Item2;
            return multiplayerState[id];
        }

        public ICollection<string> Keys
        {
            get
            {
                var keys = new HashSet<string>();
                foreach (var key in localChanges.Keys)
                    keys.Add(key);
                foreach (var key in multiplayerState.Keys)
                    keys.Add(key);
                foreach (var key in localRemovals.Keys)
                    keys.Remove(key);
                return keys;
            }
        }
        
        public IEnumerable<TItem> Values
        {
            get
            {
                foreach (var key in Keys)
                    yield return GetValue(key);
            }
        }
        
        public void UpdateValue(string id)
        {
            UpdateValue(id, GetValue(id));
        }
        
        public void UpdateValue(string id, TItem value)
        {
            if (localRemovals.ContainsKey(id))
                localRemovals.Remove(id);
            localChanges[id] = (Multiplayer.NextUpdateIndex, value);
            Multiplayer.SetSharedState(id, SerializeItem(value));
            KeyUpdated?.Invoke(id);
        }
        
        public void RemoveValue(string id)
        {
            if (localChanges.ContainsKey(id))
                localChanges.Remove(id);
            localRemovals[id] = Multiplayer.NextUpdateIndex;
            Multiplayer.RemoveSharedStateKey(id);
            KeyRemoved?.Invoke(id);
        }
    }
}