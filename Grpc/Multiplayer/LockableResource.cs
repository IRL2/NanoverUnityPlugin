using System;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Session;
using UnityEngine;

namespace Narupa.Grpc.Multiplayer
{
    public class LockableResource<T>
    {
        public LockableResource(MultiplayerSession session,
                                string key,
                                Converter<object, T> objectToValue = null,
                                Converter<T, object> valueToObject = null)
        {
            this.session = session;
            ResourceKey = key;
            State = ResourceLockState.Unknown;
            session.SharedStateDictionaryKeyUpdated += SharedStateDictionaryKeyUpdated;
            this.objectToValue = objectToValue;
            this.valueToObject = valueToObject;
        }

        private Converter<object, T> objectToValue;

        private Converter<T, object> valueToObject;

        private void SharedStateDictionaryKeyUpdated(string key)
        {
            if (key == ResourceKey)
            {
                value = GetValueFromMultiplayer();
            }
        }

        private T GetValueFromMultiplayer()
        {
            var obj = session.GetSharedState(ResourceKey);
            if (objectToValue != null)
                return objectToValue(obj);
            if (obj is T v)
                return v;
            return default;
        }

        private void SetMultiplayerValue()
        {
            session.SetSharedState(ResourceKey, ValueToObject(value));
        }

        private object ValueToObject(T value)
        {
            return valueToObject != null ? valueToObject(value) : value;
        }

        private T value;

        public string ResourceKey { get; private set; }

        public event Action StateChanged;

        public ResourceLockState State { get; private set; }

        private MultiplayerSession session;

        public T Value => value;

        public void SetValueIfLocked(T value)
        {
            if (State == ResourceLockState.Accepted)
                session.SetSharedState(ResourceKey, value);
        }

        public void TryLock()
        {
            TryLockAsync().AwaitInBackground();
        }

        public void TryRelease()
        {
            value = GetValueFromMultiplayer();
            TryReleaseAsync().AwaitInBackground();
        }

        public async Task TryLockAsync()
        {
            SetState(ResourceLockState.Pending);
            var success = await session.LockResource(ResourceKey);
            if (!success)
                value = GetValueFromMultiplayer();
            SetState(success ? ResourceLockState.Accepted : ResourceLockState.Rejected);
        }

        public async Task TryReleaseAsync()
        {
            if (State == ResourceLockState.Accepted || State == ResourceLockState.Pending)
            {
                SetState(ResourceLockState.Unknown);
                value = GetValueFromMultiplayer();
                await session.ReleaseResource(ResourceKey);
            }
        }

        private void SetState(ResourceLockState state)
        {
            State = state;
            Debug.Log(state);
            StateChanged?.Invoke();
        }

        /// <summary>
        /// Set the local value of this key, and try to lock this resource to send it to everyone.
        /// If it is rejected, the value will revert to the default.
        /// </summary>
        public void SetLocalAndTryLocking(T value)
        {
            this.value = value;
            switch (State)
            {
                case ResourceLockState.Accepted:
                    SetMultiplayerValue();
                    return;
                case ResourceLockState.Rejected:
                case ResourceLockState.Unknown:
                    TryLock();
                    return;
                case ResourceLockState.Pending:
                    return;
            }
        }
    }
}