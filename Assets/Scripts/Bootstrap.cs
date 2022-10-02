using System;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("command-line args:");
        var cliArgs = Environment.GetCommandLineArgs();
        for (int i = 0; i < cliArgs.Length; i++)
        {
            var arg = cliArgs[i];
            Debug.Log($"[{i}]: {arg}");
        }
    }
}
