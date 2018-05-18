using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace RaspberryPISecurityCam.Services
{
    public static class SettingsHandler
    {
        public static T GetModelFromJSON<T>()
        {
            T model = default(T);
            var path = Path.Combine("Settings", typeof(T).ToString() + ".json");

            if (File.Exists(path))
            {
                var fileContent = File.ReadAllText(path);
                model = JsonConvert.DeserializeObject<T>(fileContent);
            }

            return model;
        }

        public static void WriteModelToJSON<T>(T model)
        {
            var path = Path.Combine("Settings", typeof(T).ToString() + ".json");
            File.WriteAllText(path, JsonConvert.SerializeObject(model));
        }

        public static string GetFilePath<T>()
        {
            return Path.Combine("Settings", typeof(T).ToString() + ".json");
        }
    }
}
