using System;

namespace Zyborg.CLI.Binder
{
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,
            Inherited = false, AllowMultiple = false)]
    public sealed class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute()
        { }

        public ArgumentAttribute(string description)
        {
            Description = description;
        }

        public ArgumentAttribute(string name, string description)
        {
            Name = name;
            Description = name;
        }

        public string Name
        { get; }

        public string Description
        { get; }

        public bool MultipleValues
        { get; set; }
    }
}