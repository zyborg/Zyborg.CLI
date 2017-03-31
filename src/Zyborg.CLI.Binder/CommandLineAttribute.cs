using System;

namespace Zyborg.CLI.Binder
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CommandLineAttribute : Attribute
    {
        public CommandLineAttribute ()
        { }

        public CommandLineAttribute (string name)
        {
            Name = name;
        }

        public string Name
        { get; set; }

        public string FullName
        { get; set; }

        public string Description
        { get; set; }

        public bool AllowArgumentSeparator
        { get; set; }

        public bool ThrowOnUnexpectedArg
        { get; set; } = true;
    }
}