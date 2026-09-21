namespace InternTrack.Core.Constants;

public static class TaskStatuses
{
    public const string ToDo = "ToDo";
    public const string InProgress = "InProgress";
    public const string Done = "Done";

    public const string ValidationPattern = "^(" + ToDo + "|" + InProgress + "|" + Done + ")$";
}
