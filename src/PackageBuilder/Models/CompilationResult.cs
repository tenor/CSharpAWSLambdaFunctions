using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace PackageBuilder.Models
{
    public class CompilationResult
    {
        public IEnumerable<Diagnostic> Diagnostics { get; set; }
        public byte[] OutputBinary { get; set; }

        public bool HasErrors
        {
            get
            {
                return Diagnostics != null && Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
            }
        }
    }
}
