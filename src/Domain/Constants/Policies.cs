namespace Domain.Constants;

public static class Policies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireUser = "RequireUser";
    public const string RequireUserOrAdmin = "RequireUserOrAdmin";
    public const string CanManageProject = "CanManageProject";
    public const string CanManageTask = "CanManageTask";
    public const string CanManageComment = "CanManageComment";
}
