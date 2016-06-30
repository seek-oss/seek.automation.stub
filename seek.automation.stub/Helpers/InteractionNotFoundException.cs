using System;

namespace seek.automation.stub
{
    public class InteractionNotFoundException : Exception
    {
        public InteractionNotFoundException(string message) : base(message) { }
    }
}
