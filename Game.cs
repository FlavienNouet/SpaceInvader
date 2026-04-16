namespace SpaceInvader;

public class Game
{
     private static readonly Rectangle PlayerShipSourceRect = new(355, 1163, 104, 64);
    private static readonly Rectangle EnemyShipSourceRect = new(390, 785, 192, 84);
    private static readonly Rectangle BunkerSourceRect = new(402, 965, 176, 128);
    private static readonly Rectangle MissileSourceRect = new(350, 864, 4, 103);
    public enum GameState
    {
        Play,
        Pause,
        Win,
        Lost
    }
    private readonly List<GameObject> objects = new();
    private readonly List<GameObject> pendingObjects = new();
     private PlayerSpaceship playerShip = null!;
    private EnemyBlock enemies = null!;

    private readonly Size gameSize;
    private bool isUpdating;
    private GameState state = GameState.Play;
    private bool pKeyWasDown;
    private bool spaceKeyWasDown;

    public IReadOnlyList<GameObject> Objects => objects;

    public PlayerSpaceship PlayerShip => playerShip;
    public Size GameSize => gameSize;

    public Game(Size clientSize)
    {
        gameSize = clientSize;
        InitializeGame();
    }

    public void AddObject(GameObject gameObject)
    {
        ArgumentNullException.ThrowIfNull(gameObject);

        if (isUpdating)
        {
            pendingObjects.Add(gameObject);
            return;
        }

        objects.Add(gameObject);
    }

    private void AddBunkers()
    {
        const int bunkerWidth = 60;
        const int bunkerHeight = 40;
        const int playerGap = 40;
        const int count = 3;

        double y = Math.Max(0, playerShip.Position.Y - bunkerHeight - playerGap);
        double availableWidth = Math.Max(0, gameSize.Width - (count * bunkerWidth));
        double gap = availableWidth / (count + 1);

        for (int i = 0; i < count; i++)
        {
            double x = gap * (i + 1) + bunkerWidth * i;
            AddObject(new Bunker(new Vecteur2d(x, y)));
        }
    }
     public void Update(double deltaTimeSeconds)
    {
        if (state == GameState.Win || state == GameState.Lost)
        {
            if (KeyPressed(Keys.Space, ref spaceKeyWasDown))
            {
                InitializeGame();
            }

            return;
        }

        if (state == GameState.Pause)
        {
            if (KeyPressed(Keys.P, ref pKeyWasDown))
            {
                state = GameState.Play;
            }

            return;
        }

        if (KeyPressed(Keys.P, ref pKeyWasDown))
        {
            state = GameState.Pause;
            return;
        }

        isUpdating = true;

        foreach (GameObject gameObject in objects)
        {
            gameObject.Update(deltaTimeSeconds);
        }

        isUpdating = false;

        if (pendingObjects.Count > 0)
        {
            objects.AddRange(pendingObjects);
            pendingObjects.Clear();
        }

        if (enemies.IsAlive() && enemies.Position.Y + enemies.Size.Height >= playerShip.Position.Y)
        {
            playerShip.Lives = 0;
        }

        if (!playerShip.IsAlive())
        {
            state = GameState.Lost;
        }
        else if (!enemies.IsAlive())
        {
            state = GameState.Win;
        }

        objects.RemoveAll(gameObject => !gameObject.IsAlive());
    }

    public void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        graphics.Clear(Color.Gray);

        foreach (GameObject gameObject in objects)
        {
            if (gameObject.IsAlive())
            {
                gameObject.Draw(graphics);
            }
        }
        string message = state switch
        {
            GameState.Pause => "Pause",
            GameState.Win => "Gagné",
            GameState.Lost => "Perdu",
            _ => "Jeu en cours"
        };

        if (state is GameState.Pause or GameState.Win or GameState.Lost)
        {
            using Font font = new(FontFamily.GenericSansSerif, 32, FontStyle.Bold, GraphicsUnit.Point);
            using StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            RectangleF bounds = new(0, 0, gameSize.Width, gameSize.Height);
            graphics.DrawString(message, font, Brushes.Black, bounds, format);
            return;
        }

        graphics.DrawString(message, SystemFonts.DefaultFont, Brushes.Black, 10f, 10f);
    }

    private void InitializeGame()
    {
        objects.Clear();
        pendingObjects.Clear();
        isUpdating = false;
        pKeyWasDown = false;
        spaceKeyWasDown = false;
        state = GameState.Play;

        Bitmap shipImage = CreatePlayerShipImage();
        Vecteur2d startPosition = new(
            (gameSize.Width - shipImage.Width) / 2.0,
            Math.Max(0, gameSize.Height - shipImage.Height - 20));

        playerShip = new PlayerSpaceship(this, startPosition, 3, shipImage, gameSize);
        objects.Add(playerShip);

        enemies = CreateEnemyBlock();
        AddObject(enemies);

        AddBunkers();
    }

     private static bool KeyPressed(Keys key, ref bool wasDown)
    {
        bool isDown = (GetAsyncKeyState((int)key) & 0x8000) != 0;
        bool pressed = isDown && !wasDown;
        wasDown = isDown;
        return pressed;
    }

     private EnemyBlock CreateEnemyBlock()
    {
        int blockWidth = Math.Max(120, (int)(gameSize.Width * 0.75));
        double startX = (gameSize.Width - blockWidth) / 2.0;

        EnemyBlock block = new EnemyBlock(new Vecteur2d(startX, 60), blockWidth, gameSize, this);
        Bitmap enemyImage = CreateEnemyShipImage();

        block.AddLine(9, 1, enemyImage);
        block.AddLine(7, 1, enemyImage);
        block.AddLine(5, 1, enemyImage);

        return block;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    private static Bitmap CreatePlayerShipImage()
    {
        return CreateSpriteFromSheet(PlayerShipSourceRect, new Size(48, 48));
    }

    private static Bitmap CreateEnemyShipImage()
    {
        return CreateSpriteFromSheet(EnemyShipSourceRect, new Size(42, 30));
    }

        internal static Bitmap CreateBunkerImage()
    {
        return CreateSpriteFromSheet(BunkerSourceRect, new Size(60, 40));
    }

        internal static Bitmap CreateMissileImage()
    {
        return CreateSpriteFromSheet(MissileSourceRect, new Size(6, 16));
    }

        private static Bitmap CreateSpriteFromSheet(Rectangle sourceRectangle, Size targetSize)
    {
        using Bitmap spriteSheet = LoadSpriteSheet();
        Bitmap bitmap = new(targetSize.Width, targetSize.Height);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        graphics.DrawImage(spriteSheet, new Rectangle(Point.Empty, targetSize), sourceRectangle, GraphicsUnit.Pixel);

        return bitmap;
    }

    private static Bitmap LoadSpriteSheet()
    {
        string sheetPath = Path.Combine(AppContext.BaseDirectory, "assets", "spaceinvaderspritesheet.png");
        return new Bitmap(Image.FromFile(sheetPath));
    }
}