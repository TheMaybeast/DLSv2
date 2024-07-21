using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DLSv2.Utils;

using Core;
using Conditions;

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

                // Configure default mode
                if (string.IsNullOrEmpty(dlsModel.DefaultModeName) || !dlsModel.Modes.Any(x => x.Name == dlsModel.DefaultModeName))
                {
                    dlsModel.DefaultMode = new LightMode()
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
                    };
                    // set default mode name to auto-generated
                    dlsModel.DefaultModeName = "DLS_DEFAULT_MODE";
                    // insert first so that any other triggered modes will override
                    dlsModel.Modes.Insert(0, dlsModel.DefaultMode);
                }
                else
                {
                    dlsModel.DefaultMode = dlsModel.Modes.First(m => m.Name == dlsModel.DefaultModeName);
                }

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

                        ("Added: " + vehicle + " from " + Path.GetFileName(file)).ToLog();
                    }
                    else
                        ("" + model + " conflicted when reading " + Path.GetFileName(file)).ToLog(LogLevel.ERROR);
                }

                ("Added VCF: " + name).ToLog();
            }
            catch (Exception e)
            {
                ("Failed to import VCF (" + Path.GetFileName(file) + "): " + e.Message).ToLog(LogLevel.ERROR);
            }
        }

        return registeredModels;
    }
}