namespace DungeonStrike.Assets.Source.Core
{
    public sealed class Entity : DungeonStrikeBehavior
    {
        public string EntityType { get; private set; }
        public string EntityId { get; private set; }

        public void Initialize(string entityType, string entityId)
        {
            ErrorHandler.CheckNotNull(new { entityType, entityId });
            EntityType = entityType;
            EntityId = entityId;
        }
    }
}