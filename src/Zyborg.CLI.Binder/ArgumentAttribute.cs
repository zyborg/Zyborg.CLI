using System;

namespace Zyborg.CLI.Binder
{
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,
            Inherited = false, AllowMultiple = false)]
    public sealed class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute()
        { }

        public ArgumentAttribute(string name)
        {
            Name = name;
        }

        public ArgumentAttribute(string name, string description)
        {
            Name = name;
            Description = name;
        }

        public string Name
        { get; set; }

        public string Description
        { get; set; }

        public bool? MultipleValues
        { get; set; }
    }
}