using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PackageBuilder.Models;
using System;

namespace PackageBuilder.Controllers
{
    public class HomeController : Controller
    {
        readonly IHostingEnvironment env;
        public HomeController(IHostingEnvironment env)
        {
            this.env = env;
        }

        public IActionResult Index(string downloadKey)
        {
            var model = new HomePageViewModel();

            if (!String.IsNullOrWhiteSpace(downloadKey))
            {
                object storedObj = GetFileCache().Peek(downloadKey);
                if (storedObj != null)
                {
                    //Fetch code from stored TempData if this is a download page
                    model.Code = JsonConvert.DeserializeObject<CodePackage>((string)storedObj).Code;

                    ViewBag.DownloadKey = downloadKey;
                }
            }
            
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(string code, int? unused)
        {
            var compileResult = ScriptCompiler.Compile(code);

            if (compileResult.HasErrors)
            {
                return View(new HomePageViewModel(compileResult.Diagnostics, code));
            }

            //Store Code and Binary output in TempData and redirect to Download Page
            var key = GetFileCache().Store(JsonConvert.SerializeObject( new CodePackage { Code = code, OutputBinary = compileResult.OutputBinary }));

            return RedirectToAction("Index", new { downloadKey = key });
                
        }

        public IActionResult Download(string Id)
        {
            //Read user code from TempData
            object storedObj = GetFileCache().Get(Id);
            if (storedObj == null) return NotFound();
                
            var package = JsonConvert.DeserializeObject<CodePackage>((string)storedObj);

            //Add code to Zip Template
            var zip = new ZipPackager(env);
            var download = zip.AddCode(package);

            //Serve file
            return File(download, "application/zip", "CSharpAWSLambdaPackage.zip");
        }

        public IActionResult Error()
        {
            return View();
        }

        ITempCache GetFileCache()
        {
            return new TempDataCache(TempData);
        }
    }
}
