using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;

using Newtonsoft.Json;

using ExtremeSkins.Module;

namespace ExtremeSkins
{
    public class ExtremeHatManager
    {
        public static Dictionary<string, CustomHat> HatData = new Dictionary<string, CustomHat>();

        public const string FolderPath = @"\ExtremeHat\";
        public const string InfoFileName = "info.json";

        private class HatInfo
        {
            public string Author { get; set; }
            public string Name { get; set; }
            public bool FrontFlip { get; set; }
            public bool Back { get; set; }
            public bool BackFlip { get; set; }
            public bool Climb { get; set; }
            public bool Bound { get; set; }
            public bool Shader { get; set; }
        }


        public static void Initialize()
        {
            HatData.Clear();
        }

        public static void CheckUpdate()
        {

        }

        public static void Load()
        {
            string[] hatsFolder = Directory.GetDirectories(
                string.Concat(Path.GetDirectoryName(Application.dataPath), FolderPath));

            foreach (string hat in hatsFolder)
            {
                if (!string.IsNullOrEmpty(hat))
                {
                    HatInfo info = JsonConvert.DeserializeObject<HatInfo>(
                        string.Concat(hat, InfoFileName));

                    HatData.Add(
                        info.Name,
                        new CustomHat(
                            hat, info.Author, info.Name, info.FrontFlip,
                            info.Back, info.BackFlip, info.Climb, info.Shader, info.Bound));
                }
            }


        }
        private static void downLoad()
        {

        }
    }
}
