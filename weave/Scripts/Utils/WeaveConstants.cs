using Godot;

namespace Weave.Utils;

public static class WeaveConstants
{
    public static bool DevButtonsEnabled => false;

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
    public const string DifficultyLogFileCsvPath = "./Loggings/difficulty.csv";

    #endregion Logging

    #region Inputs

    public const string GamepadJoinAction = "gamepad_join";
    public const string GamepadLeaveAction = "gamepad_leave";
    public const string ToggleFullscreenAction = "toggle_fullscreen";

    #endregion Inputs

    #region Groups

    public const string GoalGroup = "goal";
    public const string LineGroup = "line";
    public const string FireflyGroup = "firefly";
    public const string ObstacleGroup = "obstacle";

    #endregion Groups

    #region URLs

    public const string SignallingServerUrl = "wss://weave-signalling-server-30235e6a17df.herokuapp.com/";
    public const string StunServerUrl = "stun:stun.l.google.com:19302";
    public const string WeaveFrontendUrl = "https://weave-front-3ca0f45187f2.herokuapp.com";

    #endregion URLs

}
