using EventInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModBagman.HarmonyPatches;
using System.Text;

namespace ModBagman;

/// <summary>
/// Developer console for SoG.
/// </summary>
public class DeveloperConsole : ICanHazInput
{
    private const int MaxMessageHistory = 100;
    private const int MaxCommandHistory = 50;

    private readonly Texture2D _pixel;

    private readonly List<string> _messageHistory = new();
    private readonly List<string> _commandHistory = new();
    private string _latestCommand = "";

    private bool _triggered = true;

    private const int Spacing = 2;
    private const int Border = 2;
    private const int CommandLineTickerCycle = 60;

    private Color BorderColor = Color.White * 0.75f;
    private Color TextColor = Color.White;
    private Color BackgroundColor = Color.Black * 0.75f;

    private SpriteFont ConsoleFont;

    private int LineHeight;
    private int CommandSpacing;
    private const string CommandDecorativePrefix = "$ ";

    private int _commandLineTicker = 0;
    private int _keyRepeat = 0;
    private Keys _lastKey = Keys.None;

    private int _cursor;
    private float _cursorVisualPosition;

    // I *love* side effects
    private int Cursor
    {
        get
        {
            var value = Math.Max(0, Math.Min(_cursor, _currentCommand.Length));

            if (value != _cursor)
            {
                _cursorVisualPosition = ConsoleFont.MeasureString(_currentCommand.ToString()[..value]).X;
            }

            _cursor = value;
            return value;
        }
        set
        {
            _cursor = Math.Max(0, Math.Min(value, _currentCommand.Length));
            _cursorVisualPosition = ConsoleFont.MeasureString(_currentCommand.ToString()[.._cursor]).X;
        }
    }

    private int _commandHistoryIndex = -1;
    private int _messageHistoryOffset = 0;

    private Rectangle ConsoleArea;
    private Rectangle HistoryArea;
    private Rectangle CommandArea;
    private Rectangle CommandTextBox;

    private bool _setup = false;

    private readonly StringBuilder _currentCommand = new("");

    /// <summary>
    /// Gets or sets whenver the console is active.
    /// </summary>
    private bool _active;
    internal bool Active
    {
        get => _active;
        set
        {
            _active = value;
            _framesSinceActive = -1;
        }
    }

    private int _framesSinceActive = -1;

    /// <summary>
    /// Initializes the console.
    /// Call before using methods in here.
    /// </summary>
    internal DeveloperConsole()
    {
        _pixel = new Texture2D(Globals.Game.GraphicsDevice, 1, 1);
        _pixel.SetData(new Color[] { Color.White });
    }

    private void SetupIfNeeded()
    {
        if (_setup)
            return;

        ConsoleFont = FontManager.Reg7Spacing1;

        LineHeight = ConsoleFont.LineSpacing;
        CommandSpacing = (int)ConsoleFont.MeasureString(CommandDecorativePrefix).X;

        ConsoleArea = new(0, 0, 640, 360);
        HistoryArea = new(
            ConsoleArea.Left + Spacing + Border,
            ConsoleArea.Top + Spacing + Border,
            ConsoleArea.Width - 2 * (Spacing + Border),
            ConsoleArea.Height - 2 * (Spacing + Border) - LineHeight - (2 * Spacing + Border)
            );
        CommandArea = new(
            HistoryArea.Left,
            HistoryArea.Bottom + 2 * Spacing + Border,
            HistoryArea.Width,
            LineHeight
            );
        CommandTextBox = new(
            CommandArea.Left + CommandSpacing,
            CommandArea.Top,
            CommandArea.Width - CommandSpacing,
            CommandArea.Height
            );

        _setup = true;
    }

    /// <summary>
    /// Run update logic for the console.
    /// </summary>
    internal void Update()
    {
        if (Active)
            _framesSinceActive++;

        _commandLineTicker = (_commandLineTicker + 1) % CommandLineTickerCycle;

        if (Globals.Game.xStateMaster.enGameState == StateMaster.GameStates.InGame && Globals.Game.xStolenInput != null)
            return;

        LocalInputHelper input = Globals.Game.xInput_Menu;

        if (Globals.Game.xStateMaster.enGameState == StateMaster.GameStates.InGame)
        {
            input = Globals.Game.xInput_Game;
        }

        ProcessInput(input);
    }

    /// <summary>
    /// Renders the console's contents.
    /// </summary>
    /// <param name="batch"></param>
    internal void Render(SpriteBatch batch)
    {
        if (!Active)
            return;

        SetupIfNeeded();

        // Border
        DrawBox(batch, AddPadding(HistoryArea, Spacing + Border), Border, BorderColor, BackgroundColor);
        DrawBox(batch, AddPadding(CommandArea, Spacing + Border), Border, BorderColor, BackgroundColor);

        // Some text to render
        batch.DrawString(ConsoleFont, _currentCommand, new Vector2(CommandTextBox.Left, CommandTextBox.Top), TextColor);

        batch.DrawString(ConsoleFont, CommandDecorativePrefix, new Vector2(CommandTextBox.Left - CommandSpacing, CommandTextBox.Top), Color.LightGray);

        if (_commandLineTicker < CommandLineTickerCycle / 2)
        {
            float size = ConsoleFont.MeasureString("|").X;

            batch.DrawString(ConsoleFont, "|", new Vector2(CommandTextBox.Left - size / 2 + _cursorVisualPosition, CommandTextBox.Top), Color.LightGray);
        }

        int height = HistoryArea.Bottom - (LineHeight + 1);

        foreach (var message in _messageHistory.Skip(_messageHistoryOffset))
        {
            batch.DrawString(ConsoleFont, message, new Vector2(HistoryArea.Left, height), TextColor);
            height -= LineHeight + 1;

            if (height < LineHeight)
                break;
        }
    }

