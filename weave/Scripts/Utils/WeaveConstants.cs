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
    public const string GamepadStart = "gamepad_start";
    public const string ToggleFullscreenAction = "toggle_fullscreen";

    #endregion Inputs

    #region Groups

    public const string GoalGroup = "goal";
    public const string LineGroup = "line";
    public const string FireflyGroup = "firefly";
    public const string ObstacleGroup = "obstacle";

    #endregion Groups
}
