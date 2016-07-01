using System;

namespace seek.automation.stub.Helpers
{
    public class InteractionNotFoundException : Exception
    {
        public InteractionNotFoundException(string message) : base(message) { }
    }
}