    /// <summary>
    /// Adds a message to console history.
    /// </summary>
    /// <param name="text"></param>
    public void AddMessage(string text)
    {
        _messageHistory.Insert(0, text);
        if (_messageHistory.Count > MaxMessageHistory)
            _messageHistory.RemoveAt(_messageHistory.Count - 1);
    }

    internal void ClearMessages()
    {
        _messageHistory.Clear();
    }

    internal void RecordCommand(string command)
    {
        _commandHistory.Insert(0, command);
        if (_commandHistory.Count > MaxCommandHistory)
            _commandHistory.RemoveAt(_commandHistory.Count - 1);
    }

    /// <!-- nodoc -->
    public void ProcessInput(LocalInputHelper xGameInput, LocalInputHelper xMenuInput)
    {
        ProcessInput(xMenuInput);
    }

    private void ProcessInput(LocalInputHelper input)
    {
        if (input.KeyPressed(Microsoft.Xna.Framework.Input.Keys.OemTilde))
        {
            if (!_triggered)
            {
                _triggered = true;

                if (!Active && Globals.Game.xStolenInput == null)
                {
                    Globals.Game.xStolenInput = this;
                    Active = true;
                }
                else if (Active && Globals.Game.xStolenInput == this)
                {
                    Globals.Game.xStolenInput = null;
                    Active = false;
                }
            }
            return;
        }
        else
        {
            _triggered = false;
        }
    }

    private static Rectangle AddPadding(Rectangle rect, int padding)
    {
        return new Rectangle(
            rect.Left - padding,
            rect.Top - padding,
            rect.Width + 2 * padding,
            rect.Height + 2 * padding
            );
    }

    private void DrawBox(SpriteBatch batch, Rectangle rect, int borderSize, Color border, Color content)
    {
        Rectangle topEdge = new(
            rect.Left,
            rect.Top,
            rect.Width,
            borderSize
            );
        Rectangle bottomEdge = new(
            rect.Left,
            rect.Bottom - borderSize,
            rect.Width,
            borderSize
            );
        Rectangle leftEdge = new(
            rect.Left,
            rect.Top,
            borderSize,
            rect.Height
            );
        Rectangle rightEdge = new(
            rect.Right - borderSize,
            rect.Top,
            borderSize,
            rect.Height
            );

        batch.Draw(_pixel, topEdge, border);
        batch.Draw(_pixel, bottomEdge, border);
        batch.Draw(_pixel, leftEdge, border);
        batch.Draw(_pixel, rightEdge, border);
        batch.Draw(_pixel, AddPadding(rect, -borderSize), content);
    }

    internal void KeyDown(object sender, KeyEventArgs e)
    {
        if (_framesSinceActive <= 5)
            return;

        if (_lastKey != e.KeyCode)
        {
            _keyRepeat = 0;
            _lastKey = e.KeyCode;
        }

        if (e.KeyCode == Keys.Left)
        {
            Cursor--;
        }
        else if (e.KeyCode == Keys.Right)
        {
            Cursor++;
        }
        else if (e.KeyCode == Keys.Up)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                _messageHistoryOffset++;
            }
            else
            {
                if (_commandHistoryIndex == -1)
                    _latestCommand = _currentCommand.ToString();

                _commandHistoryIndex = Math.Min(_commandHistoryIndex + 1, _commandHistory.Count - 1);

                if (_commandHistoryIndex != -1)
                    _currentCommand.Clear().Append(_commandHistory[_commandHistoryIndex]);

                Cursor = _currentCommand.Length;
            }
        }
        else if (e.KeyCode == Keys.Down)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                _messageHistoryOffset = Math.Max(_messageHistoryOffset - 1, 0);
            }
            else
            {
                if (_commandHistoryIndex == -1)
                    return;

                _commandHistoryIndex = Math.Max(_commandHistoryIndex - 1, -1);

                if (_commandHistoryIndex == -1)
                {
                    _currentCommand.Clear().Append(_latestCommand);
                }
                else
                {
                    _currentCommand.Clear().Append(_commandHistory[_commandHistoryIndex]);
                }

                Cursor = _currentCommand.Length;
            }
        }
    }

    internal void CharEntered(object sender, CharacterEventArgs e)
    {
        if (_framesSinceActive <= 5)
            return;

        if (e.Character == '`')
        {
            // Ignore
        }
        else if (e.Character == '\b')
        {
            if (Cursor > 0 && _currentCommand.Length > 0)
            {
                Cursor--;
                _currentCommand.Remove(Cursor, 1);
            }
        }
        else if (e.Character == '\t')
        {
            // Ignore
        }
        else if (e.Character == '\r')
        {
            string command = _currentCommand.ToString();

            AddMessage(CommandDecorativePrefix + command);
            _currentCommand.Length = 0;
            Cursor = 0;
            _commandHistoryIndex = -1;
            _messageHistoryOffset = 0;
            _latestCommand = "";
            _commandHistory.Insert(0, command);
            ExecuteCommand(command);
        }
        else if (e.Character != '\u001b')
        {
            _currentCommand.Insert(Cursor, e.Character);
            Cursor++;
        }
    }

    private static readonly char[] Delim = new[] { ' ' };

    private void ExecuteCommand(string command)
    {
        string[] parts = command.Split(Delim, 2);

        var prev = SoG_CAS.RedirectChatToConsole;
        SoG_CAS.RedirectChatToConsole = true;
        _Chat_ParseCommand.ParseModCommands(parts[0], parts.Length > 1 ? parts[1] : "", Globals.Game.xLocalPlayer.iConnectionIdentifier);
        SoG_CAS.RedirectChatToConsole = prev;
    }
}
