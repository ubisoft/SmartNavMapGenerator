using Mono.Options;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


public class CommandLine
{
    private static readonly CommandLine instance = new CommandLine();

    private Dictionary<string, CommandInterface> commands;

    private CommandSet commandSet;

    public static CommandLine Instance
    {
        get
        {
            return instance;
        }
    }

    public void Init()
    {
        commands = new Dictionary<string, CommandInterface>();
        commandSet = new CommandSet("<MapGenerator.exe|MapGenerator>", new UnityTextWriter(), new UnityTextWriter());
        CommandInterface generateCommand = new GenerateCommand();
        commands.Add(generateCommand.Name, generateCommand);

        CommandInterface changeGoalsCommand = new ChangeGoalsCommand();
        commands.Add(changeGoalsCommand.Name, changeGoalsCommand);

        CommandInterface versionCommand = new VersionCommand();
        commands.Add(versionCommand.Name, versionCommand);
    }

    public void AddOptions(string command, string name, OptionsInterface cli)
    {
        commands[command].AddOptions(name, cli);
    }

    public CommandInterface ParseCommand(ref List<string> args)
    {
        int indexGenerationArgs = args.FindIndex(0, (string arg) => arg == "--");
        if (indexGenerationArgs == -1)
            return null;
        args.RemoveRange(0, indexGenerationArgs + 1);

        CommandInterface command = null;
        foreach (var pair in commands)
        {
            pair.Value.Run = (IEnumerable<string> args) => command = pair.Value;
            commandSet.Add(
                pair.Value
            );
        }

        int returnValue = commandSet.Run(args);
        if (returnValue > 0)
            return null;

        args.RemoveAt(0);
        return command;
    }

    public IEnumerator Coroutine_Execute(string[] argsArray)
    {
        CommandInterface command;
        try
        {
            Init();

            List<string> args = new List<string>(argsArray);
            command = CommandLine.Instance.ParseCommand(ref args);
            if (command == null)
                yield break;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            yield break;
        }

        yield return command?.Execute();
    }
}
