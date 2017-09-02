using System;

namespace DungeonStrike.Source.Utilities
{
    public class IdGenerator
    {
        /// <returns>A randomly generated ID to use as an Action ID</returns>
        public static string NewActionId()
        {
            return NewId("A");
        }

        /// <returns>A randomly generated ID to use as a Client ID</returns>
        public static string NewClientId()
        {
            return NewId("C");
        }

        private static string NewId(string prefix)
        {
            var guid = Guid.NewGuid();
            var clientId = prefix + ":" + Convert.ToBase64String(guid.ToByteArray());
            return clientId.Substring(0, clientId.Length - 2);
        }
    }
}