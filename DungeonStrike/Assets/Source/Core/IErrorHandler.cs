using System;

namespace DungeonStrike.Assets.Source.Core
{
    public interface IErrorHandler
    {
        void ReportError<T>(string message, T value = default(T));

        void CheckArgument<T>(bool expression, string message = "", T value = default(T));

        void CheckState<T>(bool expression, string message = "", T value = default(T));

        void CheckNotNull<T>(T values);

        Exception UnexpectedEnumValue(Enum value);
    }
}