namespace DungeonStrike.Assets.Source.Core
{
    public sealed class StandardLogger : ILogger
    {
        private DungeonStrikeBehavior _behaviour;

        internal StandardLogger(DungeonStrikeBehavior behaviour)
        {
            _behaviour = behaviour;
        }

        public void Log(string message)
        {

        }

        public void Log<T>(string message, T value)
        {

        }
    }
}