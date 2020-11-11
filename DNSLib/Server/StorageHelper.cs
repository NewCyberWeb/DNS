using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib.Server
{
    internal class JsonStorageHelper
    {
        private static readonly string StoragePath = Environment.CurrentDirectory + "\\Storage\\";
        public static T GetStorageFileContents<T>(string path)
        {
            if (!File.Exists(StoragePath + path)) return default;

            string file = File.ReadAllText(StoragePath + path);

            return JsonConvert.DeserializeObject<T>(file);
        }

        public static void SaveStorageFile<T>(T file, string path)
        {
            if (!Directory.Exists(StoragePath))
            {
                Directory.CreateDirectory(StoragePath);
            }
            File.WriteAllText(StoragePath + path, JsonConvert.SerializeObject(file));
        }
    }
}
