using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace PackageBuilder.Models
{
    public class HomePageViewModel
    {
        public HomePageViewModel()
        {
            Code = GetDefaultCode();
        }

        public HomePageViewModel(IEnumerable<Diagnostic> diagnostics, string code)
        {
            Code = code;
            Errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => {
                var span = d.Location.GetLineSpan().StartLinePosition;
                return "(" + (span.Line + 1) + "," + (span.Character + 1) + "): " + d.GetMessage();
                }
            );
        }

        public bool IsDownloadPage { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public string Code { get; set; }

        static string GetDefaultCode()
        {
            return @"using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

public static async Task&lt;HttpResponseMessage&gt; Run(HttpRequestMessage req)
{
    return new HttpResponseMessage
    {
        Content = new StringContent(
            JsonConvert.SerializeObject(new { greeting = ""Hello World!""})
            )
    };
}";
        }

    }
}
