using System.Diagnostics.CodeAnalysis;
using gli.Helpers;

namespace gli.Helpers
{
    public static class Extensions
    {
        extension<TEnum>(TEnum en) where TEnum : struct, Enum
        {
            public static TEnum[] Values => Enum.GetValues<TEnum>();
            public static string[] ValueNames => Enum.GetNames<TEnum>();

            public string? Name => Enum.GetName(en);
        }

        public static bool TryParse<T>(this string? s,
            [MaybeNullWhen(false)] out T result,
            IFormatProvider? formatProvider = null
        ) where T : IParsable<T>
            => T.TryParse(s, formatProvider, out result);

        public static T Parse<T>(this string s,
            IFormatProvider? formatProvider = null
        ) where T : IParsable<T>
            => T.Parse(s, formatProvider);

        public static bool TryParse<T>(this ReadOnlySpan<char> s,
            [MaybeNullWhen(false)] out T result,
            IFormatProvider? formatProvider = null
        ) where T : ISpanParsable<T>
            => T.TryParse(s, formatProvider, out result);

        public static T Parse<T>(this ReadOnlySpan<char> s,
            IFormatProvider? formatProvider = null
        )
            where T : ISpanParsable<T>
            => T.Parse(s, formatProvider);
    }
}

namespace CommandLine
{
    public static class ParserExt
    {
        private static readonly Lazy<Parser> LenientParser = new(() => new Parser(settings =>
        {
            settings.HelpWriter = Logger.TextWriter;
            settings.IgnoreUnknownArguments = true;
            settings.MaximumDisplayWidth -= (int)(settings.MaximumDisplayWidth * 0.175);
        }));

        private static readonly Lazy<Parser> DefaultParser = new(() => new Parser(settings =>
        {
            settings.HelpWriter = Logger.TextWriter;
            settings.IgnoreUnknownArguments = true;
            settings.MaximumDisplayWidth -= (int)(settings.MaximumDisplayWidth * 0.175);
        }));

        extension(Parser)
        {
            public static Parser LenientDefault => LenientParser.Value;
            public static Parser CustomDefault => DefaultParser.Value;
        }
    }
}