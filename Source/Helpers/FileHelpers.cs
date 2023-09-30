using System;
using System.IO;

namespace Launcher.Source.Helpers;

public static class FileHelpers {
    public static string LoadFileText(string fileName) {
        try {
            StreamReader streamReader = new(ReferenceValues.AppDirectory + fileName + ".json");
            string fileText = null;
            while (!streamReader.EndOfStream) {
                fileText = streamReader.ReadToEnd();
            }

            streamReader.Close();

            return fileText;
        } catch (Exception) {
            return null;
        }
    }

    public static void SaveFileText(string fileName, string fileText) {
        try {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.WriteAllText(ReferenceValues.AppDirectory + fileName + ".json", fileText);
        } catch (Exception) {
            //ignore
        }
    }
}