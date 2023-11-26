using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watchers;

namespace ModBagman;

internal partial class ModBagmanMod : Mod
{
    [ModCommand("Help")]
    private void Help(string[] args, int connection)
    {
        Mod mod = args.Length == 0 ? this : CommandEntry.MatchModByTarget(args[0]);

        if (mod == null)
        {
            CAS.AddChatMessage($"[{Name}] Unknown mod '{args[0]}'!");
            return;
        }

        List<string> commandList = mod.GetCommands()?.Commands.Keys.ToList() ?? new List<string>();

        if (commandList.Count == 0)
        {
            CAS.AddChatMessage($"[{Name}] No commands are defined for this mod!");
            return;
        }

        string target = args.Length == 0 ? "" : $" for {mod.Name}";

        if (!string.IsNullOrEmpty(mod.GetCommands().Alias))
            target += $" (alias: {mod.GetCommands().Alias})";

        CAS.AddChatMessage($"[{Name}] Command list{target}:");

        var messages = new List<string>();
        var concated = "";
        foreach (var cmd in commandList)
        {
            if (concated.Length + cmd.Length > 40)
            {
                messages.Add(concated);
                concated = "";
            }
            concated += cmd + " ";
        }
        if (concated != "")
            messages.Add(concated);

        foreach (var line in messages)
            CAS.AddChatMessage(line);
    }

    [ModCommand("ModList")]
    private void ModList(string[] args, int connection)
    {
        CAS.AddChatMessage($"[{Name}] Mod Count: {ModManager.Mods.Count}");

        var messages = new List<string>();
        var concated = "";
        foreach (var mod in ModManager.Mods)
        {
            string name = mod.Name;
            if (concated.Length + name.Length > 40)
            {
                messages.Add(concated);
                concated = "";
            }
            concated += name + " ";
        }
        if (concated != "")
            messages.Add(concated);

        foreach (var line in messages)
            CAS.AddChatMessage(line);
    }

    [ModCommand("RenderColliders")]
    private void RenderColliders(string[] args, int connection)
    {
        if (args.Any(x => !(x == "-c" || x == "-l" || x == "-m")))
        {
            CAS.AddChatMessage($"Usage: /{ModName}:{nameof(RenderColliders)} [-c] [-l] [-m]");
            return;
        }

        _colliderRC.RenderCombat = args.Contains("-c");
        _colliderRC.RenderLevel = args.Contains("-l");
        _colliderRC.RenderMovement = args.Contains("-m");

        _colliderRCActive = _colliderRC.RenderCombat || _colliderRC.RenderLevel || _colliderRC.RenderMovement;

        RenderMaster render = Globals.Game.xRenderMaster;

        render.UnregisterRenderComponenent(_colliderRC);

        if (_colliderRCActive)
        {
            render.RegisterComponent(RenderMaster.SubRenderLayer.AboveSorted, _colliderRC);
            string msg = "Collider rendering enabled for ";
            msg += _colliderRC.RenderCombat ? "Combat, " : "";
            msg += _colliderRC.RenderLevel ? "Level, " : "";
            msg += _colliderRC.RenderMovement ? "Movement, " : "";

            msg = msg.Remove(msg.Length - 2, 2);

            CAS.AddChatMessage(msg);
        }
        else
        {
            CAS.AddChatMessage("Collider rendering disabled.");
        }
    }

