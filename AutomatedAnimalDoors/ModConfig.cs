using StardewModdingAPI.Utilities;

namespace SMAPIAutomatedDoors
{
    public sealed class ModConfig
    {
        public int OpenDoorsTime { get; set; } = 630;
        public int CloseDoorsTime { get; set; } = 2300;
        public bool OpenOnRainyDays { get; set; } = false;
        public bool OpenDuringWinter { get; set; } = false;
        public bool UseForBarn { get; set; } = true;
        public bool UseForCoop { get; set; } = true;
        public KeybindList ToggleDoorsKey { get; set; } = KeybindList.Parse("LeftShift + F5");
    }
}
