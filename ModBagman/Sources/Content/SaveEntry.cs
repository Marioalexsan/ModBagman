namespace ModBagman;

[ModEntry(0)]
public class SaveEntry : Entry<CustomEntryID.SaveID>
{
    public delegate void SaveCallback(Stream stream);
    public delegate void LoadCallback(Stream stream, Version saveModVersion);

    public SaveCallback WorldSave
    {
        get => _worldSave;
        set
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            _worldSave = value;
        }
    }
    private SaveCallback _worldSave;

    internal LoadCallback WorldLoad
    {
        get => _worldLoad;
        set
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            _worldLoad = value;
        }
    }
    private LoadCallback _worldLoad;

    internal SaveCallback CharacterSave
    {
        get => _characterSave;
        set
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            _characterSave = value;
        }
    }
    private SaveCallback _characterSave;

    internal LoadCallback CharacterLoad
    {
        get => _characterLoad;
        set
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            _characterLoad = value;
        }
    }
    private LoadCallback _characterLoad;

    internal SaveCallback ArcadeSave
    {
        get => _arcadeSave;
        set
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            _arcadeSave = value;
        }
    }
    private SaveCallback _arcadeSave;

    internal LoadCallback ArcadeLoad
    {
        get => _arcadeLoad;
        set
        {
            ErrorHelper.ThrowIfNotLoading(Mod);
            _arcadeLoad = value;
        }
    }
    private LoadCallback _arcadeLoad;

    protected override void Cleanup()
    {
        // Nothing to do
    }

    protected override void Initialize()
    {
        // Nothing to do
    }
}
