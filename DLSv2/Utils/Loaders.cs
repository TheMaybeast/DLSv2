using DLSv2.Core;
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
            var path = @"Plugins\DLS\";
            var registeredModels = new Dictionary<Model, DLSModel>();

            var attrOverrides = new XmlAttributeOverrides();
            GroupConditions.AddCustomAttributes(attrOverrides);

            var dlsSerializer = new XmlSerializer(typeof(DLSModel), attrOverrides);

            foreach (var file in Directory.EnumerateFiles(path, "*.xml"))
            {
                try
                {
                    DLSModel dlsModel;
                    using (var reader = new StreamReader(file))
                    {
                        dlsModel = (DLSModel)dlsSerializer.Deserialize(reader);
                    }

                    var name = Path.GetFileNameWithoutExtension(file);
                    ("Adding VCF: " + name).ToLog();

                    // Parses Vehicles
                    var vehicles = dlsModel.Vehicles.Split(',').Select(s => s.Trim()).ToList();
                    foreach (var vehicle in vehicles)
                    {
                        var model = new Model(vehicle);
                        if (!registeredModels.TryGetValue(model, out _))
                        {
                            registeredModels.Add(model, dlsModel);

                            // Add V2V Sync Config
                            SyncManager.AddSyncGroup(model, dlsModel.SyncGroup);

                            // Add speed multiplier drift
                            SyncManager.AddDriftRange(model, dlsModel.DriftRange);

                            // Adds Light Modes
                            if (string.IsNullOrEmpty(dlsModel.DefaultMode))
                            {
                                dlsModel.Modes.Add(new LightMode()
                                {
                                    Name = "DLS_DEFAULT_MODE",
                                    ApplyDefaultSirenSettings = true,
                                    Yield = new Yield() { Enabled = true },
                                    Requirements = new AllCondition(new List<BaseCondition>()
                                    {
                                        new VehicleOwnerCondition()
                                        {
                                            IsPlayerVehicle = false
                                        }
                                    })
                                });
                            }

                            ("Added: " + vehicle + " from " + Path.GetFileName(file)).ToLog();
                        }
                        else
                            ("" + model + " conflicted when reading " + Path.GetFileName(file)).ToLog(LogLevel.ERROR);
                    }

                    ("Added VCF: " + name).ToLog();
                }
                catch (Exception e)
                {
                    ("Failed to import VCF (" + Path.GetFileNameWithoutExtension(file) + "): " + e.ToString()).ToLog(LogLevel.ERROR);
                }
            }

            return registeredModels;
        }
    }
}
