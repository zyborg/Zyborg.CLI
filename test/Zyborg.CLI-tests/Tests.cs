using System.IO;
using Xunit;
using Zyborg.CLI.Binder.Models;

namespace Zyborg.CLI.Binder
{
    public class Tests
    {
        [Fact]
        public void TestEmptyModel1() 
        {
            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;

            var x = CommandLineBinding<EmptyModel1>.Build();
            x.Out = outWriter;
            x.Error = errWriter;
            x.Execute("-f foo --bar bar baz");
            x.Execute("-h");
            x.Execute("-v");

            Assert.Equal(string.Empty, outWriter.ToString());
            Assert.Equal(string.Empty, errWriter.ToString());
        }

        [Fact]
        public void TestEmptyModel2() 
        {
            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;

            var y = CommandLineBinding<EmptyModel2>.Build();
            y.Out = outWriter;
            y.Error = errWriter;
            y.Execute("-f foo --bar bar baz");
            y.Execute("-h");
            y.Execute("-v");

            Assert.Equal(string.Empty, outWriter.ToString());
            Assert.Equal(string.Empty, errWriter.ToString());
        }

        [Fact]
        public void TestHelpfulEmptyModel() 
        {
            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;

            var binding = CommandLineBinding<HelpfulEmptyModel>.Build();
            binding.Out = outWriter;
            binding.Error = errWriter;

            binding.Execute("-f foo --bar bar baz");
            Assert.Equal(string.Empty, outWriter.ToString());

            binding.Execute("-v");
            Assert.Equal(string.Empty, outWriter.ToString());

            binding.Execute("-h");
            Assert.True(outWriter.ToString().Contains("Usage"));
            Assert.True(outWriter.ToString().Contains("Options"));
            Assert.True(outWriter.ToString().Contains("Show help information"));

            Assert.Equal(string.Empty, errWriter.ToString());
        }

        [Fact]
        public void TestVersionedEmptyModel() 
        {
            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;

            var binding = CommandLineBinding<VersionedEmptyModel>.Build();
            binding.Out = outWriter;
            binding.Error = errWriter;
            binding.Execute("-f foo --bar bar baz");
            Assert.Equal(string.Empty, outWriter.ToString());

            binding.Execute("-h");
            Assert.Equal(string.Empty, outWriter.ToString());

            binding.Execute("--version");

            Assert.Equal($"{nameof(VersionedEmptyModel)} {VersionedEmptyModel.SVER}",
                    binding.GetFullNameAndVersion());
            Assert.Equal($"{nameof(VersionedEmptyModel)}\r\n{VersionedEmptyModel.LVER}",
                    outWriter.ToString().Trim());
            Assert.Equal(string.Empty, errWriter.ToString());
        }

        [Fact]
        public void TestDynamicallyVersionedEmptyModel() 
        {
            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;

            var binding = CommandLineBinding<DynamicallyVersionedEmptyModel>.Build();
            binding.Out = outWriter;
            binding.Error = errWriter;
            binding.Execute("-f foo --bar bar baz");
            Assert.Equal(string.Empty, outWriter.ToString());

            binding.Execute("-h");
            Assert.Equal(string.Empty, outWriter.ToString());

            binding.Execute("--version");

            Assert.Equal($"{nameof(DynamicallyVersionedEmptyModel)} {DynamicallyVersionedEmptyModel.SVER}",
                    binding.GetFullNameAndVersion());
            Assert.Equal($"{nameof(DynamicallyVersionedEmptyModel)}\r\n{DynamicallyVersionedEmptyModel.LVER}",
                    outWriter.ToString().Trim());
            Assert.Equal(string.Empty, errWriter.ToString());
        }

        [Fact]
        public void TestBasicSwitches()
        {
            var boolSwitch = "--" + nameof(BasicSwitches.ImplicitBool).ToLower();
            var boolNullSwitch = "--" + nameof(BasicSwitches.ImplicitBoolNullable).ToLower();

            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;
            var binding = CommandLineBinding<BasicSwitches>.Build();
            binding.Out = outWriter;
            binding.Error = errWriter;
            binding.Execute("--help-me");
            Assert.True(outWriter.ToString().Contains(boolSwitch));
            Assert.True(outWriter.ToString().Contains(boolNullSwitch));
            Assert.Equal(string.Empty, errWriter.ToString());

            binding = CommandLineBinding<BasicSwitches>.Build();
            binding.Execute(boolSwitch);
            Assert.True(binding.Model._impliciBoolCalled);
            Assert.True(binding.Model._impliciBoolValue);
            Assert.False(binding.Model.ImplicitBoolNullable.HasValue);

            // TODO:
            // Apparently NoValue options of the formt --opt:true or --opt:false
            // are not support contrary to my interpretation of
            //    https://msdn.microsoft.com/en-us/magazine/mt763239.aspx?f=255&MSPPError=-2147217396
            //model = CommandLineModel<BasicSwitches>.Build();
            //model.Execute(boolSwitch + "=true");
            //Assert.True(model.Instance._impliciBoolCalled);
            //Assert.False(model.Instance._impliciBoolValue);
            //Assert.False(model.Instance.ImplicitBoolNullable.HasValue);

            binding = CommandLineBinding<BasicSwitches>.Build();
            binding.Execute(boolNullSwitch);
            Assert.False(binding.Model._impliciBoolCalled);
            Assert.False(binding.Model._impliciBoolValue);
            Assert.True(binding.Model.ImplicitBoolNullable.HasValue);
            Assert.True(binding.Model.ImplicitBoolNullable.Value);
        }

