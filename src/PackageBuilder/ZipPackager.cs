using Microsoft.AspNetCore.Hosting;
using PackageBuilder.Models;
using System.IO;
using System.IO.Compression;

namespace PackageBuilder
{
    public class ZipPackager
    {
        readonly IHostingEnvironment env;
        const string APP_DATA_FOLDER = "AppData";
        const string ZIP_TEMPLATE_FILE = "ZipTemplate.zip";
        const string FUNCTION_ZIP_FOLDER = "DotNetFunction";
        public ZipPackager(IHostingEnvironment env)
        {
            this.env = env;
        }

        public byte[] AddCode(CodePackage pkg)
        {
            //Load ZipTemplate file into Zip Stream

            string zipFilePath = Path.Combine(env.ContentRootPath, APP_DATA_FOLDER, ZIP_TEMPLATE_FILE);

            using (MemoryStream ms = new MemoryStream())
            {
                var templateContent = File.ReadAllBytes(zipFilePath);
                ms.Write(templateContent, 0, templateContent.Length);
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Update, true))
                {
                    //Write run.csx file into zip
                    var codeEntry = archive.CreateEntry(FUNCTION_ZIP_FOLDER + @"/run.csx");
                    using (var writer = new StreamWriter(codeEntry.Open()))
                    {
                        writer.WriteLine(GetCodeFileHeader());
                        writer.WriteLine();
                        writer.Write(pkg.Code);
                    }

                    //Write DotNetFunction.dll into zip
                    var binaryEntry = archive.CreateEntry(FUNCTION_ZIP_FOLDER + @"/DotNetFunction.dll");
                    using (var binStream = binaryEntry.Open())
                    {
                        binStream.Write(pkg.OutputBinary, 0, pkg.OutputBinary.Length);
                    }
                }

                ms.Flush();
                return ms.ToArray();
            }
        }

        private string GetCodeFileHeader()
        {
            return @"/******************************************************
* This file is included to identify the code in this package.
* It is not used by the function package.
******************************************************/";

        }
    }
}
