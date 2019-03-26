using System;
using System.Reflection;
using UnityModManagerNet;
using Harmony12;
using UnityEngine;

namespace AutoFiremanMod
{
    public class Main
    {
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // Something
            return true; // If false the mod will show an error.
        }
    }

    [HarmonyPatch(typeof(SteamLocoSimulation), "SimulateTick")]
    class SteamLocoSimulation_SimulateTick_Patch
    {
        static void Postfix(SteamLocoSimulation __instance)
        {
            // Target values for water, coal, steam
            // const float steamTarget = 9; // Not used right now but who knows, maybe I'll automate everything
            const float waterTarget = 15000;
            const float coalTarget = 54;

            // Max deviations. Larger numbers will mean more fluctuation in the boiler.
            const float waterDiff = 750;

            float steamPressure = __instance.boilerPressure.value;
            float coalLevel = __instance.coalbox.value;
            float waterLevel = __instance.boilerWater.value;
            
            if (
                coalLevel == 0 &&       // For some reason, these values are populated with real data only
                steamPressure == 0 &&   // once every 3 ticks. If they're the values above, then this is
                waterLevel == 14400     // one of those magical skipped ticks. Do nothing.
            )
            {
                return;
            }

            // Keep coal topped up
            if (coalLevel <= coalTarget)
            {
                __instance.AddCoalChunk();
            }

            // Make sure the boiler has water in it
            if (Math.Abs(waterLevel - waterTarget) > waterDiff)
            {
                SimComponent injector = __instance.injector;
                float newValue = (waterLevel < waterTarget) ? 1.0f : 0.0f;
                
                injector.SetValue(newValue);
            }
        }
    }
}