        [Fact]
        public void TestBasicCommands()
        {
            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;
            var binding = CommandLineBinding<BasicCommands>.Build();
            binding.Out = outWriter;
            binding.Error = errWriter;

            binding.Execute(BasicCommands.HELP_TEMPLATE);
// Console.WriteLine("*******************************");
// Console.WriteLine("Out:  " + outWriter.ToString());
// Console.WriteLine("Err:  " + errWriter.ToString());
// Console.WriteLine("*******************************");
            Assert.True(outWriter.ToString().Contains("sub1"));
            Assert.True(outWriter.ToString().Contains("sub2"));
            Assert.True(outWriter.ToString().Contains("sub3"));
            Assert.Equal(string.Empty, errWriter.ToString());
            Assert.Null(binding.Model.Sub1);
            Assert.Null(binding.Model._sub2);
            Assert.Null(binding.Model.Sub3);

            binding = CommandLineBinding<BasicCommands>.Build();
            binding.Execute("sub1");
            Assert.False(binding.Model.Opt1);
            Assert.NotNull(binding.Model.Sub1);
            Assert.Null(binding.Model.Sub1.Url);
            Assert.Null(binding.Model._sub2);
            Assert.Null(binding.Model.Sub3);

            binding = CommandLineBinding<BasicCommands>.Build();
            binding.Execute("--opt1", "sub1");
            Assert.True(binding.Model.Opt1);
            Assert.NotNull(binding.Model.Sub1);
            Assert.Null(binding.Model.Sub1.Url);
            Assert.Null(binding.Model._sub2);
            Assert.Null(binding.Model.Sub3);

            binding = CommandLineBinding<BasicCommands>.Build();
            binding.Execute("sub1", "--url", "foo");
            Assert.NotNull(binding.Model.Sub1);
            Assert.Equal("foo", binding.Model.Sub1.Url);
            Assert.Null(binding.Model._sub2);
            Assert.Null(binding.Model.Sub3);

            binding = CommandLineBinding<BasicCommands>.Build();
            binding.Execute("sub2");
            Assert.Null(binding.Model.Sub1);
            Assert.NotNull(binding.Model._sub2);
            Assert.Null(binding.Model._sub2.Path);
            Assert.Null(binding.Model.Sub3);
        }

        [Fact]
        public void TestEmptyModel2b() 
        {
            var x = CommandLineBinding<EmptyModel2>.Build();
            x.Execute("-f", "foo", "--bar", "bar", "baz");
        }

        [Fact]
        public void TestBasicMultiOptions()
        {
            var outWriter = new StringWriter();
            var errWriter = new StringWriter();;
            var binding = CommandLineBinding<BasicMultiOptions>.Build();
            binding.Out = outWriter;
            binding.Error = errWriter;

            binding.Execute(BasicMultiOptions.HELP_TEMPLATE);
// Console.WriteLine("*******************************");
// Console.WriteLine("Out:  " + outWriter.ToString());
// Console.WriteLine("Err:  " + errWriter.ToString());
// Console.WriteLine("*******************************");

            binding = CommandLineBinding<BasicMultiOptions>.Build();
            binding.Execute("--multi1", "v1", "--multi1", "v2", "--multi1", "v3");
            Assert.Equal(new[] { "v1", "v2", "v3", }, binding.Model.Multi1);

            binding = CommandLineBinding<BasicMultiOptions>.Build();
            binding.Execute("--multi2", "v1", "--multi2", "v2", "--multi2", "v3");
            Assert.Equal(new[] { "v1", "v2", "v3", }, binding.Model._multi2Values);

            binding = CommandLineBinding<BasicMultiOptions>.Build();
            binding.Execute("--multi3", "v1", "--multi3", "v2", "--multi3", "v3");
            Assert.Equal(new[] { "v1", "v2", "v3", }, binding.Model._multi3Values);

            binding = CommandLineBinding<BasicMultiOptions>.Build();
            binding.Execute(
                    "--multi1", "v1", "--multi2", "v2", "--multi3", "v3",
                    "--multi1", "vA", "--multi2", "vB", "--multi3", "vC",
                    "--multi1", "vI", "--multi2", "vII", "--multi3", "vIII"
                    );
            Assert.Equal(new[] { "v1", "vA", "vI", }, binding.Model.Multi1);
            Assert.Equal(new[] { "v2", "vB", "vII", }, binding.Model._multi2Values);
            Assert.Equal(new[] { "v3", "vC", "vIII", }, binding.Model._multi3Values);
        }