    [ModCommand("Spawn")]
    private void Spawn(string[] args, int connection)
    {
        if (NetUtils.IsClient)
        {
            CAS.AddChatMessage("Can't use this command as a client!");
            return;
        }

        if (!Globals.Game.bUseDebugInRelease)
        {
            CAS.AddChatMessage("You must switch to debug mode first!");
            return;
        }

        if (args.Length < 2)
        {
            CAS.AddChatMessage($"Usage: /{ModName}:{nameof(Spawn)} <Entry Type> <Mod.NameID>:<Entry.ModID>");
            CAS.AddChatMessage("Valid Entry Types: Item, Pin");
            return;
        }

        switch (args[0])
        {
            case "Item":
                {
                    long count = 1;

                    if (args.Length > 3 || args.Length == 3 && !long.TryParse(args[2], out count))
                    {
                        CAS.AddChatMessage($"Usage: /{ModName}:{nameof(Spawn)} Item <Mod.NameID>:<Item.ModID> [amount]");
                        return;
                    }

                    if (count < 1 || count > 1000)
                    {
                        CAS.AddChatMessage($"You can only spawn between 1 and 1000 items at a time.");
                        return;
                    }

                    var entry = ResolveEntry(args[1], Entries.Items, fuzzy: true);

                    if (entry == null)
                    {
                        CAS.AddChatMessage("The mod or entry ID is invalid!");
                        return;
                    }

                    PlayerEntity player = Globals.Game.xLocalPlayer.xEntity;

                    long counter = count;
                    while (counter-- > 0)
                    {
                        Globals.Game._EntityMaster_AddItem(entry.GameID, player.xTransform.v2Pos, player.xRenderComponent.fVirtualHeight, player.xCollisionComponent.ibitCurrentColliderLayer, Utility.RandomizeVector2Direction(Globals.Game.randomInVisual));
                    }

                    CAS.AddChatMessage($"Spawned {count} items.");
                }
                return;
            case "Pin":
                {
                    if (args.Length > 2)
                    {
                        CAS.AddChatMessage($"Usage: /{ModName}:{nameof(Spawn)} Pin <Mod.NameID>:<Pin.ModID>");
                        return;
                    }

                    var entry = ResolveEntry(args[1], Entries.Pins, fuzzy: true);

                    if (entry == null)
                    {
                        CAS.AddChatMessage("The mod or entry ID is invalid!");
                        return;
                    }

                    PlayerEntity player = Globals.Game.xLocalPlayer.xEntity;

                    Globals.Game._EntityMaster_AddWatcher(new PinSpawned(entry.GameID, new Vector2(330f, 324f), player.xTransform.v2Pos));

                    CAS.AddChatMessage($"Spawned pin.");
                }
                return;
            default:
                CAS.AddChatMessage($"Usage: /{ModName}:{nameof(Spawn)} <Entry Type> <Mod.NameID>:<Entry.ModID>");
                CAS.AddChatMessage("Valid Entry Types: Item, Pin");
                return;
        }
    }

    [ModCommand("ToggleDebug")]
    private void ToggleDebugMode(string[] args, int connection)
    {
        Globals.Game.bUseDebugInRelease = !Globals.Game.bUseDebugInRelease;
        CAS.AddChatMessage("Debug mode is now " + (Globals.Game.bUseDebugInRelease ? "on" : "off"));

        if (Globals.Game.bUseDebugInRelease)
        {
            CAS.AddChatMessage("Try not to break anything, eh?");
        }
    }


    [ModCommand("PlayMusic")]
    private void PlayMusic(string[] args, int connection)
    {
        if (args.Length != 2)
        {
            CAS.AddChatMessage($"Usage: /{ModName}:{nameof(PlayMusic)} <Mod.NameID> <Music>");
            return;
        }

        AudioEntry entry = ModManager.Mods.FirstOrDefault(x => x.Name == args[0])?.GetAudio();

        if (entry == null)
        {
            CAS.AddChatMessage($"Unknown mod.");
            return;
        }

        var musicID = entry.GetMusicID(args[1]);

        if (!musicID.HasValue && entry.Mod != ModManager.Vanilla)
        {
            CAS.AddChatMessage($"Unknown music.");
            return;
        }

        CAS.AddChatMessage($"Playing music.");
        Globals.Game.xSoundSystem.PlaySong(musicID.ToString(), true);
    }

    [ModCommand("PlayEffect")]
    private void PlayEffect(string[] args, int connection)
    {
        if (args.Length != 2)
        {
            CAS.AddChatMessage($"Usage: /{ModName}:{nameof(PlayMusic)} <Mod.NameID> <Effect>");
            return;
        }

        AudioEntry entry = ModManager.Mods.FirstOrDefault(x => x.Name == args[0])?.GetAudio();

        if (entry == null)
        {
            CAS.AddChatMessage($"Unknown mod.");
            return;
        }

        var effectID = entry.GetEffectID(args[1]);

        if (!effectID.HasValue && entry.Mod != ModManager.Vanilla)
        {
            CAS.AddChatMessage($"Unknown effect.");
            return;
        }

        CAS.AddChatMessage($"Playing effect.");
        Globals.Game.xSoundSystem.PlayCue(effectID.ToString(), Globals.Game.xLocalPlayer.xEntity.xTransform.v2Pos);
    }

}
