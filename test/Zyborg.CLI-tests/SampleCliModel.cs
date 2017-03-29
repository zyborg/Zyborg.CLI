using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;

namespace Zyborg.CLI.Binder
{
 
    [CommandLineApplication("SampleApp", FullName = "Sample Application",
            Description = "This is a sample CLI application.")]
    [HelpOption("-h|--help")]
    [VersionOption("-v|--version",
            ShortVersion = "0.0.0",
            LongVersion = "Version 0.0.0")]
    public class SampleCliModel
    {
        [ShortVersionGetter]
        public string Version
        {
            get { return this.GetType().GetTypeInfo().Assembly.GetName().Version.ToString(); }
        }

        [LongVersionGetter]
        public string GetLongVersion() => $"Version {this.Version}";


        // Can be specified as any of:
        //    -f
        //    --flag-option
        //    --flag-option:true
        //    --flag-option:false
        [Option("-f|--flag-option", "A flag option")]
        public bool? FlagOption
        { get; set; }

        // Invoked during "setup" of the CLI configuraiton
        public void Bar_OnConfigure(CommandOption option)
        {
        }

        [Option("-s|--single", "A single-valued option")]
        public string SingleValueOption
        { get; set; }

        [Option("-m|--multi", "A multi-valued option")]
        public string[] MultiValueOption
        { get; set; }

        [Option]
        public void Name(string value)
        {
            // do something with value
        }

        // A flag option that will invoke this method if specified
        [Option]
        public void AnotherFlag()
        { }

        // [CommandAttribute]
        // public SubCommand1 SubCommand1
        // { get; set; }

        // [CommandAttribute]
        // public void SubCommand2(SubCommand1 cmd)
        // {
            
        // }


        public void OnInit(CommandLineApplication cla)
        { }
    }


    public class SubCommand1
    {

    }
}