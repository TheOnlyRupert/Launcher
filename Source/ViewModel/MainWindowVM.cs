using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Launcher.Source.Helpers;
using Launcher.Source.Json;
using Launcher.Source.ViewModel.Base;

namespace Launcher.Source.ViewModel;

public class MainWindowVM : BaseViewModel {
    private readonly JsonVersion versionOnline;
    private string _iconImage, _textOutput, _progressBarText, _appName, _button1Text, _button2Text;
    private int _progressBarValue;
    private bool isUpdateAvailable, _isAutoUpdate;

    public MainWindowVM() {
        IconImage = "../../Resources/Images/icon.png";
        versionOnline = new JsonVersion();
        ProgressBarValue = 0;
        ProgressBarText = "Standing By";
        AppName = ReferenceValues.APP_NAME + " Launcher";

        try {
            ReferenceValues.AppDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location + "/");
            ReferenceValues.AppDirectory = ReferenceValues.AppDirectory.Substring(0, ReferenceValues.AppDirectory.Length - 12);
        } catch (Exception) {
            ReferenceValues.AppDirectory = Environment.CurrentDirectory;
        }

        /* Get Settings File */
        try {
            ReferenceValues.JsonSettingsMaster = JsonSerializer.Deserialize<JsonSettings>(FileHelpers.LoadFileText("settings"));
        } catch (Exception) {
            ReferenceValues.JsonSettingsMaster = new JsonSettings {
                isAutoUpdate = true
            };

            FileHelpers.SaveFileText("settings", JsonSerializer.Serialize(ReferenceValues.JsonSettingsMaster));
        }

        IsAutoUpdate = ReferenceValues.JsonSettingsMaster.isAutoUpdate;

        JsonVersion versionCurrent = new();

        /* First Get Online Version File */
        try {
            using WebClient client = new();
            client.Headers.Add("user-agent", "Anything");
            byte[] bytes = client.DownloadData("https://raw.githubusercontent.com/TheOnlyRupert/" + ReferenceValues.APP_VERSION_NAME + "/main/version.json");
            versionOnline = JsonSerializer.Deserialize<JsonVersion>(Encoding.UTF8.GetString(bytes));
        } catch (WebException) {
            //ignore
        } catch (Exception e) {
            Console.WriteLine(e);
        }

        /* Then Get Local Version */
        try {
            StreamReader streamReader = new(ReferenceValues.AppDirectory + "/" + ReferenceValues.APP_NAME + "/version.json");
            string fileText = null;
            while (!streamReader.EndOfStream) {
                fileText = streamReader.ReadToEnd();
            }

            streamReader.Close();
            if (fileText != null) {
                versionCurrent = JsonSerializer.Deserialize<JsonVersion>(fileText);
            }
        } catch (Exception) {
            //ignore
        }

        /* Finally Compare Versions */
        if (versionOnline.versionMajor == 0 && versionOnline.versionMinor == 0 && versionOnline.versionPatch == 0 && versionCurrent.versionMajor == 0 && versionCurrent.versionMinor == 0 &&
            versionCurrent.versionPatch == 0) {
            TextOutput = "No version of " + ReferenceValues.APP_NAME + " located. Unable to get update file. Re-install the program from the Google Drive link.";
            Button1Text = "Exit";
            Button2Text = "Force Launch";
            isUpdateAvailable = false;
        } else if (versionOnline.versionMajor == 0 && versionOnline.versionMinor == 0 && versionOnline.versionPatch == 0) {
            TextOutput = "Unable to get update file. Possibly offline?\nCurrent: v" + versionCurrent.versionMajor + "." + versionCurrent.versionMinor + "." + versionCurrent.versionPatch + "-" +
                         versionCurrent.versionBranch;
            Button1Text = "Exit";
            Button2Text = "Launch";
            isUpdateAvailable = false;
        } else if (versionCurrent.versionMajor == 0 && versionCurrent.versionMinor == 0 && versionCurrent.versionPatch == 0) {
            TextOutput = "No version file of " + ReferenceValues.APP_NAME + " located.\nCurrent: NONE\nOnline: v" + versionOnline.versionMajor + "." + versionOnline.versionMinor + "." +
                         versionOnline.versionPatch + "-" +
                         versionOnline.versionBranch;
            Button1Text = "Download and Install";
            Button2Text = "Force Launch";
            isUpdateAvailable = true;
        } else if (versionCurrent.versionMajor != versionOnline.versionMajor) {
            TextOutput = "A new major update is available to download\nCurrent: v" + versionCurrent.versionMajor + "." + versionCurrent.versionMinor + "." + versionCurrent.versionPatch + "-" +
                         versionCurrent.versionBranch + "\nUpdate: v" + versionOnline.versionMajor + "." + versionOnline.versionMinor + "." + versionOnline.versionPatch + "-" +
                         versionOnline.versionBranch;
            Button1Text = "Download and Install";
            Button2Text = "Skip Update and Launch";
            isUpdateAvailable = true;
        } else if (versionCurrent.versionMinor != versionOnline.versionMinor) {
            TextOutput = "A new minor update is available to download\nCurrent: v" + versionCurrent.versionMajor + "." + versionCurrent.versionMinor + "." + versionCurrent.versionPatch + "-" +
                         versionCurrent.versionBranch + "\nUpdate: v" + versionOnline.versionMajor + "." + versionOnline.versionMinor + "." + versionOnline.versionPatch + "-" +
                         versionOnline.versionBranch;
            Button1Text = "Download and Install";
            Button2Text = "Skip Update and Launch";
            isUpdateAvailable = true;
        } else if (versionCurrent.versionPatch != versionOnline.versionPatch) {
            TextOutput = "A new patch is available to download\nCurrent: v" + versionCurrent.versionMajor + "." + versionCurrent.versionMinor + "." + versionCurrent.versionPatch + "-" +
                         versionCurrent.versionBranch + "\nUpdate: v" + versionOnline.versionMajor + "." + versionOnline.versionMinor + "." + versionOnline.versionPatch + "-" +
                         versionOnline.versionBranch;
            Button1Text = "Download and Install";
            Button2Text = "Skip Update and Launch";
            isUpdateAvailable = true;
        } else {
            TextOutput = "LEGS is currently up to date\nCurrent: v" + versionCurrent.versionMajor + "." + versionCurrent.versionMinor + "." + versionCurrent.versionPatch + "-" +
                         versionCurrent.versionBranch;
            Button1Text = "Exit";
            Button2Text = "Launch";
            isUpdateAvailable = false;
        }

