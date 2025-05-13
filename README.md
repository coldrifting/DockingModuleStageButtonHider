### Docking Module Stage Button Hider

This small KSP mod hides the set staged UI option for selected docking ports.
This may be useful if you wish to add a stagable action to a part that is also a docking port, since KSP will stage all of a parts actions at the same time.

Requirements: [Harmony](https://github.com/KSPModdingLibs/HarmonyKSP)

Below is an example of how to use the mod to disable staging for the default docking port via Module Manager.

```css
// Disable staging for the stock docking port
@PART[dockingPort2]
  {
  @MODULE[ModuleDockingNode]
  {
    stagingEnabled = False
  }
}
```
