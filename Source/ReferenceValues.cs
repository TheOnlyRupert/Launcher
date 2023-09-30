using Launcher.Source.Json;

namespace Launcher.Source;

public static class ReferenceValues {
    /* Currently HomeControl or LEGS */
    public const string APP_NAME = "HomeControl";

    /* Currently HomeControl or LEGS-Public */
    public const string APP_VERSION_NAME = "HomeControl";

    public static string AppDirectory { get; set; }
    public static JsonSettings JsonSettingsMaster { get; set; }
}