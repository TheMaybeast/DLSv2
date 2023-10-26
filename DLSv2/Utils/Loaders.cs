using DLSv2.Core;
using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DLSv2.Utils
{
    internal class Loaders
    {
        public static Dictionary<Model, DLSModel> GetAllDLSModels()
        {
            string path = @"Plugins\DLS\";
            _ = new DLSModel();
            Dictionary<Model, DLSModel> dictModels = new Dictionary<Model, DLSModel>();
            foreach (string file in Directory.EnumerateFiles(path, "*.xml"))
            {
                try
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(DLSModel));
                    StreamReader streamReader = new StreamReader(file);

                    DLSModel dlsModel = (DLSModel)mySerializer.Deserialize(streamReader);
                    streamReader.Close();

                    string name = Path.GetFileNameWithoutExtension(file);
                    ("Adding VCF: " + name).ToLog();

                    // Parses Siren Tones
                    List<string> sirenTones = dlsModel.SoundSettings.Tones.Replace(" ", "").Split(',').ToList();
                    foreach (string sirenTone in sirenTones)
                        dlsModel.SoundSettings.SirenTones.Add(sirenTone);
                    if (dlsModel.SoundSettings.SirenTones.Count < 2)
                        dlsModel.SoundSettings.SirenTones = new List<string> { "sirens_slow_dir", "fast_9mvv0vf" };

                    // Parses Vehicles
                    List<string> vehicles = dlsModel.Vehicles.Replace(" ", "").Split(',').ToList();
                    foreach (string vehicle in vehicles)
                    {
                        Model model = new Model(vehicle);
                        if (!dictModels.ContainsKey(model))
                        {
                            dictModels.Add(model, dlsModel);
                            ("Added: " + vehicle + " from " + Path.GetFileName(file)).ToLog();
                        }
                        else
                            ("WARNING: " + model + " conflicted when reading " + Path.GetFileName(file)).ToLog();
                    }

                    ("Added VCF: " + name).ToLog();
                }
                catch (Exception e)
                {
                    ("VCF IMPORT ERROR (" + Path.GetFileNameWithoutExtension(file) + "): " + e.Message).ToLog();
                    Game.LogTrivial("VCF IMPORT ERROR (" + Path.GetFileNameWithoutExtension(file) + "): " + e.Message);
                }
            }
            return dictModels;
        }
    }
}
