namespace Weave.Utils;

public static class WeaveConstants
{
    #region Lines

    public const int LineWidth = 6;
    public const int MenuLineWidth = 14;

    #endregion Lines

    #region Countdown

    public const float CountdownLength = 2;
    public const float InitialCountdownLength = 4;

    #endregion Countdown

    #region Logging

    public const string FpsLogFileCsvPath = "./Loggings/fps.csv";
    public const string SpeedLogFileCsvPath = "./Loggings/speed.csv";
    public const string ScoreLogFileJsonPath = "./Loggings/score.json";

    #endregion Logging

    #region Inputs

    public const string GamepadJoinAction = "gamepad_join";
    public const string GamepadLeaveAction = "gamepad_leave";

    #endregion Inputs

    #region Groups

    public const string GoalGroup = "goal";
    public const string LineGroup = "line";
    public const string FireflyGroup = "firefly";
    public const string ObstacleGroup = "obstacle";

    #endregion Groups

    public static bool DevButtonsEnabled => true;

    #region URLs

    public const string SignallingServerURL = "wss://weave-signalling-server-30235e6a17df.herokuapp.com/";
    public const string STUNServerURL = "stun:stun.l.google.com:19302";
    public const string WeaveFrontendURL =  "https://weave-front-3ca0f45187f2.herokuapp.com";

    #endregion URLs
}