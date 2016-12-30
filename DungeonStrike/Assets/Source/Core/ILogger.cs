namespace DungeonStrike.Assets.Source.Core
{
    public interface ILogger
    {
        void Log(string message);

        void Log<T>(string message, T value);
    }
}