using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageBuilder.Models
{
    public class CodePackage
    {
        public string Code { get; set; }
        public byte[] OutputBinary { get; set; }

    }
}
