using System.Diagnostics;
using CommandLine;
using DotNetWait;
using Mono.Cecil;
using Mono.Cecil.Cil;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(o =>
    {
        Run(o);
    });

void Run(Options options)
{
    // Load the assembly
    using var assembly = AssemblyDefinition.ReadAssembly(options.File, new ReaderParameters{ReadWrite = true});
    
    // Find the entry point method
    var entryPoint = assembly.EntryPoint;

    // Get the IL processor for the entry point method
    var ilProcessor = entryPoint.Body.GetILProcessor();

    // Create instructions to add
    Instruction[] instructions;
    if (options.Mode == Mode.Break)
    {
        var launchDebuggerInstruction = ilProcessor.Create(OpCodes.Call, assembly.MainModule.ImportReference(
            typeof(Debugger).GetMethod(nameof(Debugger.Launch), Type.EmptyTypes)));
        instructions = new[]
        {
            launchDebuggerInstruction,
            Instruction.Create(OpCodes.Pop),
        };
    }
    else if(options.Mode== Mode.Prompt)
    {
        var writeLineInstruction = ilProcessor.Create(OpCodes.Call, assembly.MainModule.ImportReference(
            typeof(Console).GetMethod(nameof(Console.WriteLine), new Type[] { typeof(string) })));
        var readKeyInstruction = ilProcessor.Create(OpCodes.Call, assembly.MainModule.ImportReference(
            typeof(Console).GetMethod(nameof(Console.ReadKey), Type.EmptyTypes)));
        
        instructions = new[]
        {
            Instruction.Create(OpCodes.Ldstr, "Please attach debugger, then press any key"),
            writeLineInstruction,
            readKeyInstruction,
            Instruction.Create(OpCodes.Pop),
        };
    }
    else
    {
        throw new InvalidOperationException("Invalid mode");
    }
    
    var firstInstruction = ilProcessor.Body.Instructions[0];
    // Insert the new instructions at the beginning of the method
    for (int i = 0; i < instructions.Length; i++)
    {
        ilProcessor.InsertBefore(firstInstruction, instructions[i]);
    }
    
    // Save the modified assembly
    assembly.Write();

    Console.WriteLine("Assembly modified and saved successfully.");
}