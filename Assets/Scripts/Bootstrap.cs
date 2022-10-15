using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    public static bool IsServer = false;
    public static ushort ServerIndex = 0;

    private void Awake()
    {
        Debug.Log("command-line args:");
        var cliArgs = Environment.GetCommandLineArgs();
        for (int i = 0; i < cliArgs.Length; i++)
        {
            var arg = cliArgs[i];
            Debug.Log($"[{i}]: {arg}");

            // When -serverIndex argument is seen, look for the next value
            if (string.Compare(arg, "-serverIndex", StringComparison.Ordinal) == 0 && cliArgs.Length > i + 1)
            {
                IsServer = true;

                int serverIndex;
                if (int.TryParse(cliArgs[i + 1], out serverIndex))
                {
                    // serverIndex argument can be parsed as an integer
                    // Load the scene that this server is responsible of
                    ServerIndex = (ushort) serverIndex;
                    SceneManager.LoadScene(serverIndex + 1, LoadSceneMode.Additive);
                    Debug.Log("Loaded grid " + serverIndex);
                }
                else
                {
                    // Cannot parse the argument as an integer
                    Debug.LogWarning("-serverIndex value must be an integer");
                }
            }
        }

        if (!IsServer)
        {
            // No serverIndex argument found, this is a client
            Debug.Log("This is a client app.");
            new GameObject("ClientBehaviour").AddComponent<ClientBehaviour>();
        }
    }
}
