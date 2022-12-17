using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Unity.Logging;

public static class HelperFunctions
{
    public static IEnumerator LogProcessIDEndpoint()
    {
        var processID = Process.GetCurrentProcess().Id;
        var filePath = "pid_endpoint.txt";

        if (!File.Exists(filePath))
        {
            File.CreateText(filePath).Dispose();
        }

        string output;
        if (Bootstrap.IsServer)
        {
            // Log Process ID, Instance Type and Assigned Index
            Log.Debug($"Process ID: {processID}\n" +
                      $"Instance Type: Server\n" +
                      $"Assigned Index: {Bootstrap.ServerIndex}");

            output = processID + " " + (10000 + Bootstrap.ServerIndex) + "\n";
        }
        else
        {
            // Log Process ID, Instance Type and Assigned Index
            Log.Debug($"Process ID: {processID}\n" +
                      $"Instance Type: Client\n" +
                      $"Assigned Index: {Bootstrap.ClientIndex}");

            output = processID + " " + (20000 + Bootstrap.ClientIndex) + "\n";
        }

        if (IsFileLocked(new FileInfo(filePath)))
        {
            //Log.Debug("The processID-endpoint file is locked");
            yield return null;
        }
        else
        {
            File.AppendAllText(filePath, output);
        }
    }

    private static bool IsFileLocked(FileInfo file)
    {
        try
        {
            using(FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }

        //file is not locked
        return false;
    }
}
