using System;
using System.Text;
using DungeonStrike.Source.Utilities;
using UnityEngine;

namespace DungeonStrike.Source.Core
{
    public class LogContext
    {
        private readonly string _clientId;
        private readonly LogContext _parentContext;
        private readonly Type _type;
        private readonly string _gameObjectName;

        /// <summary>
        /// Creates a new LogContext which is a child of <paramref name="parentContext"/> for a behavior of type
        /// <paramref name="type"/> attached to <paramref name="gameObject"/>.
        /// </summary>
        public static LogContext NewContext(LogContext parentContext, Type type, GameObject gameObject)
        {
            if (parentContext == null)
            {
                throw new ArgumentNullException(nameof(parentContext));
            }
            return new LogContext(parentContext._clientId, parentContext, type,
                    gameObject != null ? gameObject.name : null);
        }

        /// <summary>
        /// Creates a new instance of the root log context. Should only be invoked from the Root.
        /// </summary>
        public static LogContext NewRootContext(Type type)
        {
            return new LogContext(IdGenerator.NewId("C"), null, type, null);
        }

        private LogContext(string clientId, LogContext parentContext, Type type, string gameObjectName)
        {
            _clientId = clientId;
            _parentContext = parentContext;
            _type = type;
            _gameObjectName = gameObjectName;
        }

        public void AppendContextParameters(StringBuilder stringBuilder)
        {
            stringBuilder.Append(":client-id \"").Append(_clientId).Append("\", ");
            stringBuilder.Append(":parents [");
            var parent = _parentContext;
            while (parent != null)
            {
                if (parent != _parentContext)
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append("\"").Append(parent._type).Append("\"");
                parent = parent._parentContext;
            }
            stringBuilder.Append("], ");

            if (_type != null)
            {
                stringBuilder.Append(":source \"").Append(_type).Append("\", ");
            }
            if (_gameObjectName != null)
            {
                stringBuilder.Append(":object-name \"").Append(_gameObjectName).Append("\", ");
            }
        }
    }
}