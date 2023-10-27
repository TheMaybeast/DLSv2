using DLSv2.Core;
using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
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
        public static List<Model> ParseVCFs()
        {
            string path = @"Plugins\DLS\";
            List<Model> registeredModels = new List<Model>();

            // Clear dicts, if exists
            ModeManager.Modes = new Dictionary<Model, Dictionary<string, Mode>>();
            ControlGroupManager.ControlGroups = new Dictionary<Model, Dictionary<string, ControlGroup>>();
            SirenController.SirenTones = new Dictionary<Model, List<Tone>>();
            SirenController.Horns = new Dictionary<Model, string>();

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

                    // Parses Control Groups
                    foreach (ControlGroup controlGroup in dlsModel.ControlGroups)
                        foreach (ModeSelection modeSelection in controlGroup.Modes)
                            modeSelection.Modes = modeSelection.ModesRaw.Split(',').ToList();

                    // Parses Vehicles
                    List<string> vehicles = dlsModel.Vehicles.Replace(" ", "").Split(',').ToList();
                    foreach (string vehicle in vehicles)
                    {
                        Model model = new Model(vehicle);
                        if (!registeredModels.Contains(model))
                        {
                            registeredModels.Add(model);
                                                        
                            // Adds Modes
                            ModeManager.Modes.Add(model, new Dictionary<string, Mode>());
                            foreach (Mode mode in dlsModel.Modes)
                                ModeManager.Modes[model].Add(mode.Name, mode);

                            // Adds Control Groups
                            ControlGroupManager.ControlGroups.Add(model, new Dictionary<string, ControlGroup>());
                            foreach (ControlGroup controlGroup in dlsModel.ControlGroups)
                                ControlGroupManager.ControlGroups[model].Add(controlGroup.Name, controlGroup);

                            // Adds Siren Tones
                            SirenController.SirenTones.Add(model, new List<Tone>());
                            foreach (Tone tone in dlsModel.SoundSettings.Tones)
                                SirenController.SirenTones[model].Add(tone);
                            SirenController.Horns.Add(model, dlsModel.SoundSettings.Horn);

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

            return registeredModels;
        }
    }
}
