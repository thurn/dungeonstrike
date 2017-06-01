using System;

namespace DungeonStrike.Source.Utilities
{
    public class IdGenerator
    {
        public static string NewId(string prefix)
        {
            var guid = Guid.NewGuid();
            var clientId = prefix + ":" + Convert.ToBase64String(guid.ToByteArray());
            return clientId.Substring(0, clientId.Length - 2);
        }
    }
}