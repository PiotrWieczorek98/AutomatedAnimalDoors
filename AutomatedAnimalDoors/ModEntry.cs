using GenericModConfigMenu;
using SMAPIAutomatedDoors;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace AutomatedAnimalDoors
{
    internal sealed class ModEntry : Mod
    {
        private ModConfig Config = new();
        private bool openDoorsEventFired;
        private bool closeDoorsEventFired;
        private enum DoorAction
        {
            OPEN, CLOSE, TOGGLE
        }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Open doors time",
                tooltip: () => "In Stardew's time format. 6:00 am is 600, 1:30 pm is 1330, 3:50 pm is 1550, 1:00 am (before sleeping) is 2500",
                getValue: () => Config.OpenDoorsTime,
                setValue: value => Config.OpenDoorsTime = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Close doors time",
                tooltip: () => "In Stardew's time format. 6:00 am is 600, 1:30 pm is 1330, 3:50 pm is 1550, 1:00 am (before sleeping) is 2500",
                getValue: () => Config.CloseDoorsTime,
                setValue: value => Config.CloseDoorsTime = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Use for barns",
                getValue: () => Config.UseForBarn,
                setValue: value => Config.UseForBarn = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Use for coops",
                getValue: () => Config.UseForCoop,
                setValue: value => Config.UseForCoop = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Open doors during winter",
                getValue: () => Config.OpenDuringWinter,
                setValue: value => Config.OpenDuringWinter = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Open doors on rainy days",
                getValue: () => Config.OpenOnRainyDays,
                setValue: value => Config.OpenOnRainyDays = value
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => "Toggle doors",
                getValue: () => Config.ToggleDoorsKey,
                setValue: value => Config.ToggleDoorsKey = value
            );
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!Game1.hasLoadedGame || !Context.IsMainPlayer)
            {
                return;
            }

            if (!openDoorsEventFired &&
                e.NewTime >= Config.OpenDoorsTime &&
                (!Game1.isRaining || Config.OpenOnRainyDays) &&
                (!Game1.IsWinter || Config.OpenDuringWinter))
            {
                ChangeDoorsStatus(DoorAction.OPEN);
                openDoorsEventFired = true;
            }

            if (!closeDoorsEventFired 
                && Game1.timeOfDay >= Config.CloseDoorsTime)
            {
                ChangeDoorsStatus(DoorAction.CLOSE);
                closeDoorsEventFired = true;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Config.ToggleDoorsKey.JustPressed())
            {
                ChangeDoorsStatus(DoorAction.TOGGLE);
            }
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            // If animals didn't return before going to bed we're sending them inside
            using (List<FarmAnimal>.Enumerator enumerator = Game1.getFarm().getAllFarmAnimals().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FarmAnimal current = enumerator.Current;
                    if (!current.IsHome)
                    {
                        current.warpHome();
                    }
                }
            }

            // If gates didn't close before going to bed we're closing them now
            if (!closeDoorsEventFired)
            {
                ChangeDoorsStatus(DoorAction.CLOSE);
            }

            openDoorsEventFired = false;
            closeDoorsEventFired = false;
        }

        private void ChangeDoorsStatus(DoorAction action)
        {
            using (List<Building>.Enumerator enumerator = Game1.getFarm().buildings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Building current = enumerator.Current;
                    string name = current.GetIndoorsName();

                    if (string.IsNullOrEmpty(name) || 
                        (name.Contains("Barn") && !Config.UseForBarn) || 
                        (name.Contains("Coop") && !Config.UseForCoop))
                    {
                        continue;
                    }

                    switch (action)
                    {
                        case DoorAction.OPEN:
                        {
                            if (!current.animalDoorOpen)
                            {
                                current.ToggleAnimalDoor(Game1.player);
                            }
                            break;
                        }
                        case DoorAction.CLOSE:
                        {
                            if (current.animalDoorOpen)
                            {
                                current.ToggleAnimalDoor(Game1.player);
                            }
                            break;
                        }
                        case DoorAction.TOGGLE:
                        {
                            current.ToggleAnimalDoor(Game1.player);
                            break;
                        }
                        default: 
                            break;
                    }
                }
            }
        }
    }
}