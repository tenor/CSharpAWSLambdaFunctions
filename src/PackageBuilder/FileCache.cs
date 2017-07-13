using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;

namespace PackageBuilder
{
    public class FileCache : ITempCache
    {
        const string APP_DATA_FOLDER = "AppData";
        const string TMP_CACHE_FOLDER = ".tmpcache";

        string cacheFolder;
        TimeSpan expiration;
        public FileCache(IHostingEnvironment env, TimeSpan expiration)
        {
            this.cacheFolder = Path.Combine(env.ContentRootPath, APP_DATA_FOLDER, TMP_CACHE_FOLDER);
            this.expiration = expiration;
        }

        public object Get(string key)
        {
            var contents = ReadFile(key);
            TryDeleteFile(key);
            return contents;
        }

        public object Peek(string key)
        {
            return ReadFile(key);
        }

        public string Store(string value)
        {
            //Attempt to purge old files in cache folder
            bool successEnumDir = true;
            IEnumerable<string> dirList = null;
            try
            {
                dirList = Directory.EnumerateFiles(cacheFolder);
            }
            catch
            {
                successEnumDir = false;
            }

            if (successEnumDir)
            {
                foreach (var item in dirList)
                {
                    if(IsOldFile(Path.GetFileName(item)))
                    {
                        TryDeleteFile(Path.GetFileName(item));
                    }

                }
            }

            //Create tmpCache folder if it doesn't exist
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            //Save new object to disk
            string fileName = DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString("N");
            File.WriteAllText(Path.Combine(cacheFolder, fileName), value);

            return fileName;
        }

        private string ReadFile(string fileName)
        {
            return File.ReadAllText(Path.Combine(cacheFolder, fileName));
        }

        private void TryDeleteFile(string fileName)
        {
            try
            {
                File.Delete(Path.Combine(cacheFolder, fileName));
            }
            catch { }
        }

        private bool IsOldFile(string fileName)
        {
            if (!fileName.Contains("_")) return false;
            int underScorePos = fileName.IndexOf("_");
            if (underScorePos == 0) return false;
            long age;
            if (Int64.TryParse(fileName.Substring(0, underScorePos), out age ))
            {
                long tickNow = DateTime.Now.Ticks;
                if (tickNow < age) return true; //System clock was reset or something else is off -- assume file is old
                if (tickNow - age > expiration.Ticks) return true;
            }

            return false;
        }
    }
}
