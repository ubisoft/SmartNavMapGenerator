using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using UnityEngine;

public abstract class OptionsInterface
{
    protected OptionSet options = new OptionSet();

    public OptionSet Options { get { return options; } }

    public abstract string Name();

    public bool ValidateAndParse(ref List<string> args)
    {
        if (!ValidateArgs(args)) return false;

        return Parse(ref args);
    }

    public abstract bool Parse(ref List<string> args);

    public abstract bool ValidateArgs(List<string> args);

    protected bool ParseFlag(string argName, string strFlag, out bool flag)
    {
        flag = strFlag == null;
        if (!flag && !bool.TryParse(strFlag, out flag))
        {
            Debug.LogError(String.Format("{0} format does not match {0}[=true|false]", argName));
            return false;
        }
        return true;
    }
}
