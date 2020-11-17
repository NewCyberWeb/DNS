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

        /// <summary>
        /// Gets the written JSON Data file and converts it's content back into the specified object.
        /// </summary>
        /// <typeparam name="T">The type to convert the file content back into</typeparam>
        /// <param name="path">the filename to retrieve data from</param>
        /// <returns></returns>
        public static T GetStorageFileContents<T>(string path)
        {
            if (!File.Exists(StoragePath + path)) return default;

            string file = File.ReadAllText(StoragePath + path);

            return JsonConvert.DeserializeObject<T>(file);
        }

        /// <summary>
        /// Writes an object to a specified file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <param name="path"></param>
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
