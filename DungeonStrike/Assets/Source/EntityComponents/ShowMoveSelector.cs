using System.Threading.Tasks;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;

namespace DungeonStrike.Source.EntityComponents
{
    public class ShowMoveSelector : EntityComponent
    {
        protected override string MessageType => ShowMoveSelectorMessage.Type;

        protected override Task<Result> OnEnableEntityComponent()
        {
            Logger.Log("ShowMoveSelector:: OnEnableEntityComponent");
            return Async.Success;
        }

        protected override Task<Result> HandleMessage(Message receivedMessage)
        {
            var message = (ShowMoveSelectorMessage) receivedMessage;
            Logger.Log("Got show move selector message");
            Logger.Log(message.Positions.ToString());
            return Async.Success;
        }
    }
}