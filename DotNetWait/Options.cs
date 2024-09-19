using CommandLine;

namespace DotNetWait;

public class Options
{
    [Option('f', "file", Required = true, HelpText = "the filename to be modified.")]
    public string File { get; set; }
    [Option('m', "mode", Required = false, Default = Mode.Break,HelpText = "Prompt: Prompt the user to attach the application to a debugger; Break: launch JIT debugger(Windows only).")]
    public Mode Mode { get; set; }
}

public enum Mode
{
    Prompt,//todo: ConsolePrompt, Timeout
    Break
}