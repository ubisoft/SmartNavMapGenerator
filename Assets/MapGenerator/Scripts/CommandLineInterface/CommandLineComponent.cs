using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class CommandLineComponent : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Execute());
    }

    private IEnumerator Execute()
    {
        Debug.Log("******* Map Generator Logs *******");
        if (Application.isBatchMode)
            yield return CommandLine.Instance.Coroutine_Execute(System.Environment.GetCommandLineArgs());
        else
            yield return CommandLine.Instance.Coroutine_Execute(@"-- generate --name test --spawn-goals 6 --hard-floor-weight 2 --grounds".Split());
        Debug.Log("**********************************");
        Application.Quit();
    }
}
