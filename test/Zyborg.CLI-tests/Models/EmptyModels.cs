namespace Zyborg.CLI.Binder.Models
{
    public class EmptyModel1
    { }


    [CommandLineApplication]
    public class EmptyModel2
    { }

    [HelpOption("-h|-?|--help")]
    public class HelpfulEmptyModel
    { }


    [CommandLineApplication(FullName = nameof(VersionedEmptyModel))]
    [VersionOption("-v|--version",
            ShortVersion = SVER,
            LongVersion = LVER)]
    public class VersionedEmptyModel
    {
        public const string SVER = "1.0.1";
        public const string LVER = "Version " + SVER + " (Snappy Turtle)";
    }

    [CommandLineApplication(FullName = nameof(DynamicallyVersionedEmptyModel))]
    [VersionOption("-v|--version")]
    public class DynamicallyVersionedEmptyModel
    {
        public const string SVER = "2.1.1";
        public const string LVER = "Version " + SVER + " (Pompous Porcupine)";

        [ShortVersionGetter]
        public string Version => SVER;

        [LongVersionGetter]
        public string FullVersion()
        {
            return LVER;
        }
    }
}