        [Fact]
        public void TestBasicOnBind()
        {
            var b1 = CommandLineBinding<BasicOnBind1>.Build();
            Assert.NotNull(b1.Model._opt1OnBindConfig);
            Assert.NotNull(b1.Model._arg1OnBindConfig);
            Assert.NotNull(b1.Model._cmd1OnBindConfig);
        }

        [Fact]
        public void TestBasicOnInit()
        {
            var b1 = CommandLineBinding<BasicOnInit1>.Build();
            Assert.False(b1.Model._onInitInvoked);

            var b2 = CommandLineBinding<BasicOnInit2>.Build();
            Assert.True(b2.Model._onInitInvoked);
            Assert.NotNull(b2.Model._onInitParam);
        }

        [Fact]
        public void TestBasicOnExec()
        {
            var b1 = CommandLineBinding<BasicOnExec1>.Build();
            Assert.False(b1.Model._onExecInvoked);
            var r1 = b1.Execute();
            Assert.False(b1.Model._onExecInvoked);
            Assert.Equal(0, r1);

            var b2 = CommandLineBinding<BasicOnExec2>.Build();
            Assert.False(b2.Model._onExecInvoked);
            Assert.Null(b2.Model._onExecParam);
            var r2 = b2.Execute();
            Assert.True(b2.Model._onExecInvoked);
            Assert.NotNull(b2.Model._onExecParam);
            Assert.Equal(0, r2);

            var b3 = CommandLineBinding<BasicOnExec3>.Build();
            Assert.False(b3.Model._onExecInvoked);
            Assert.Null(b3.Model._onExecParam);
            var r3 = b3.Execute();
            Assert.True(b3.Model._onExecInvoked);
            Assert.NotNull(b3.Model._onExecParam);
            Assert.Equal(BasicOnExec3.RETURN_VALUE, r3);
        }

        [Fact]
        public void TestBasicRemainingArgs()
        {
            var empty = new string[0];

            var b1_1 = CommandLineBinding<BasicRemainingArgs1>.Build();
            Assert.Null(b1_1.Model.TheRest);
            b1_1.Execute();
            Assert.Equal(empty, b1_1.Model.TheRest);

            var b1_2 = CommandLineBinding<BasicRemainingArgs1>.Build();
            Assert.Null(b1_2.Model.TheRest);
            b1_2.Execute("--flag1");
            Assert.Equal(empty, b1_2.Model.TheRest);

            var b1_3 = CommandLineBinding<BasicRemainingArgs1>.Build();
            Assert.Null(b1_3.Model.TheRest);
            b1_3.Execute("--opt1", "opt1-value", "--multi1", "m1", "--multi1", "m2");
            Assert.Equal(empty, b1_3.Model.TheRest);

            var b2_1 = CommandLineBinding<BasicRemainingArgs1>.Build();
            Assert.Null(b2_1.Model.TheRest);
            b2_1.Execute("foo");
            Assert.Equal(new[] { "foo" }, b2_1.Model.TheRest);

            var b2_2 = CommandLineBinding<BasicRemainingArgs1>.Build();
            Assert.Null(b2_2.Model.TheRest);
            b2_2.Execute("foo", "bar", "baz");
            Assert.Equal(new[] { "foo", "bar", "baz" }, b2_2.Model.TheRest);
            
            var b3_1 = CommandLineBinding<BasicRemainingArgs1>.Build();
            Assert.Null(b3_1.Model.TheRest);
            b3_1.Execute("--flag1", "foo");
            Assert.Equal(new[] { "foo" }, b3_1.Model.TheRest);

            var b3_2 = CommandLineBinding<BasicRemainingArgs1>.Build();
            Assert.Null(b3_2.Model.TheRest);
            b3_2.Execute("--opt1", "opt1-value", "--multi1", "m1", "--multi1", "m2", "foo", "bar", "baz");
            Assert.Equal("opt1-value", b3_2.Model.Opt1);
            Assert.Equal(new[] { "m1", "m2" }, b3_2.Model.Multi1);
            Assert.Equal(new[] { "foo", "bar", "baz" }, b3_2.Model.TheRest);

            // This scenario is not supported (interleaved un-named args with named args)
            //var b3_3 = CommandLineBinding<BasicRemainingArgs1>.Build();
            //Assert.Null(b3_3.Model.TheRest);
            //b3_3.Execute("--opt1", "opt1-value", "foo", "--multi1", "m1", "bar", "--multi1", "m2", "baz");
            //Assert.Equal("opt1-value", b3_3.Model.Opt1);
            //Assert.Equal(new[] { "m1", "m2" }, b3_3.Model.Multi1);
            //Assert.Equal(new[] { "foo", "bar", "baz" }, b3_3.Model.TheRest);
        }
    }
}
