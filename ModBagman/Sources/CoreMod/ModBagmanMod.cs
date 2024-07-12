namespace ModBagman;

using Version = System.Version;

internal partial class ModBagmanMod : Mod
{
    internal override bool IsBuiltin => true;

    public const string ModName = "ModBagman";

    public static ModBagmanMod Instance => (ModBagmanMod)ModManager.Mods.First(x => x.Name == ModName);

    public override string Name => ModName;
    public override Version Version => Globals.ModBagmanVersion;

    public override void PostLevelLoad(PostLevelLoadData data)
    {
        if (_colliderRCActive)
        {
            RenderMaster render = Globals.Game.xRenderMaster;

            render.UnregisterRenderComponenent(_colliderRC);
            render.RegisterComponent(RenderMaster.SubRenderLayer.AboveSorted, _colliderRC);
        }
    }

    public override void Load()
    {
        _colliderRC = new ColliderRenderer();
        CreateCommands().AutoAddModCommands("mod");
        CreateAudio();
        CreateNetwork();
    }

    public override void Unload()
    {
        _colliderRC = null;
    }

    private ColliderRenderer _colliderRC;

    private bool _colliderRCActive = false;

    private Entry<IDType> ResolveEntry<IDType, EntryType>(string entryId, EntryManager<IDType, EntryType> manager, bool fuzzy = false)
        where IDType : struct, Enum
        where EntryType : Entry<IDType>
    {
        string[] parts = entryId.Split(':');

        if (parts.Length != 2)
            return null;

        Mod target = fuzzy ? CommandEntry.MatchModByTarget(parts[0]) : ModManager.Mods.FirstOrDefault(x => x.Name == parts[0]);

        if (target == null)
            return null;

        return manager.Get(target, parts[1]);
    }
}
