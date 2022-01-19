using Mono.Options;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class CommandInterface : Mono.Options.Command
{
    protected Dictionary<string, OptionsInterface> optionsDict = new Dictionary<string, OptionsInterface>();

    public CommandInterface(string name, string help = null) : base(name, help)
    {
    }

    public void AddOptions(string name, OptionsInterface options)
    {
        optionsDict[name] = options;
    }

    protected abstract void Setup();

    public override int Invoke(IEnumerable<string> arguments)
    {
        List<string> args = arguments.ToList();
        if (!SetupAndParse(ref args))
            return 1;
        Run?.Invoke(arguments);
        return 0;
    }

    public bool SetupAndParse(ref List<string> args)
    {
        bool parsedWithoutError = Parse(ref args);
        Setup();
        return parsedWithoutError;
    }

    public bool Parse(ref List<string> args)
    {
        bool parsedWithoutError = true;

        parsedWithoutError &= ParseConcrete(ref args);
        Options = new OptionSet();
        int argCount = args.Count;
        Options.Add("?|h|help", "Show this message and exit", v =>
        {
            if (argCount <= 1)
            {
                HelpMessage(optionsDict.Values.ToList());
                parsedWithoutError = false;
            }
        });
        args = Options.Parse(args);

        if (args.Count > 0)
        {
            Debug.LogWarning("Unknown parameters: " + System.String.Join(" ", args));
            parsedWithoutError = false;
        }
        return parsedWithoutError;
    }
    protected abstract bool ParseConcrete(ref List<string> args);

    public void HelpMessage()
    {
        HelpMessage(optionsDict.Values.ToList());
    }

    private void HelpMessage(List<OptionsInterface> options)
    {
        System.IO.TextWriter writer = new System.IO.StringWriter();
        writer.WriteLine(
            "Usage:\n"+
            "    " + Usage() + "\n\n" +
            "Description:\n" +
            "    " + Description() + "\n\n" +
            "Options:"); ;
        foreach (OptionsInterface cli in options)
        {
            cli.Options.WriteOptionDescriptions(writer);
        }
        Debug.Log(writer.ToString());
    }

    public abstract string Description();

    protected abstract string Usage();

    public abstract IEnumerator Execute();
}
