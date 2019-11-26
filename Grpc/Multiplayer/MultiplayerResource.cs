using System;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Session;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Represents a multiplayer resource that is shared across the multiplayer
    /// service.
    /// </summary>
    /// <typeparam name="TValue">The type to interpret the value of this key as.</typeparam>
    public class MultiplayerResource<TValue>
    {
        /// <summary>
        /// Create a multiplayer resource.
        /// </summary>
        /// <param name="session">The multiplayer session that will provide this value.</param>
        /// <param name="key">The key that identifies this resource in the dictionary.</param>
        /// <param name="objectToValue">
        /// An optional converter for converting the value in
        /// the dictionary to an appropriate value.
        /// </param>
        /// <param name="valueToObject">
        /// An optional converter for converting the value
        /// provided to one suitable for serialisation to protobuf.
        /// </param>
        public MultiplayerResource(MultiplayerSession session,
                                   string key,
                                   Converter<object, TValue> objectToValue = null,
                                   Converter<TValue, object> valueToObject = null)
        {
            this.session = session;
            ResourceKey = key;
            LockState = MultiplayerResourceLockState.Unlocked;
            session.SharedStateDictionaryKeyUpdated += SharedStateDictionaryKeyUpdated;
            this.objectToValue = objectToValue;
            this.valueToObject = valueToObject;
        }

        private readonly Converter<object, TValue> objectToValue;

        private readonly Converter<TValue, object> valueToObject;

        /// <summary>
        /// Convert the value provided to one suitable for serialisation to protobuf.
        /// </summary>
        private object ValueToObject(TValue value)
        {
            return valueToObject != null ? valueToObject(value) : value;
        }

        /// <summary>
        /// Convert the value in the dictionary to an appropriate value.
        /// </summary>
        private TValue ObjectToValue(object obj)
        {
            if (objectToValue != null)
                return objectToValue(obj);
            if (obj is TValue v)
                return v;
            return default;
        }

        /// <summary>
        /// Callback for when the shared value is changed.
        /// </summary>
        public Action SharedValueChanged;

        /// <summary>
        /// Callback for when the value is changed, either remotely or locally.
        /// </summary>
        public Action ValueChanged;

        /// <summary>
        /// Callback for when a lock request is accepted.
        /// </summary>
        public Action LockAccepted;

        /// <summary>
        /// Callback for when a lock request is rejected.
        /// </summary>
        public Action LockRejected;

        /// <summary>
        /// Callback for when a lock is released.
        /// </summary>
        public Action LockReleased;

        private void SharedStateDictionaryKeyUpdated(string key)
        {
            if (key == ResourceKey)
            {
                UpdateValueFromMultiplayer();
                SharedValueChanged?.Invoke();
            }
        }

        private TValue GetValueFromMultiplayer()
        {
            var obj = session.GetSharedState(ResourceKey);
            if (objectToValue != null)
                return objectToValue(obj);
            if (obj is TValue v)
                return v;
            return default;
        }

        /// <summary>
        /// Copy the locally set version of this value to the multiplayer service.
        /// </summary>
        private void UpdateMultiplayerValue()
        {
            session.SetSharedState(ResourceKey, ValueToObject(value));
        }

        /// <summary>
        /// Copy the remote value to this value.
        /// </summary>
        private void UpdateValueFromMultiplayer()
        {
            value = GetValueFromMultiplayer();
            ValueChanged?.Invoke();
        }

        private TValue value;

        /// <summary>
        /// The key which identifies this resource.
        /// </summary>
        public readonly string ResourceKey;

        /// <summary>
        /// The current state of the lock on this resource.
        /// </summary>
        public MultiplayerResourceLockState LockState { get; private set; }

        private MultiplayerSession session;

        /// <summary>
        /// Value of this resource. Mirrors the value in the remote dictionary, unless a
        /// local change is in progress.
        /// </summary>
        public TValue Value => value;

        /// <summary>
        /// Obtain a lock on this resource.
        /// </summary>
        public void ObtainLock()
        {
            ObtainLockAsync().AwaitInBackground();
        }

        /// <summary>
        /// Release the lock on this resource.
        /// </summary>
        public void ReleaseLock()
        {
            UpdateValueFromMultiplayer();
            ReleaseLockAsync().AwaitInBackground();
        }

        private async Task ObtainLockAsync()
        {
            LockState = MultiplayerResourceLockState.Pending;
            var success = await session.LockResource(ResourceKey);
            LockState = success
                            ? MultiplayerResourceLockState.Locked
                            : MultiplayerResourceLockState.Unlocked;
            if (success)
            {
                LockAccepted?.Invoke();
            }
            else
            {
                // Revert to remote value.
                UpdateValueFromMultiplayer();
                LockRejected?.Invoke();
            }
        }

        private async Task ReleaseLockAsync()
        {
            if (LockState != MultiplayerResourceLockState.Unlocked)
            {
                LockState = MultiplayerResourceLockState.Unlocked;
                UpdateValueFromMultiplayer();
                LockReleased?.Invoke();
                await session.ReleaseResource(ResourceKey);
            }
        }

        /// <summary>
        /// Set the local value of this key, and try to lock this resource to send it to
        /// everyone.
        /// If it is rejected, the value will revert to the default.
        /// </summary>
        public void UpdateValueWithLock(TValue value)
        {
            this.value = value;
            ValueChanged?.Invoke();
            switch (LockState)
            {
                case MultiplayerResourceLockState.Unlocked:
                    ObtainLock();
                    return;
                case MultiplayerResourceLockState.Pending:
                    return;
                case MultiplayerResourceLockState.Locked:
                    UpdateMultiplayerValue();
                    return;
            }
        }

        /// <summary>
        /// Set the value of this resource, disregarding the state of any lock.
        /// </summary>
        public void UpdateValueWithoutLock(TValue value)
        {
            this.value = value;
            ValueChanged?.Invoke();
            UpdateMultiplayerValue();
        }
    }
}