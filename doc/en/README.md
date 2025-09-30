### Quick Link
 - [Latest Release](https://github.com/yukieiji/ExtremeRoles/releases/latest)
 - [Wiki](https://yukieiji.github.io/ExtremeRoles.Docs/)
 - [How to translate](https://github.com/yukieiji/ExtremeRoles/tree/master?tab=readme-ov-file#how-to-translate)
<div><img src="../img/burner.png" /></div>

<div align="center"><h1>Extreme Roles, Extreme Skins and Engine Voice Engine</h1></div>

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

![AmongUs](https://img.shields.io/badge/AmongUs-v2021.12.15～v17.0.0(v2025.9.9)-green)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/yukieiji/ExtremeRoles)](https://github.com/yukieiji/ExtremeRoles/releases/latest)
[![GitHub](https://img.shields.io/github/license/yukieiji/ExtremeRoles)](https://github.com/yukieiji/ExtremeRoles/blob/master/LICENSE.md)
[![Twitter URL](https://img.shields.io/twitter/url?label=Twitter&style=social&url=https%3A%2F%2Ftwitter.com%2Fyukieiji)](https://twitter.com/yukieiji)
[![Discord](https://img.shields.io/discord/994790791304200252?label=Discord)](https://t.co/czLmgXLUBU)

---

## Wiki is here => [https://yukieiji.github.io/ExtremeRoles.Docs/](https://yukieiji.github.io/ExtremeRoles.Docs/)

---

# Extreme Roles
Main features include:
- Addition of a third faction, "Neutral," and ghost roles.
- **Over 100 unique roles** added.
    - All MOD roles can be used in conjunction with official Among Us roles.
    - All MOD roles can be assigned to multiple people.
    - HideNSeek is also fully supported, and MOD roles can be used.
- **Over 1300 options** added.
    - Detailed option settings (vision, vision effects, kill cooldown, etc.) are implemented for all roles.
    - There are also various options such as shuffle strength and engineer vent adjustments.
- Lightweight and high-speed operation.
    - **It rarely becomes heavy** due to processing (light operation has been reported even on PCs where other MODs do not work).
    - Performance optimization and weight reduction are carried out as needed using static analysis, etc.
- Advanced role assignment function using "Role Assignment Filter" and "Assignment Weight".
- MOD option import/export function.
- MOD update/downgrade/version check function.
- High compatibility with other MODs (such as **Submerged**).
    - Installation/update check/uninstallation function for compatible MODs.
    - Implementation of dedicated options with compatible MODs.
- Safe boot function that allows updating without replacement even if a problem occurs at startup.
- Multilingual support (currently only English, Japanese, and Simplified Chinese!!).
- Equipped with a REST API that can acquire a wide variety of information (if you want an API, please contact us).

## Additional Role List

- As of Extreme Roles v2025.9.24.0, more will be added in the future.

### Host Role
- Xion

### Normal Roles

|  Crewmate  |  Impostor  | Neutral |
| ---- | ---- | ---- |
|  Special Crew  |  Special Impostor  | Alice |
|  Sheriff  |  Evolver  | Jackal |
|  Maintainer  |  Carrier  | Sidekick |
|  Neet  |  Psycho Killer  | Taskmaster |
|  Watchdog  |  Bounty Hunter  | Missionary |
|  Supervisor  |  Painter  | Jester |
|  Bodyguard  |  Faker  | Yandere |
|  Whisperer  |  Overloader  | Youko |
|  Time Master  |  Cracker  | Miner |
|  Agency  |  Bomber  | Eater |
|  Baker  |  Merry  | Queen |
|  Cursemaker  |  Slavedriver  | Servant |
|  Fencer  |  Sandworm  | Totocalcio |
|  Opener |  Smasher  | Madmate |
|  Carpenter |  Assault Master  | Umbreyer |
|  Survivor |  Shooter  | Doll |
|  Captain |  Last Wolf  | Hatter |
|  Photographer |  Commander  | Artist |
|  Delusioner |  Hypnotist  | Lawbreaker |
|  Resurrector |  Underwarper  |  Tucker |
|  Gambler |  Magician  | Chimera |
|  Teleporter |  Zombie  | Ironmate |
|  Moderator |  Slime | Monika |
|  Psychic |  Thief  | Heretic  |
|  Bait  |  Crewshroom  | Shepherd |
|  Jailer |  Terrorist  | Furry |
|  Yardbird |  Raider  | Intimate |
|  Summoner |  Glitch  | Surrogate |
|  Exorcist  | Hijacker | Knight  |
|  Lower  | Timebreaker | Pawn  |
|  Merlin  | Scavenger  | Vigilante  |
|  Hero  | Boxer | Delinquent |
|  Investigator  | Assassin | Traitor |
|  Assistant  | Shares |  |
|  Apprentice Investigator  | Villain  |  |
|  Buddies  |  |  |
|  Lovers  |  |  |
|  Supporter |  |  |
|  Guesser |  |  |
|  Mover |  |  |
|  Accelerator |  |  |
|  Skater |  |  |
|  Barter |  |  |

### Ghost Roles

|  Crewmate  |  Impostor  | Neutral |
| ---- | ---- | ---- |
|  Faunus  |  Slacker  | Forus |
|  Poltergeist  |  Ventgeist  | Wisp |
|  Shutter  |  Igniter  |  |
|    |  Doppelganger  |  |

* "Neet" and "Lovers" can also be Neutral depending on the option settings (default is Crewmate).
* "Lovers", "Supporter", "Guesser", "Mover", "Accelerator", "Skater" and "Barter" can also be Impostors depending on the option settings (default is Crewmate).
* "Sidekick", "Apprentice Investigator", "Servant", "Doll", "Yardbird", "Lawbreaker", and "Chimera" are not assigned at the start of the game and are assigned when conditions are met.
* "Merlin" and "Assassin", "Hero" and "Villain" and "Vigilante", "Investigator" and "Assistant", "Delinquent" and "Wisp" are a pair.
* "Shepherd", "Furry", "Intimate", "Surrogate", "Knight", and "Pawn" are **fallback roles and are only assigned under certain conditions (such as the existence of a specific role) and settings.**
* "Shepherd", "Intimate", and "Knight" **become sub-team roles with specific settings.**
* For details, see [Roles on the Wiki](https://yukieiji.github.io/ExtremeRoles.Docs/docs/%E8%BF%BD%E5%8A%A0%E5%BD%B9%E8%81%B7/%E8%BF%BD%E5%8A%A0%E5%BD%B9%E8%81%B7.html).

# Extreme Skins
An add-on for adding cosmetics to Extreme Roles. Main features include:
- MOD update/version check function.
- In addition to normal skins, "animated skins" that can be animated have been added (only for hats and visors).
- Anyone can easily add and test hats.
- Anyone can easily add and test visors.
- Anyone can easily add and test nameplates.
- Anyone can easily test adding colors.

### If you want to make your own hat or visor, or want to publish the hat or visor you made, please contact me on Twitter etc.!!

# Extreme Voice Engine
A client add-on that adds a text-to-speech function to Extreme Roles. Main features include:
- A client add-on that works if only those who want to use it install it.
- Reading out meeting chats, etc. using synthetic voice software.
- Compatible with various synthetic voice software (each person needs to install it. Currently only VOICEVOX is supported).
   - Please check the [official website](https://voicevox.hiroshiba.jp/) etc. for the terms of use and composition of VOICEVOX.
- Easy operation with various commands.
   - Please check the Wiki for details.


# Currently confirmed bugs
- When ExtremeVoiceEngine is installed and Xion's command is used while using Xion, it displays "Invalid command" even though it is working.
  - This bug is a display problem that occurs because ExtremeVoiceEngine and ExtremeRoles have implemented separate command processing, so it has no effect on operation.

# Release Schedule


# Compatible versions with AmongUs and download of the latest version
- You can download the latest version from [here](https://github.com/yukieiji/ExtremeRoles/releases/latest).
- Extreme Roles v (version number) is without skins, Extreme Roles v (version number) with Extreme Skins is with skins.

|  AmongUs Version  |  Extreme Roles Version  |
| ---- | ---- |
|  v17.0.0(v2025.9.9)  | v2025.9.10.0 ～ v2025.9.24 |
|  v16.0.5(v2025.5.20)/v16.1.0(v2025.5.20)  | v14.0.0.0 ～ v2025.9.7.0 |
|  v16.0.0(v2025.3.25)/v16.0.2(v2025.3.31)  | v13.0.0.0 ～ v13.1.2.0 |
|  v2024.8.13s/v2024.8.13e/v2024.9.4s/v2024.9.4e<br>v2024.10.29s/v2024.10.29e/v2024.11.26s/v2024.11.26e  | v12.0.0.0 ～ v12.1.5.3 |
|  v2024.6.18s/v2024.6.18e  | v11.0.0.0 ～ v11.1.1.0 |
|  v2024.3.5s/v2024.3.5e  | v10.0.0.0 ～ v10.1.1.1 |
|  v2023.10.28s/v2023.10.28e  | v9.1.0.0 ～ v9.2.3.4 |
|  v2023.10.24s/v2023.10.24e  | v9.0.0.0 ～ v9.0.3.2 |
|  v2023.7.11s/v2023.7.11e/v2023.7.12s/v2023.7.12e  | v8.1.0.0 ～ v8.2.6.2 |
|  v2023.6.13s/v2023.6.13e/v2023.6.27s/v2023.6.27e  | v8.0.0.0 ～ v8.0.0.4 |
|  v2023.3.28s/v2023.3.28e  | v7.0.0.0 ～ v7.1.2.0 |
|  v2023.2.28s/v2023.2.28e  | v6.0.0.0 ～ v6.0.0.6 |
|  v2022.12.08s/v2022.12.08e/v2022.12.14s/v2022.12.14e  |  v5.0.0.0 ～ v5.1.1.1 |
|  v2022.10.25s/v2022.10.25e  |  v4.0.0.0 ～ v4.0.1.4 |
|  v2022.10.18s/v2022.10.18e  |  v3.3.0.3 ～ v3.3.0.6 |
|  v2022.08.23s/v2022.08.23e/v2022.08.24s/v2022.08.24e/v2022.09.20s/v2022.09.20e  |  v3.2.2.5 ～ v3.3.0.2 |
|  v2022.08.23s/v2022.08.23e/v2022.08.24s/v2022.08.24e  |  v3.2.2.0 ～ v3.2.2.4 |
|  v2022.06.21s/v2022.06.22e/v2022.07.12s/v2022.07.12e  |  v3.0.0.0 ～ v3.2.1.4 |
|  v2022.03.29s/v2022.03.29e/v2022.04.19e  |  v2.0.5.0 ～ v2.2.0.2 |
|  v2022.03.29s/v2022.03.29e  |  v1.99.90.0 ～ v2.0.4.0 |
|  v2021.12.15s/v2022.02.08s/v2022.02.23s/v2022.02.24s/v2021.12.15e/v2022.02.24e  |  v1.18.2.0 ～ v1.19.0.0 |
|  v2021.12.15s/v2022.02.08s/v2021.12.15e  |  v1.17.0.0 ～ v1.18.1.0  |
|  v2021.12.15s/v2021.12.15e  |  v1.11.1.1 ～ v1.16.1.0  |

# How to build
- If you can't build due to errors, etc., please contact us and we will respond.
- Required environment
  - VisualStudio 2022
    - If you need anything, you will be asked to install it when you open the sln.
- Environment construction
  1. Clone the repository
  2. Move the directory to the cloned directory
  3. Run "MakeEnv.bat"
- Build
  1. Open "ExtremeRoles.sln" in VisualStudio 2022 and build it.
     - If you need anything, please install it.
     - The first build will take some time because it will restore the Nuget package.
     - If the assets are not loaded properly, try building again.

# Credits & Thanks
- TheOtherRoles - We have been developing by referencing, quoting, and modifying the code of [TOR](https://github.com/Eisbison/TheOtherRoles) and [TOR-GM](https://github.com/yukinogatari/TheOtherRoles-GM) (except for the parts related to roles (options, patches, etc.)) since the time of development. It would have been impossible to develop this MOD without TOR. Also the source of ideas for Bounty Hunter, Carpenter, Shooter, and Captain.
- [Jackal and Sidekick](https://www.twitch.tv/dhalucard) - MOD created by **Dhalucard**, source of ideas for Jackal and Sidekick.
- [Sheriff-Mod](https://github.com/Woodi-dev/Among-Us-Sheriff-Mod) - MOD created by **Woodi-dev**, source of ideas for Sheriff.
- [Among-Us-Love-Couple-Mod](https://www.curseforge.com/among-us/all-mods/love-couple-mod) - MOD created by **Woodi-dev**, source of ideas for Lovers.
- [TooManyRolesMods](https://github.com/Hardel-DW/TooManyRolesMods) - MOD created by **Hardel-DW**, source of ideas for Time Master.
- [TownOfUs](https://github.com/slushiegoose/Town-Of-Us) - MOD created by **Slushiegoose**, source of ideas for Umbreyer.
- [Jester](https://github.com/Maartii/Jester) - MOD created by **Maartii**, source of ideas for Jester.
- [Goose-Goose-Duck](https://store.steampowered.com/app/1568590/Goose_Goose_Duck) - MOD created by **Slushygoose**, source of ideas for Eater.
- [PropHunt](https://github.com/ugackMiner53/PropHunt) - MOD created by **ugackMiner53**, used as a reference for the code of Slime and Mover.

- [Unity VOICEVOX Bridge](https://github.com/mikito/unity-voicevox-bridge) - A library for calling Voivo from Unity created by **mikito**, used as a reference for the Voivo part of EVE.
- [CuiCommandParser](https://github.com/oika/CuiCommandParser) - A CUI option parser library created by **oika**, used as a reference for the command line analysis of EVE.
- [UnityMainThreadDispatcher](https://github.com/PimDeWitte/UnityMainThreadDispatcher) - A library for performing main thread processing of Unity in a thread-safe manner created by **PimDeWitte**, used as a reference for the processing of the REST API.

- Resistance: Avalon - Source of ideas for Merlin and Assassin.
- Shadow Hunters - Source of ideas for Alice and Overloader.

- Microsoft.CSharp: Used to use dynamic type.
- GAHAG : https://gahag.net/ Used when creating buttons.
- Google Font Icons : https://fonts.google.com/icons ApacheLicenceV2, used with some modifications to create some icons.
- Google note emoji：https://github.com/googlefonts/noto-emoji Some ApacheLicenceV2 icons are used.

- - Sound Effect Lab：https://soundeffect-lab.info/ SE of some roles are used.
- Let's play with free sound effects：https://taira-komori.jpn.org/welcome.html SE of some roles are used.
- VOICEVOX: https://voicevox.hiroshiba.jp/ Used for some voices.
    - Credit notation
        - VOICEVOX:Zundamon


## About the button icon
The button icon image is created based on public domain material whose copyright has been waived. If you don't like it and want to replace it, please contact us.

## About multilingual support (Translation)
Multilingual support is possible, but since we are developing with implementation speed as a priority, Japanese is implemented preferentially. If you would like to translate into another language or have translated, please contact us.
EXRole can support multiple languages, but only Japanese is implemented because I prioritize the speed of implementation. Please contact me if you would like to translate into another language or if you have translated into another language.

- Language support status

|  Language Name/Languages  |  Status | Translator/Translator(Thank you!!) |
| ---- | ---- | --- |
|  English/English  |   Mostly Translated  | [yuhgao](https://github.com/yuhgao) |
|  Japanese/Japanese  |  Fully Translated  | - |
|  简体中文/SChinese  |   Fully Translated  | [ZeMingoh233](https://github.com/ZeMingoh233)<br>四个憨批汉化组([fivefirex](https://github.com/fivefirex), 123，乱线Namdam_096，氢氧则名)<br>[小鹿SAMA](https://github.com/ADeerWhoLovesEveryone) |
|  繁体中文/TChinese  |   Mostly Translated  | [FangkuaiYa](https://github.com/FangkuaiYa) |

### How to translate

ExR has been using the `ResX format translation`(XML) system since v11.1.1.0.

You can either edit [the files](https://github.com/yukieiji/ExtremeRoles/tree/master/ExtremeRoles/Translation/resx) as they are or use the [ResXResourceManager](https://github.com/dotnet/ResXResourceManager) to add translations with a few simple operations.

- Use Visual Studio 2022

  1. Clone Repository
      - ExR uses `git-flow`, so please switch to `develop` branch if possible
  3. Open `ExtremeRoles.sln` with VisualStudio
      - If VisualStudio component is missing something you need, you will be show to install it, so please follow the instructions.
  4. Open `ResX Resource Manager` in `View` to `Other Windows` of VisualStudio
      - If it's not there, install `ResXManager` from `Manage Extensions` in `Extension` and reboot VisualStudio.
  5. Add or edit translations!!

- ResXResourceManager standalone

  1. Clone Repository(Like /a)
      - ExR uses `git-flow`, so please switch to `debelop` branch if possible
  3. Download `ResXResourceManager` standalone App : [from](https://github.com/dotnet/ResXResourceManager/releases/latest)
  4. Open `ResXResourceManager.exe`
  5. Set Directory to Cloned path(Like /a/ExtremeRoles)
  6. Add or edit translations!!

#### Q＆A

- Q : Can't checkout new branch
- A : Fork ExR again and clone it, or run `git remote prune`, `git branch -d develop`(Please delete all local `develop` branches) and try again.