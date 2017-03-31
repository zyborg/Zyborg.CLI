using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Xunit;
using Zyborg.CLI.Binder.Models;

namespace Zyborg.CLI.Binder
{
    public class DocSampleTests
    {
        [Fact]
        public void TestSameHelp()
        {
            var claOut = new StringWriter();
            var claError = new StringWriter();

            var binderOut = new StringWriter();
            var binderError = new StringWriter();

            var cla = BuildDocSampleCLA();
            var binder = CommandLineBinding<DocSampleModel>.Build();

            SetOutErr(cla, claOut, claError);
            binder.Out = binderOut;
            binder.Error = binderError;

            var args = new[] { "-?" };

            cla.Execute(args);
            binder.Execute(args);

//Console.WriteLine("Out:  " + claOut.ToString());
//Console.WriteLine("Err:  " + claError.ToString());

            Assert.Equal(claOut.ToString(), binderOut.ToString());
            Assert.Equal(claError.ToString(), binderError.ToString());
        }

        [Fact]
        public void TestSameNameHelp()
        {
            var claOut = new StringWriter();
            var claError = new StringWriter();

            var binderOut = new StringWriter();
            var binderError = new StringWriter();

            var cla = BuildDocSampleCLA();
            var binder = CommandLineBinding<DocSampleModel>.Build();

            SetOutErr(cla, claOut, claError);
            binder.Out = binderOut;
            binder.Error = binderError;

            var args = new[] { "name", "--help-me" };

            cla.Execute(args);
            binder.Execute(args);

//Console.WriteLine(nameof(TestSameNameHelp) + "Out:  " + claOut.ToString());
//Console.WriteLine(nameof(TestSameNameHelp) + "Err:  " + claError.ToString());

            Assert.Equal(claOut.ToString(), binderOut.ToString());
            Assert.Equal(claError.ToString(), binderError.ToString());
        }

        [Fact]
        public void TestSameExecBadOption()
        {
            var claOut = new StringWriter();
            var claError = new StringWriter();

            var binderOut = new StringWriter();
            var binderError = new StringWriter();

            var cla = BuildDocSampleCLA();
            var binder = CommandLineBinding<DocSampleModel>.Build();

            SetOutErr(cla, claOut, claError);
            binder.Out = binderOut;
            binder.Error = binderError;

            cla.Execute("--foo");
            binder.Execute("--foo");

// Console.WriteLine("Out:  " + claOut.ToString());
// Console.WriteLine("Err:  " + claError.ToString());

            Assert.Equal(claOut.ToString(), binderOut.ToString());
            Assert.Equal(claError.ToString(), binderError.ToString());
        }

        [Fact]
        public void TestSameExecMultiNames()
        {
            var args = new[]
            {
                new[] { "-g", "Howdy!" },
                new[] { "-g", "Howdy!", "--uppercase" },
                new[] { "-g", "Howdy!", "--uppercase", "name", "John", "Jacob", "Jingle" },
            };

            var claOut = new StringWriter();
            var claError = new StringWriter();

            var binderOut = new StringWriter();
            var binderError = new StringWriter();

            foreach (var a in args)
            {
                var cla = BuildDocSampleCLA();
                var binder = CommandLineBinding<DocSampleModel>.Build();

                SetOutErr(cla, claOut, claError);
                binder.Out = binderOut;
                binder.Error = binderError;

                cla.Execute(a);
                binder.Execute(a);
            }

//Console.WriteLine(nameof(TestSameExecMultiNames) + "Out:  " + binderOut.ToString());
//Console.WriteLine(nameof(TestSameExecMultiNames) + "Err:  " + binderError.ToString());

            Assert.Equal(claOut.ToString(), binderOut.ToString());
            Assert.Equal(claError.ToString(), binderError.ToString());
        }

        public static CommandLineApplication BuildDocSampleCLA()
        {
            // Define the root command configuration
            var cla = new CommandLineApplication(throwOnUnexpectedArg: false);

            // Add various single-value or multi-value options or no-value options (flags)
            CommandOption greeting = cla.Option("-$|-g|--greeting <greeting>",
                    "The greeting to display.",
                    CommandOptionType.SingleValue);
            CommandOption uppercase = cla.Option("-u|--uppercase",
                    "Display the name in uppercase.",
                    CommandOptionType.NoValue);

            // Add some named arguments and/or child commands (here we combine both)
            CommandArgument names = null;
            cla.Command("name", (childCla) =>
            {
                childCla.HelpOption("--help-me");
                names = childCla.Argument(
                        "names",
                        "Enter the names of the people to be greeted.",
                        multipleValues: true);
                childCla.OnExecute(() =>
                {
                    cla.Out.WriteLine("Executing...");
                    if (greeting.HasValue())
                    {
                        cla.Out.WriteLine("Greetings to the following:");
                        foreach (var n in names.Values)
                        {
                            cla.Out.WriteLine(greeting.Value() + " "
                                    + (uppercase.HasValue() ? n.ToUpper() : n));
                        }
                    }

                    return 0;
                });
            });

            // Enable built-in support for nicely-formatted help
            cla.HelpOption("-?|-h|--help");

            // Define a handler for *resolving* the result of parsing the CLI args
            // which usually entails interpretting all the possible values and
            // combinations and invoking some action
            cla.OnExecute(() =>
            {
                cla.Out.WriteLine("Executing...");
                if (greeting.HasValue())
                {
                    cla.Out.WriteLine($"{greeting.Value()} to the {(uppercase.HasValue() ? "WORLD" : "World")}");
                }

                return 0;
            });

            // Apply the configuration to interpret the CLI args
            //cla.Execute(args);

            return cla;
        }

        public static void SetOutErr(CommandLineApplication cla, TextWriter outWriter, TextWriter errWriter)
        {
            cla.Out = outWriter;
            cla.Error = errWriter;

            foreach (var sub in cla.Commands)
                SetOutErr(sub, outWriter, errWriter);
        }
    }
}