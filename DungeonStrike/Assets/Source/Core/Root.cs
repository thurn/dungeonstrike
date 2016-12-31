using System;
using DungeonStrike.Assets.Source.Messaging;

namespace DungeonStrike.Assets.Source.Core
{
    public sealed class Root : DungeonStrikeBehavior
    {
        public override void DungeonStrikeBehaviorAwake()
        {
            gameObject.AddComponent<MessageRouter>();
        }
    }
}