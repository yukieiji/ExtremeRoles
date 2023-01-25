﻿using AmongUs.GameOptions;
using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.RoleSelector.Ghost;
using ExtremeRoles.GameMode.RoleSelector.Normal;
using ExtremeRoles.GameMode.Vison;

// TODO: setプロパティ => initにする 

namespace ExtremeRoles.GameMode
{
    public class ExtremeGameModeManager
    {
        public static ExtremeGameModeManager Instance { get; private set; }

        public GameModes CurrentGameMode { get; }

        public IShipGlobalOption ShipOption { get; private set; }
        public IRoleSelector RoleSelector { get; private set; }

        // TODO：このクラスに含める必要があるか検証する必要あり
        public IVisonModifier Vison { get; private set; }

        public ExtremeGameModeManager(GameModes mode)
        {
            CurrentGameMode = mode;
        }

        public static void Create(GameModes mode)
        {
            GameModes currentMode = Instance?.CurrentGameMode ?? GameModes.None;

            if (currentMode == mode) { return; }

            Instance = new ExtremeGameModeManager(mode);

            IModeFactory factory = mode switch
            {
                GameModes.Normal => new ClassicGameModeOptionFactory(),
                GameModes.HideNSeek => new HideNSeekGameModeFactory(),
                _ => null,
            };

            Instance.ShipOption = factory.CreateGlobalOption();
            Instance.Vison = factory.CreateVisonModifier();
        }

        public void Load()
        {
            Instance.ShipOption.Load();
        }

        public IIntroRunner GetIntroRunner()
            => CurrentGameMode switch
            {
                GameModes.Normal => new ClassicIntroRunner(),
                GameModes.HideNSeek => new HideNSeekIntroRunner(),
                _ => null
            };
    }
}