        if (IsAutoUpdate && isUpdateAvailable) {
            DownloadFile();
        }
    }

    public ICommand ButtonCommand => new DelegateCommand(ButtonCommandLogic, true);

    private void DownloadFile() {
        using WebClient client = new();
        client.DownloadFileCompleted += DownloadCompleted;
        client.DownloadProgressChanged += DownloadProgress;
        client.DownloadFileAsync(new Uri("https://github.com/TheOnlyRupert/" + ReferenceValues.APP_VERSION_NAME + "/releases/download/v" + versionOnline.versionMajor + "." +
                                         versionOnline.versionMinor + "." +
                                         versionOnline.versionPatch + "-" + versionOnline.versionBranch + "/" + ReferenceValues.APP_NAME + ".v" + versionOnline.versionMajor + "." +
                                         versionOnline.versionMinor + "." +
                                         versionOnline.versionPatch + "-" + versionOnline.versionBranch + ".zip"),
            "LEGS.v" + versionOnline.versionMajor + "." + versionOnline.versionMinor + "." + versionOnline.versionPatch + "-" + versionOnline.versionBranch + ".zip");
    }

    private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e) {
        ProgressBarValue = e.ProgressPercentage;
        ProgressBarText = "Downloading " + e.ProgressPercentage + "%";
    }

    private void ButtonCommandLogic(object param) {
        switch (param) {
        case "button1":
            if (isUpdateAvailable) {
                DownloadFile();
            } else {
                Application.Current.Shutdown();
            }

            break;
        case "button2":
            try {
                Process.Start(ReferenceValues.AppDirectory + "/" + ReferenceValues.APP_NAME + "/" + ReferenceValues.APP_NAME);
                Application.Current.Shutdown();
            } catch (Exception) {
                TextOutput = "Failed to Launch LEGS. Try re-downloading file.";
            }

            break;
        }
    }

    private void DownloadCompleted(object sender, AsyncCompletedEventArgs e) {
        string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "tempUnzip");
        string extractPath = Directory.GetCurrentDirectory();
        try {
            ZipFile.ExtractToDirectory(ReferenceValues.AppDirectory + "/" + ReferenceValues.APP_NAME + ".v" + versionOnline.versionMajor + "." + versionOnline.versionMinor + "." +
                                       versionOnline.versionPatch + "-" + versionOnline.versionBranch + ".zip", tempPath);
        } catch (Exception e1) {
            Console.WriteLine(e1);
        }

        IEnumerable<IGrouping<string, string>> files = Directory.EnumerateFiles(tempPath, "*", SearchOption.AllDirectories).GroupBy(s => Path.GetDirectoryName(s));
        foreach (IGrouping<string, string> folder in files) {
            string targetFolder = folder.Key.Replace(tempPath, extractPath);
            Directory.CreateDirectory(targetFolder);

            foreach (string file in folder) {
                string targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                if (File.Exists(targetFile)) {
                    File.Delete(targetFile);
                }

                File.Move(file, targetFile);
            }
        }

        Directory.Delete(tempPath, true);

        TextOutput = "LEGS has been updated to the most recent version.";
        Button1Text = "Exit";
        Button2Text = "Launch";
        isUpdateAvailable = false;
        ProgressBarText = "Done";
        ProgressBarValue = 0;
    }

    private void SaveSettings() {
        ReferenceValues.JsonSettingsMaster.isAutoUpdate = IsAutoUpdate;
        FileHelpers.SaveFileText("settings", JsonSerializer.Serialize(ReferenceValues.JsonSettingsMaster));
    }

    #region Fields

    public string IconImage {
        get => _iconImage;
        set {
            _iconImage = value;
            RaisePropertyChangedEvent("IconImage");
        }
    }

    public string TextOutput {
        get => _textOutput;
        set {
            _textOutput = value;
            RaisePropertyChangedEvent("TextOutput");
        }
    }

    public string ProgressBarText {
        get => _progressBarText;
        set {
            _progressBarText = value;
            RaisePropertyChangedEvent("ProgressBarText");
        }
    }

    public int ProgressBarValue {
        get => _progressBarValue;
        set {
            _progressBarValue = value;
            RaisePropertyChangedEvent("ProgressBarValue");
        }
    }

    public string AppName {
        get => _appName;
        set {
            _appName = value;
            RaisePropertyChangedEvent("AppName");
        }
    }

    public string Button1Text {
        get => _button1Text;
        set {
            _button1Text = value;
            RaisePropertyChangedEvent("Button1Text");
        }
    }

    public string Button2Text {
        get => _button2Text;
        set {
            _button2Text = value;
            RaisePropertyChangedEvent("Button2Text");
        }
    }

    public bool IsAutoUpdate {
        get => _isAutoUpdate;
        set {
            _isAutoUpdate = value;
            SaveSettings();
            RaisePropertyChangedEvent("IsAutoUpdate");
        }
    }

    #endregion
}