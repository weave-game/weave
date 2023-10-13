namespace Weave.Utils;

public static class WeaveConstants
{
    #region Lines

    public const int LineWidth = 6;
    public const int MenuLineWidth = 14;

    #endregion Lines

    #region Countdown

    public const int CountdownLength = 2;

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

    public static bool LockedGoals => false;
}