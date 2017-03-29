using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Zyborg.CLI.Binder
{
    /// Options are identified with a name, where the name is prefixed with either a single (-)
    /// or double dash (--). Option names are programmatically defined using templates and a
    /// template can include one or more of the following three designators: short name, long name, symbol.
    /// In addition, an Option might have a value associated with it. For example, a template might be
    /// “-n | --name | -# <Full Name>,” allowing the full name option to be identified by any of the
    /// three designators. (However, the template doesn’t need all three designators.) Note that it’s
    /// the use of a single or double dash that determines whether a short or long name is specified,
    /// regardless of the actual length of the name.
    ///
    /// To associate a value with an option, you can use either a space or the assignment operator (=).
    /// -f=Inigo and -l Montoya, therefore, are both examples of specifying an option value.
    ///
    /// If numbers are used in the template, they’ll be part of the short or long names, not the symbol.

    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        public OptionAttribute()
        { }

        public OptionAttribute(string description)
        {
            Description = description;
        }

        public OptionAttribute(string template, string description)
        {
            Template = template;
            Description = description;
        }

        public string Template
        { get; set; }

        public string Description
        { get; set; }

        public CommandOptionType? OptionType
        { get; set; }

        public bool Inherited
        { get; set; } = false;
    }
}