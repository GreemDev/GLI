namespace GitLabCli.API.Helpers;

public static class QueryParameters
{
    public static (string, object) Sort(string type) => ("sort", type);
    public static (string, object) OrderBy(string ordering) => ("order_by", ordering);
}