using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Bender.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Concatenates the specified stings using the specified separator between each member leaving distinct only values.
        /// </summary>
        /// <param name="refString"></param>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        /// <param name="equalityComparer"></param>
        /// <param name="stringSplitOptions"></param>
        /// <returns></returns>
        public static string JoinDistinct(this string refString, char separator, IEnumerable<string> values, IEqualityComparer<string>? equalityComparer = null, StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries)
        {
            return string.Join(
                separator.ToString(CultureInfo.InvariantCulture),
                refString
                    .Split(new[] { separator }, stringSplitOptions)
                    .Union(values, equalityComparer ?? EqualityComparer<string>.Default)
                );
        }

        public static string EvaluateScriptingInjections(this string template)
        {
            return
                new Regex("<<c#(?<expression>.+)#>>", RegexOptions.Singleline)
                .Replace(template, m =>
                {
                    var script = CSharpScript.Create<object>(m.Groups["expression"].Value);
                    return script.RunAsync().Result.ReturnValue.ToString() ?? string.Empty;
                });
        }

    }
}
