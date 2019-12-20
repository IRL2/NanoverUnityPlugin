using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Narupa.Protocol.Command;

namespace Narupa.Grpc.Tests.Commands
{
    internal class CommandService : Command.CommandBase, IBindableService
    {
        public event Action<string, Struct> ReceivedCommand;

        public override async Task<CommandReply> RunCommand(CommandMessage request, ServerCallContext context)
        {
            ReceivedCommand?.Invoke(request.Name, request.Arguments);
            return new CommandReply();
        }

        public override async Task<GetCommandsReply> GetCommands(GetCommandsRequest request, ServerCallContext context)
        {
            return new GetCommandsReply();
        }

        public ServerServiceDefinition BindService()
        {
            return Command.BindService(this);
        }
    }
}