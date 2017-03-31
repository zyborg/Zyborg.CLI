using Microsoft.Extensions.CommandLineUtils;

namespace Zyborg.CLI.Binder.Models
{
    [CommandLine(ThrowOnUnexpectedArg = false)]
    [HelpOption("-?|-h|--help")]
    public class DocSampleModel
    {
        [Option("-$|-g|--greeting <greeting>", "The greeting to display.")]
        public string Greeting
        { get; set; }

        [Option("-u|--uppercase", "Display the name in uppercase.")]
        public bool UpperCase
        { get; set; }

        [Command("name")]
        public NameCommandModel Name
        { get; set; }

        public void Command_OnExec(CommandLineApplication cla)
        {
            cla.Out.WriteLine("Executing...");

            if (!string.IsNullOrEmpty(Greeting))
            {
                cla.Out.WriteLine($"{Greeting} to the {(UpperCase ? "WORLD" : "World")}");
            }
        }
    }

    [HelpOptionAttribute("--help-me")]
    public class NameCommandModel
    {
        private DocSampleModel _parent;

        public NameCommandModel(DocSampleModel parent)
        {
            _parent = parent;
        }

        [Argument(Description = "Enter the names of the people to be greeted.")]
        public string[] Names
        { get; set; } = new string[0];

        public void Command_OnExec(CommandLineApplication cla)
        {
            cla.Out.WriteLine("Executing...");
            if (!string.IsNullOrEmpty(_parent.Greeting))
            {
                cla.Out.WriteLine("Greetings to the following:");
                foreach (var n in Names)
                {
                    cla.Out.WriteLine(_parent.Greeting + " "
                            + (_parent.UpperCase ? n.ToUpper() : n));
                }
            }
        }
    }
}