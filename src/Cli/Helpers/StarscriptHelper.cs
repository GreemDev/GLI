using Starscript;

namespace gli.Helpers;

public static class StarscriptHelper
{
    public static readonly StarscriptHypervisor Hypervisor = StarscriptHypervisor.Create().WithStandardFunctions();

    public static StarscriptHypervisor WithStandardFunctions(this StarscriptHypervisor hv) =>
        hv.WithStandardLibrary().WithStandardLibraryHttp()
            .Set("getAgeOf", GetAgeOf);

    public static Value GetAgeOf(StarscriptFunctionContext ctx)
    {
        ctx.Constrain(Constraint.ExactlyThreeArguments);
        var year = ctx.NextNumber(3);
        var month = ctx.NextNumber(2);
        var day = ctx.NextNumber(1);

        var birthDate = new DateTime(year.TruncateToInt(), month.TruncateToInt(), day.TruncateToInt());

        var age = DateTime.Now - birthDate;

        return (age.TotalDays / 365.25).TruncateToInt();
    }
}