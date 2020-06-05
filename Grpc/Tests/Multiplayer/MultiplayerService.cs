using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Narupa.Core;
using Narupa.Core.Collections;
using Narupa.Protocol.State;

namespace Narupa.Grpc.Tests.Multiplayer
{
    public class MultiplayerService : State.StateBase, IBindableService
    {
        private ObservableDictionary<string, object> resources
            = new ObservableDictionary<string, object>();

        private Dictionary<string, string> locks = new Dictionary<string, string>();

        public IDictionary<string, object> Resources => resources;

        public IReadOnlyDictionary<string, string> Locks => locks;

        public override async Task<UpdateLocksResponse> UpdateLocks(
            UpdateLocksRequest request,
            ServerCallContext context)
        {
            var token = request.AccessToken;
            foreach (var requestKey in request.LockKeys.Fields.Keys)
                if (locks.ContainsKey(requestKey) && locks[requestKey] != token)
                    return new UpdateLocksResponse
                    {
                        Success = false
                    };
            foreach (var (key, lockTime) in request.LockKeys.Fields)
            {
                if (lockTime.KindCase == Value.KindOneofCase.NullValue)
                {
                    locks.Remove(key);
                }
                else
                {
                    locks[key] = token;
                }
            }

            return new UpdateLocksResponse
            {
                Success = true
            };
        }

        public override async Task<UpdateStateResponse> UpdateState(
            UpdateStateRequest request,
            ServerCallContext context)
        {
            var token = request.AccessToken;
            foreach (var requestKey in request.Update.ChangedKeys.Fields.Keys)
                if (locks.ContainsKey(requestKey) && locks[requestKey] != token)
                    return new UpdateStateResponse
                    {
                        Success = false
                    };
            foreach (var (key, value) in request.Update.ChangedKeys.Fields)
            {
                if (value.KindCase == Value.KindOneofCase.NullValue)
                {
                    resources.Remove(key);
                }
                else
                {
                    resources[key] = token;
                }
            }

            return new UpdateStateResponse
            {
                Success = true
            };
        }

        public override async Task SubscribeStateUpdates(SubscribeStateUpdatesRequest request,
                                                         IServerStreamWriter<StateUpdate>
                                                             responseStream,
                                                         ServerCallContext context)
        {
            var millisecondTiming = (int) (request.UpdateInterval * 1000);
            var update = new StateUpdate
            {
                ChangedKeys = new Struct()
            };

            void ResourcesOnCollectionChanged(object sender,
                                              NotifyCollectionChangedEventArgs e)
            {
                var (changes, removals) = e.AsChangesAndRemovals<string>();

                foreach (var change in changes)
                    update.ChangedKeys.Fields[change] = resources[change].ToProtobufValue();
                foreach (var removal in removals)
                    update.ChangedKeys.Fields[removal] = Value.ForNull();
            }

            resources.CollectionChanged += ResourcesOnCollectionChanged;
            while (true)
            {
                await Task.Delay(millisecondTiming);
                if (update.ChangedKeys.Fields.Any())
                {
                    var toSend = update;
                    update = new StateUpdate
                    {
                        ChangedKeys = new Struct()
                    };
                    await responseStream.WriteAsync(toSend);
                }
            }
        }

        public ServerServiceDefinition BindService()
        {
            return State.BindService(this);
        }

        public void SetValueDirect(string key, string value)
        {
            this.resources[key] = value;
        }
    }
}