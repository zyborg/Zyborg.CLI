using System;

namespace Zyborg.CLI.Binder
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class HelpOptionAttribute : Attribute
    {
        public HelpOptionAttribute(string templates)
        {
            Template = templates;
        }

        public string Template
        { get; }
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class VersionOptionAttribute : Attribute
    {
        public VersionOptionAttribute(string templates)
        {
            Template = templates;
        }

        public string Template
        { get; }

        public string ShortVersion
        { get; set; }

        public string LongVersion
        { get; set; }
    }

    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,
            Inherited = false, AllowMultiple = false)]
    public sealed class ShortVersionGetterAttribute : Attribute
    {
        public ShortVersionGetterAttribute()
        { }
    }

    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,
            Inherited = false, AllowMultiple = false)]
    public sealed class LongVersionGetterAttribute : Attribute
    {
        public LongVersionGetterAttribute()
        { }
    }
}