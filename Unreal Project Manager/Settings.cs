using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Unreal_Project_Manager
{
    public class Settings
    {
        public string BasePath { get; set; }

        public static void Save(Settings settings)
        {
            var serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(settings);
            File.WriteAllText($"UPMSettings.yaml", yaml);
        }

        public static Settings Read(string infile)
        {
            string yaml = File.ReadAllText(infile);
            var deserializer = new DeserializerBuilder().Build();
            Settings setting = deserializer.Deserialize<Settings>(yaml);
            return setting;
        }
    }

    public class ProjectSettings
    {
        public List<string> ProtectedFiles { get; set; } = new List<string>();

        public static void Save(ProjectSettings settings, string outpath)
        {
            var serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(settings);
            File.WriteAllText($"{outpath}\\UPMProjectSettings.yaml", yaml);
        }

        public static ProjectSettings Read(string infile)
        {
            string yaml = File.ReadAllText(infile);
            var deserializer = new DeserializerBuilder().Build();
            ProjectSettings setting = deserializer.Deserialize<ProjectSettings>(yaml);
            return setting;
        }
    }
}
