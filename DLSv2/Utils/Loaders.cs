﻿using DLSv2.Core;
using DLSv2.Core.Lights;
using DLSv2.Core.Sound;
using DLSv2.Threads;
using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DLSv2.Conditions;

namespace DLSv2.Utils
{
    internal class Loaders
    {
        public static Dictionary<Model, DLSModel> ParseVCFs()
        {
            string path = @"Plugins\DLS\";
            Dictionary<Model, DLSModel> registeredModels = new Dictionary<Model, DLSModel>();

            // Clear dictionaries, if exists
            ModeManager.Modes = new Dictionary<Model, Dictionary<string, Mode>>();
            ControlGroupManager.ControlGroups = new Dictionary<Model, Dictionary<string, ControlGroup>>();
            AudioControlGroupManager.ControlGroups = new Dictionary<Model, Dictionary<string, AudioControlGroup>>();
            ControlsManager.ClearInputs();
            PlayerManager.registeredKeys = false;

            XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();
            GroupConditions.AddCustomAttributes(attrOverrides);

            XmlSerializer dlsSerializer = new XmlSerializer(typeof(DLSModel), attrOverrides);

            foreach (string file in Directory.EnumerateFiles(path, "*.xml"))
            {
                try
                {
                    DLSModel dlsModel;
                    using (StreamReader reader = new StreamReader(file))
                    {
                        dlsModel = (DLSModel)dlsSerializer.Deserialize(reader);
                    }

                    string name = Path.GetFileNameWithoutExtension(file);
                    ("Adding VCF: " + name).ToLog();

                    // Parses Vehicles
                    List<string> vehicles = dlsModel.Vehicles.Split(',').Select(s => s.Trim()).ToList();
                    foreach (string vehicle in vehicles)
                    {
                        Model model = new Model(vehicle);
                        if (!registeredModels.TryGetValue(model, out _))
                        {
                            registeredModels.Add(model, dlsModel);

                            // Add V2V Sync Config
                            SyncManager.AddSyncGroup(model, dlsModel.SyncGroup);

                            // Add speed multiplier drift
                            SyncManager.AddDriftRange(model, dlsModel.DriftRange);

                            // Adds Light Modes
                            ModeManager.Modes.Add(model, new Dictionary<string, Mode>());
                            if (string.IsNullOrEmpty(dlsModel.DefaultMode))
                            {
                                ModeManager.Modes[model].Add("DLS_DEFAULT_MODE", new Mode()
                                {
                                    Name = "DLS_DEFAULT_MODE",
                                    ApplyDefaultSirenSettings = true,
                                    Yield = new Yield()
                                    {
                                        Enabled = true
                                    },
                                    Requirements = new AllCondition(new List<BaseCondition>()
                                    {
                                        new VehicleOwnerCondition()
                                        {
                                            IsPlayerVehicle = false
                                        }
                                    })
                                });
                            }
                            foreach (Mode mode in dlsModel.Modes)
                                ModeManager.Modes[model].Add(mode.Name, mode);

                            // Adds Light Control Groups
                            ControlGroupManager.ControlGroups.Add(model, new Dictionary<string, ControlGroup>());
                            foreach (ControlGroup controlGroup in dlsModel.ControlGroups)
                                ControlGroupManager.ControlGroups[model].Add(controlGroup.Name, controlGroup);

                            // Adds Audio Modes
                            AudioModeManager.Modes.Add(model, new Dictionary<string, AudioMode>());
                            foreach (AudioMode mode in dlsModel.AudioSettings.AudioModes)
                                AudioModeManager.Modes[model].Add(mode.Name, mode);

                            // Adds Audio Control Groups
                            AudioControlGroupManager.ControlGroups.Add(model, new Dictionary<string, AudioControlGroup>());
                            foreach (AudioControlGroup controlGroup in dlsModel.AudioSettings.AudioControlGroups)
                                AudioControlGroupManager.ControlGroups[model].Add(controlGroup.Name, controlGroup);

                            ("Added: " + vehicle + " from " + Path.GetFileName(file)).ToLog();
                        }
                        else
                            ("WARNING: " + model + " conflicted when reading " + Path.GetFileName(file)).ToLog();
                    }

                    ("Added VCF: " + name).ToLog();
                }
                catch (Exception e)
                {
                    ("VCF IMPORT ERROR (" + Path.GetFileNameWithoutExtension(file) + "): " + e.ToString()).ToLog(true);
                }
            }

            return registeredModels;
        }
    }
}
