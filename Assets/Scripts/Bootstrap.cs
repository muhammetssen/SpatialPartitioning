using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Logging;
using Unity.Logging.Sinks;

public class Bootstrap : MonoBehaviour
{
    public static bool IsServer = false;
    public static ushort ServerIndex = 0;
    public static ushort ClientIndex = 0;

    private void Awake()
    {
        var unixTimestampSec = DateTimeOffset.Now.ToUnixTimeSeconds();
        var processId = Process.GetCurrentProcess().Id;
        Log.Logger = new LoggerConfig()
                .MinimumLevel.Debug()
                .OutputTemplate("{Timestamp} - {Level} - {Message}")
                .WriteTo.File($"Logs/Unity-t{unixTimestampSec}-p{processId}.log", minLevel: LogLevel.Debug)
                .WriteTo.StdOut(outputTemplate: "{Timestamp} - {Level} - {Message}")
#if UNITY_EDITOR
                .WriteTo.UnityDebugLog(outputTemplate: "{Message}")
#endif // UNITY_EDITOR
                .CreateLogger();

        var cliArgs = Environment.GetCommandLineArgs();
        Log.Info($"command-line args ({cliArgs.Length}):");
        for (int i = 0; i < cliArgs.Length; i++)
        {
            var arg = cliArgs[i];

            // When -serverIndex argument is seen, look for the next value
            if (string.Compare(arg, "-serverIndex", StringComparison.Ordinal) == 0 && cliArgs.Length > i + 1)
            {
                IsServer = true;

                if (int.TryParse(cliArgs[i + 1], out int serverIndex))
                {
                    // serverIndex argument can be parsed as an integer
                    // Load the scene that this server is responsible of
                    ServerIndex = (ushort)serverIndex;
                    SceneManager.LoadScene(serverIndex + 1, LoadSceneMode.Additive);
                    Log.Info("Loaded grid " + serverIndex);
                }
                else
                {
                    // Cannot parse the argument as an integer
                    Log.Warning("-serverIndex value must be an integer");
                }
            }

            // When -clientIndex argument is seen, look for the next value
            if (string.Compare(arg, "-clientIndex", StringComparison.Ordinal) == 0 && cliArgs.Length > i + 1)
            {
                if (int.TryParse(cliArgs[i + 1], out int clientIndex))
                {
                    // clientIndex argument can be parsed as an integer
                    ClientIndex = (ushort)clientIndex;
                }
                else
                {
                    // Cannot parse the argument as an integer
                    Log.Warning("-clientIndex value must be an integer");
                }
            }
        }

        if (!IsServer)
        {
            // No serverIndex argument found, this is a client
            Log.Info("This is a client app.");
            new GameObject("ClientBehaviour").AddComponent<ClientBehaviour>();
        }
    }
}
