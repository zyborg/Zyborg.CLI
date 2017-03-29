Zyborg.CLI
===

Extensions to the commandline interface (CLI) support found in
[Microsoft.Extensions.CommandLineUtils](https://www.nuget.org/packages/Microsoft.Extensions.CommandLineUtils/) (CLU).

## Zyborg.CLI.Binder

The Binder package provides support for CLI model-binding, That is, functionality to define a CLI
parameter configuration from a user-provided class (the model) and bind the results of parsing a
set of CLI arguments against that configuration to an instance of the model class.

### The Problem

The typical usage of the CLU package goes something like this:

```csharp
  public static void Main(params string[] args)
  {
    // Define the root command configuration
    var cla = new CommandLineApplication(throwOnUnexpectedArg: false);
    
    // Add various single-value or multi-value options or no-value options (flags)
    CommandOption greeting = cla.Option("-$|-g|--greeting <greeting>",
        "The greeting to display.",
        CommandOptionType.SingleValue);
    CommandOption uppercase = commandLineApplication.Option("-u | --uppercase",
        "Display the greetee in uppercase.",
        CommandOptionType.NoValue);
    
    // Add some named arguments and/or child commands (here we combine both)
    CommandArgument names;
    cla.Command("name", (childCla) =>
        names = childCla.Argument(
            "fullname",
            "Enter the full name of the person to be greeted.",
            multipleValues: true));
    
    // Enable built-in support for nicely-formatted help
    cla.HelpOption("-? | -h | --help");

    // Define a handler for *resolving* the result of parsing the CLI args
    // which usually entails interpretting all the possible values and
    // combinations and invoking some action
    cla.OnExecute(() =>
    {
        if (greeting.HasValue)
            foreach (var n in names.Values)
                Console.WriteLine(greeting.Value() + " "
                        + (uppercase.HasValue() ? n.ToUpper() : n));
    });

    // Apply the configuration to interpret the CLI args
    cla.Execute(args);
  }
```
<sup>Sample adapted from [this helpful article](https://msdn.microsoft.com/en-us/magazine/mt763239.aspx?f=255&MSPPError=-2147217396#code-snippet-2).
There are a few contrived elements in this example but they illustrate
all the key functions of the CLU package and the typical usage pattern.</sup>
  
All of this is a bit clunky -- the parameters are a bit hard to decode at a glance and the
configuration is hard to easily reason about without digging into the individual calls.  And this
is a pretty trivial example, but imagine how easily it can grow very complicated and tedious to
manage.

Now, invoking the built-in help support will produce a nicely-formatted display that definitely
helps to understand the resulting parameters, but it's not really oriented to aid
for design- and development-time.

### The Model-Driven Approach

The Binder package re-orients this imperative approach around into a declarative model.

Here is the same same configuration as above in this declarative style:

```csharp
[CommandLineApplication(ThrowOnUnexpectedArg = false)]
[HelpOption("-?|-h|--help")]
public class MyCommandModel
{
    [Option("-$|-g|--greeting <greeting>", "The greeting to display.")]
    public string Greeting
    { get; set; }

    [Option("-u|--uppercase", "Display the greeting in uppercase.")]
    public bool UpperCase
    { get; set; }

    [Command("name")]
    public NameCommandModel Name
    { get; set; }

    public void DoSomething()
    {
        if (!string.IsNullOrEmpty(Greeting) && Name?.Names != null)
            foreach (var n in Name.Names)
                Console.WriteLine(Greeting + " "
                        (UpperCase ? n.ToUpper() : n));
    }
}

public class NameCommandModel
{
    [Argument]
    public string[] Names
    { get; set; }
}

public static void Main(params string[] args)
{
    var cliBinding = CommandLineBinding<MyCommandModel>.Build();
    cliBinding.Execute(args);
    cliBinding.Model.DoSomething();
}
```
The basic idea is simple.

* To define a CLI parameter configuration:
   1. You define model classes that correspond to individual commands (including the root command)
      which correspond to each `CommandLineApplication` definition.
   1. In each model class you define properties (or methods) that correspond to your options
      and arguments.
   1. You decorate these class members with custom attributes to fine-tune
      specific behavior and override sensible defaults.
   1. You can also define properties (or methods) for sub-commands -- just specify a member type
      that is another custom model class capturing the child parameter configuration.
   1. You invoke the type-safe `Build()` method of the binder class which constructs and returns
      a *binding* instance.
* To apply the CLI parameter configuration to set of arguments:
   1. Invoke the `Execute()` method on the binding instance and pass in all the arguments.
   1. The binding will construct and store an instance of the model class.
   1. The binding will then populate the model by parsing the arguments in the context of the
      parameter configuration including resolving values for model members (setting properties
      or invoking methods) with the corresponding option values as well as named and unnamed
      arguments.
   1. For sub-commands, and instance of the child model class will be constructed and assigned
      to the corresponding parent model property (or passed into a method invocation).

Under the hood, the binder is still utilizing all of the components of the CLU package,
but uses the declarative model to build up the CLI parameter configuration and return.
There are various optional *hooks* and extension points available to you to tap into
the binding and configuration behavior so that you still have all of the same level of
power and flexibility as using the imperative CLU approach should you need it.

----

### Binder Detailed Reference

#### Model-decorating Attributes

##### `CommandLineApplication` Attribute

Class-level attribute used to decorate command model classes to specify various details
about the corresponding command.  All the attribute properties are optional, and the
attribute itself is not necessary unless you want to specify at least one of these
properties.

   * `Name` - for top-level commands (the root), there is no default name; for sub-commands,
     the default is derived from the member name corresponding to the sub-command on the parent
     model class
   * `FullName` - their is no default
   * `Description` - provides a top-level command description or description for each sub-command
     which are used to generate help messages
   * `AllowArgumentSeparator` - defaults to `false`
   * `ThrowOnUnexpectedArg` - defaults to `false`

##### `HelpOption` Attribute

Class-level attribute use to defin built-in help support.  This attribute can be used at
any command level (root or child sub-command).

* `Template` - defines which option indicators trigger the built-in help support

##### `VersionOption` Attribute

Class-level attribute used to define built-in version information support.  This attribute
really only has an effect for a top-level (root) command to resolve application-wide version
behavior.

   * `Template` - defines which option indicators trigger the built-in version support
   * `ShortVersion` - defines a static *short version* value
   * `LongVersion` - defines a static *long version* value

##### `ShortVersionGetter` Attribute

Member-level (Property or Method) attribute to designate a member that can return a
value representing the *short version* of the command.  This really only has an effect
for a top-level (root) command to resolve an application-wide version.

Defining a member with this attribute will override any static value that may be assigned
to the `ShortVersion` property of the `VersionOption` attribute at the model class level.

##### `LongVersionGetter` Attribute

Member-level (Property or Method) attribute to designate a member that can return a
value representing the *long version* of the command.  This really only has an effect
for a top-level (root) command to resolve an application-wide version.

Defining a member with this attribute will override any static value that may be assigned
to the `LongtVersion` property of the `VersionOption` attribute at the model class level.

##### `Command` Attribute

Member-level (Property or Method) attribute to define and capture a child sub-command.
The member should be able to receive a value of a custom user type that defines the
configuration of the sub-command.  The custom type can optionally be decorated with
the `CommandLineApplication` attribute to define sub-command details.  Some details
overlap with properties of this attribute which are used to override the class-level
defaults.

   * `Name` - if not specified, and not specified by the custom class'
     `CommandLineApplication` attribute, a default name is resolved based on the
     member name
   * `ThrowOnUnexpectedArg` - default to false

##### `Option` Attribute

Member-level (Property or Method) attribute

   * `Template`
   * `Description`
   * `OptionType`
   * `Inherited`

##### `Argument` Attribute

Member-level (Property or Method) attribute

   * `Name`
   * `Description`
   * `MultipleValues`

##### `RemainingArguments` Attribute

Member-level (Property or Method) attribute to designate a member that can receive a
value of type `string[]` corresponding to all the remaining arguments that are not
captured by named arguments.

   * `SkipIfNone` - default to `false`; if `true` and there are no unnamed arguments,
     then the corresponding member will not be invoked.