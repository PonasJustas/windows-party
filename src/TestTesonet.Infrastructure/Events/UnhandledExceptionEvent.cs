using System;

namespace TestTesonet.Infrastructure.Events
{
    public class UnhandledExceptionEvent
    {
        public UnhandledExceptionEvent(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
