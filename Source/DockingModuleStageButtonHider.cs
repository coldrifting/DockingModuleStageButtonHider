using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DockingModuleStageButtonHider;

[KSPAddon(KSPAddon.Startup.MainMenu, true)]
public class DockingModuleStageButtonHider : MonoBehaviour
{
    public readonly HashSet<string> PartsToHideStagingButton = [];
        
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public void Start()
    {
        // Cache part names that we want to hide the enable port staging button for from the config
        foreach (var availablePart in PartLoader.LoadedPartsList)
        {
            if (availablePart.moduleInfos.Exists(s => s.moduleName == "Docking Node"))
            {
                var dockingNodeConfig = availablePart.partConfig.GetNode("MODULE", "name", "ModuleDockingNode");
                if (dockingNodeConfig != null)
                {
                    bool stagingButtonEnabled = true;
                    if (dockingNodeConfig.TryGetValue("stagingButtonEnabled", ref stagingButtonEnabled))
                    {
                        if (!stagingButtonEnabled)
                        {
                            PartsToHideStagingButton.Add(availablePart.name);
                        }
                    }
                }
            }
        }
        
        Debug.Log("DockingModuleStageButtonHider: Patching...");

        #if DEBUG
            Harmony.DEBUG = true;
        #endif
        
        Harmony harmony = new Harmony("com.coldrifting.DockingModuleStageButtonHider");
        harmony.PatchAll();

        Debug.Log("DockingModuleStageButtonHider: Patched");
    }
}

[HarmonyPatch(typeof(UIPartActionController), "CreatePartUI")]
public class DockingPortStageButtonPatcher
{
    [Conditional("DEBUG")]
    private static void Log(string msg) => Debug.Log(msg);
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void Prefix(Part part, UIPartActionWindow.DisplayType type, UI_Scene scene)
    {
        try
        {
            Log("---UIPartActionController Patch Start---");

            var storedPartNames = Object.FindObjectOfType<DockingModuleStageButtonHider>();

            HashSet<string> partNamesToHide = storedPartNames is not null ? storedPartNames.PartsToHideStagingButton : [];
            
            Log("Printing contents of Parts to Hide staging Button");
            foreach (string s in partNamesToHide)
            {
                Log(s);
            }
            
            Log($"UIPartActionController Patch: Testing part: {part.name}");
            if (partNamesToHide.Contains(part.name))
            {
                Log($"Got part: {part.name}");
                if (part.TryGetComponent(out ModuleDockingNode node))
                {
                    Log($"Got docking node");
                    var buttonEvent = node.Events["ToggleStaging"];
                    if (buttonEvent != null)
                    {
                        Log($"Got event");
                        buttonEvent.guiActive = false;
                        buttonEvent.guiActiveEditor = false;
                    }
                }
            }
            Log("---UIPartActionController Patch End---");
        }
        catch (System.Exception ex) {
            Debug.LogWarning($"Exception in patch of UIPartActionController UIPartActionController::CreatePartUI(Part part, DisplayType type, UI_Scene scene):\n{ex}");
        }
    }
}