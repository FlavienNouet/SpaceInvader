namespace SpaceInvader;

public class Game
{
     private static readonly Rectangle PlayerShipSourceRect = new(355, 1163, 104, 64);
    private static readonly Rectangle BunkerSourceRect = new(402, 965, 176, 128);
    private static readonly Rectangle MissileSourceRect = new(350, 864, 4, 103);
    private static System.Media.SoundPlayer? shootSound;
    private static System.Media.SoundPlayer? invaderKilledSound;
    private static bool backgroundMusicStarted;
    private const string BackgroundMusicAlias = "bgm_track";
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
        StartBackgroundMusic();
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
         // Load animation frames for each enemy line
        Bitmap frameA1 = ScaleImage(LoadEnemyAnimationFrame("space__0000_A1"), 2.0);
        Bitmap frameA2 = ScaleImage(LoadEnemyAnimationFrame("space__0001_A2"), 2.0);
        Bitmap frameB1 = ScaleImage(LoadEnemyAnimationFrame("space__0002_B1"), 2.0);
        Bitmap frameB2 = ScaleImage(LoadEnemyAnimationFrame("space__0003_B2"), 2.0);
        Bitmap frameC1 = ScaleImage(LoadEnemyAnimationFrame("space__0004_C1"), 2.0);
        Bitmap frameC2 = ScaleImage(LoadEnemyAnimationFrame("space__0005_C2"), 2.0);

        // Add enemy lines with appropriate animation frames
        block.AddLine(9, 1, frameA1, frameA2);  // Back line: A1 and A2
        block.AddLine(7, 1, frameB1, frameB2);  // Middle line: B1 and B2
        block.AddLine(5, 1, frameC1, frameC2);  // Front line: C1 and C2

        return block;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

     [System.Runtime.InteropServices.DllImport("winmm.dll")]
    private static extern int mciSendString(string command, System.Text.StringBuilder? returnValue, int returnLength, IntPtr winHandle);

    internal static void StartBackgroundMusic()
    {
        if (backgroundMusicStarted)
        {
            return;
        }

        string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Audio", "spaceinvaders1.mpeg");
        if (!File.Exists(filePath))
        {
            return;
        }

        // Ensure stale alias from previous sessions is cleared.
        mciSendString($"close {BackgroundMusicAlias}", null, 0, IntPtr.Zero);

        int openResult = mciSendString($"open \"{filePath}\" type mpegvideo alias {BackgroundMusicAlias}", null, 0, IntPtr.Zero);
        if (openResult != 0)
        {
            return;
        }

        int playResult = mciSendString($"play {BackgroundMusicAlias} repeat", null, 0, IntPtr.Zero);
        if (playResult == 0)
        {
            backgroundMusicStarted = true;
        }
    }

    internal static void StopBackgroundMusic()
    {
        if (!backgroundMusicStarted)
        {
            return;
        }

        mciSendString($"stop {BackgroundMusicAlias}", null, 0, IntPtr.Zero);
        mciSendString($"close {BackgroundMusicAlias}", null, 0, IntPtr.Zero);
        backgroundMusicStarted = false;
    }
    private static Bitmap CreatePlayerShipImage()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Sprites", "Invaders", "space__0006_Player.png");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Sprite file not found: {filePath}");
        }
        Bitmap original = new Bitmap(Image.FromFile(filePath));
        return ScaleImage(original, 1.5); // Scale player to 1.5x
    }

     private static Bitmap LoadEnemyAnimationFrame(string frameName)
    {
        string framePath = Path.Combine(AppContext.BaseDirectory, "assets", "Sprites", "Invaders", $"{frameName}.png");
        if (!File.Exists(framePath))
        {
            throw new FileNotFoundException($"Sprite file not found: {framePath}");
        }
        return new Bitmap(Image.FromFile(framePath));
    }

    private static Bitmap ScaleImage(Bitmap originalImage, double scaleFactor)
    {
        ArgumentNullException.ThrowIfNull(originalImage);

        int newWidth = (int)(originalImage.Width * scaleFactor);
        int newHeight = (int)(originalImage.Height * scaleFactor);

        Bitmap scaledBitmap = new(newWidth, newHeight);

        using Graphics graphics = Graphics.FromImage(scaledBitmap);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        graphics.DrawImage(originalImage, new Rectangle(0, 0, newWidth, newHeight));

        return scaledBitmap;
    }

        internal static Bitmap CreateBunkerImage()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Sprites", "Invaders", "space__0008_ShieldFull.png");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Sprite file not found: {filePath}");
        }
        Bitmap original = new Bitmap(Image.FromFile(filePath));
        return ScaleImage(original, 1.2); // Scale bunker to 1.2x
    }

    internal static Bitmap CreateEnemyExplosionImage()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Sprites", "Invaders", "space__0009_EnemyExplosion.png");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Sprite file not found: {filePath}");
        }
        Bitmap original = new Bitmap(Image.FromFile(filePath));
        return ScaleImage(original, 2.0); // Scale explosion to 2x for visibility
    }

        internal static Bitmap CreateMissileImage()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Sprites", "Projectiles", "ProjectileA_1.png");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Sprite file not found: {filePath}");
        }
        Bitmap original = new Bitmap(Image.FromFile(filePath));
        return ScaleImage(original, 1.5); // Scale missile to 1.5x
    }

    internal static Bitmap[] CreateMissileAnimationFrames()
    {
        string[] frameNames = { "ProjectileA__2.png", "ProjectileA_3.png", "ProjectileA_4.png" };
        Bitmap[] frames = new Bitmap[frameNames.Length];

        for (int i = 0; i < frameNames.Length; i++)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Sprites", "Projectiles", frameNames[i]);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Sprite file not found: {filePath}");
            }
            Bitmap original = new Bitmap(Image.FromFile(filePath));
            frames[i] = ScaleImage(original, 1.5);
        }

        return frames;
    }

    internal static void PlayShootSound()
    {
        if (shootSound is null)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Audio", "shoot.wav");
            if (!File.Exists(filePath))
            {
                return;
            }

            shootSound = new System.Media.SoundPlayer(filePath);
        }

        shootSound.Play();
    }

     internal static void PlayInvaderKilledSound()
    {
        if (invaderKilledSound is null)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Audio", "invaderkilled.wav");
            if (!File.Exists(filePath))
            {
                return;
            }

            invaderKilledSound = new System.Media.SoundPlayer(filePath);
        }

        invaderKilledSound.Play();
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