namespace InternTrack.Core.Constants;

public static class TaskPriorities
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";

    public const string ValidationPattern = "^(" + Low + "|" + Medium + "|" + High + ")$";
}
