using System;

namespace Zyborg.CLI.Binder
{
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,
            Inherited = false, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public CommandAttribute ()
        { }

        public CommandAttribute(string name)
        {
            Name = name;
        }

        public string Name
        { get; }

        public bool ThrowOnUnexpectedArg
        { get; set; } = true; // Defaults to true to match CLU default behavior
    }
}