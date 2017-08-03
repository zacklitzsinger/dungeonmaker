/// <summary>
/// Static collection of constants for communicating with the web server.
/// </summary>
public static class WebServer {

    // WARNING: Remote server IP has been left out intentionally
    public static string REMOTE_SERVER = "http://localhost:3000";
    public static string LOCALHOST = "http://localhost:3000";
#if UNITY_EDITOR
    public static string SERVER = REMOTE_SERVER;
#else
    public static string SERVER = REMOTE_SERVER;
#endif
    public static string COOKIE = "";
}
