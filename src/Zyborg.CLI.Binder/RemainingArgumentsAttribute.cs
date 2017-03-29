using System;

namespace Zyborg.CLI.Binder
{
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,
            Inherited = false, AllowMultiple = false)]
    public sealed class RemainingArgumentsAttribute : Attribute
    {
        public RemainingArgumentsAttribute()
        { }

        public RemainingArgumentsAttribute(bool skipIfNone)
        {
            SkipIfNone = skipIfNone;
        }

        public bool SkipIfNone
        { get; set; }
    }
}