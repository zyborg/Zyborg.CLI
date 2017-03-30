using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;

namespace Zyborg.CLI.Binder.Models
{

    [CommandLineApplication]
    [HelpOption("--help-me")]
    public class BasicSwitches
    {
        public bool _impliciBoolCalled;
        public bool _impliciBoolValue;

        [Option]
        public void ImplicitBool(bool value)
        {
            _impliciBoolCalled = true;
            _impliciBoolValue = value;
        }

        [Option]
        public bool? ImplicitBoolNullable
        { get; set; }
    }

    [HelpOption(HELP_TEMPLATE)]
    [CommandLineApplication("FOO",
                ThrowOnUnexpectedArg = true)]
    public class BasicCommands
    {
        public const string HELP_TEMPLATE = "-?";

        public SubCommand2 _sub2;

        [Option]
        public bool Opt1
        { get; set; }

        [Command]
        public SubCommand1 Sub1
        { get; set; }

        [Command]
        public void Sub2(SubCommand2 cmd)
        {
            _sub2 = cmd;
        }

        [Command]
        public SubCommand3 Sub3
        { get; }

        [CommandLineApplication("FOOBAR")]
        public class SubCommand1
        {
            [Option]
            public string Url
            { get; set; }
        }

        [CommandLineApplication("SUB2", FullName = "Sub Command #2",
                Description = "This is the second sub command")]
        public class SubCommand2
        {
            [Option]
            public string Path
            { get; set; }
            
        }

        public class SubCommand3
        {
            [Option]
            public string Alias
            { get; set; }
        }
    }

    [HelpOption(HELP_TEMPLATE)]
    public class BasicMultiOptions
    {
        public const string HELP_TEMPLATE = "--help-me";

        public string[] _multi2Values;
        public IEnumerable<string> _multi3Values;

        [Option]
        public string[] Multi1
        { get; set; }

        [Option]
        public void Multi2(string[] values)
        {
            _multi2Values = values;
        }

        [Option]
        public void Multi3(params string[] values)
        {
            _multi3Values = values;
        }
    }

    public class BasicOnBind1
    {
        public CommandOption _opt1OnBindConfig;
        public CommandArgument _arg1OnBindConfig;
        public CommandLineApplication _cmd1OnBindConfig;

        [Option]
        public string Opt1
        { get; set; }

        public void Opt1_OnBind(CommandOption config)
        {
            _opt1OnBindConfig = config;
        }

        [Argument]
        public string Arg1
        { get; set; }

        public void Arg1_OnBind(CommandArgument config)
        {
            _arg1OnBindConfig = config;
        }

        [Command]
        public SubCommand1 Cmd1
        { get; set; }

        public void Cmd1_OnBind(CommandLineApplication config)
        {
            _cmd1OnBindConfig = config;
        }

        public class SubCommand1
        { }
    }

    public class BasicOnInit1
    {
        public bool _onInitInvoked;
        
        public void Model_OnInit()
        {
            _onInitInvoked = true;
        }
    }

    public class BasicOnInit2
    {
        public bool _onInitInvoked;
        public CommandLineApplication _onInitParam;

        public void Model_OnInit(CommandLineApplication cla)
        {
            _onInitInvoked = true;
            _onInitParam = cla;
        }
    }

    public class BasicOnExec1
    {
        public bool _onExecInvoked;
        
        public void Model_OnExec()
        {
            _onExecInvoked = true;
        }
    }

    public class BasicOnExec2
    {
        public bool _onExecInvoked;
        public CommandLineApplication _onExecParam;

        public void Model_OnExec(CommandLineApplication cla)
        {
            _onExecInvoked = true;
            _onExecParam = cla;
        }
    }

    public class BasicOnExec3
    {
        public const int RETURN_VALUE = 99;

        public bool _onExecInvoked;
        public CommandLineApplication _onExecParam;

        public int Model_OnExec(CommandLineApplication cla)
        {
            _onExecInvoked = true;
            _onExecParam = cla;

            return RETURN_VALUE;
        }
    }

    public class BasicRemainingArgs1
    {
        [Option]
        public bool Flag1
        { get; set; }

        [Option]
        public string Opt1
        { get; set; }

        [Option]
        public string[] Multi1
        { get; set; }

        [RemainingArguments]
        public string[] TheRest
        { get; set; }
    }
    
    public class BasicRemainingArgs2
    {
        public IEnumerable<string> _theRest;

        [RemainingArguments(true)]
        public void TheRest(IEnumerable<string> value)
        {
            _theRest = value;
        }
    }
    
    public class BasicRemainingArgs3
    {
        [RemainingArguments]
        public string[] TheRest1
        { get; set; }
        [RemainingArguments]
        public string[] TheRest2
        { get; set; }
        [RemainingArguments]
        public string[] TheRest3
        { get; set; }
    }
    
}