using Launcher.Source.Json;

namespace Launcher.Source;

public static class ReferenceValues {
    /* Currently HomeControl or LEGS */
    public const string APP_NAME = "HomeControl";
    //public const string APP_NAME = "LEGS";

    /* Currently HomeControl or LEGS-Public */
    public const string APP_VERSION_NAME = "HomeControl";
    //public const string APP_VERSION_NAME = "LEGS-Public";

    public static string AppDirectory { get; set; }
    public static JsonSettings JsonSettingsMaster { get; set; }
}