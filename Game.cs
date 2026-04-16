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
    private const int MaxWaves = 5;
    private const double WaveBannerDurationSeconds = 1.8;
    public enum GameState
    {
        Menu,
        Play,
        Pause,
        Win,
        Lost
    }

    public enum Difficulty
    {
        Easy,
        Hard
    }
    private readonly List<GameObject> objects = new();
    private readonly List<GameObject> pendingObjects = new();
     private PlayerSpaceship playerShip = null!;
    private EnemyBlock enemies = null!;

    private readonly Size gameSize;
    private int score;
    private int waveNumber = 1;
    public int WaveNumber => waveNumber;
    private double waveBannerRemainingSeconds;
    private bool isUpdating;
    private GameState state = GameState.Menu;
    private bool pKeyWasDown;
    private bool spaceKeyWasDown;
    private Difficulty selectedDifficulty = Difficulty.Easy;
    public IReadOnlyList<GameObject> Objects => objects;

    public PlayerSpaceship PlayerShip => playerShip;
    public Size GameSize => gameSize;
    public int Score => score;
    private bool mKeyWasDown;
    private bool escapeKeyWasDown;
    public Difficulty SelectedDifficulty => selectedDifficulty;

    public Game(Size clientSize)
    {
        gameSize = clientSize;
        StartBackgroundMusic();
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

    public void AddEnemyKillScore()
    {
        score += 100;
    }

     public void HandleMouseClick(Point location)
    {
        if (state != GameState.Menu)
        {
            return;
        }

        GetDifficultyButtonBounds(out Rectangle easyButton, out Rectangle hardButton);

        if (easyButton.Contains(location))
        {
            selectedDifficulty = Difficulty.Easy;
            InitializeGame();
        }
        else if (hardButton.Contains(location))
        {
            selectedDifficulty = Difficulty.Hard;
            InitializeGame();
        }
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

        if (state == GameState.Menu)
        {
            return;
        }

        if (state == GameState.Win || state == GameState.Lost)
        {
            if (KeyPressed(Keys.M, ref mKeyWasDown) || KeyPressed(Keys.Escape, ref escapeKeyWasDown))
            {
                ReturnToMenu();
                return;
            }
            if (KeyPressed(Keys.Space, ref spaceKeyWasDown))
            {
                InitializeGame();
            }

            return;
        }

        if (state == GameState.Pause)
        {
            if (KeyPressed(Keys.M, ref mKeyWasDown) || KeyPressed(Keys.Escape, ref escapeKeyWasDown))
            {
                ReturnToMenu();
                return;
            }
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

        if (waveBannerRemainingSeconds > 0)
        {
            waveBannerRemainingSeconds = Math.Max(0, waveBannerRemainingSeconds - deltaTimeSeconds);
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
            if (waveNumber >= MaxWaves)
            {
                state = GameState.Win;
            }
            else
            {
                waveNumber++;
                StartNextWave();
            }
        }

        objects.RemoveAll(gameObject => !gameObject.IsAlive());
    }

    public void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        graphics.Clear(Color.Black);

        if (state == GameState.Menu)
        {
            DrawMenu(graphics);
            return;
        }

        foreach (GameObject gameObject in objects)
        {
            if (gameObject.IsAlive())
            {
                gameObject.Draw(graphics);
            }
        }
        graphics.DrawString($"Score: {score} | Vague: {waveNumber}", SystemFonts.DefaultFont, Brushes.White, 10f, gameSize.Height - 24f);

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
            graphics.DrawString(message, font, Brushes.Lime, bounds, format);
            string hint = state == GameState.Pause
                ? "P: reprendre | M/Echap: accueil"
                : "Espace: rejouer | M/Echap: accueil";
            RectangleF hintBounds = new(0, gameSize.Height - 50, gameSize.Width, 30);
            graphics.DrawString(hint, SystemFonts.DefaultFont, Brushes.White, hintBounds, format);
            return;
        }

         if (waveBannerRemainingSeconds > 0)
        {
            using Font waveFont = new(FontFamily.GenericSansSerif, 28, FontStyle.Bold, GraphicsUnit.Point);
            using StringFormat centered = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            RectangleF waveBounds = new(0, gameSize.Height * 0.22f, gameSize.Width, 50);
            graphics.DrawString($"Vague {waveNumber}", waveFont, Brushes.Lime, waveBounds, centered);
        }

         graphics.DrawString(message, SystemFonts.DefaultFont, Brushes.White, 10f, 10f);
    }

private void DrawMenu(Graphics graphics)
    {
        using Font titleFont = new(FontFamily.GenericSansSerif, 28, FontStyle.Bold, GraphicsUnit.Point);
        using Font buttonFont = new(FontFamily.GenericSansSerif, 18, FontStyle.Bold, GraphicsUnit.Point);
        using StringFormat centered = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        RectangleF titleBounds = new(0, 60, gameSize.Width, 80);
        graphics.DrawString("Space Invaders", titleFont, Brushes.Lime, titleBounds, centered);

        GetDifficultyButtonBounds(out Rectangle easyButton, out Rectangle hardButton);

        graphics.FillRectangle(Brushes.DarkGreen, easyButton);
        graphics.DrawRectangle(Pens.Lime, easyButton);
        graphics.DrawString("Facile", buttonFont, Brushes.White, easyButton, centered);

        graphics.FillRectangle(Brushes.DarkRed, hardButton);
        graphics.DrawRectangle(Pens.OrangeRed, hardButton);
        graphics.DrawString("Difficile", buttonFont, Brushes.White, hardButton, centered);

        RectangleF hintBounds = new(0, hardButton.Bottom + 24, gameSize.Width, 30);
        graphics.DrawString("Clique sur une difficulté", SystemFonts.DefaultFont, Brushes.White, hintBounds, centered);
    }

    private void GetDifficultyButtonBounds(out Rectangle easyButton, out Rectangle hardButton)
    {
        int buttonWidth = 180;
        int buttonHeight = 56;
        int spacing = 24;
        int totalWidth = buttonWidth * 2 + spacing;
        int startX = (gameSize.Width - totalWidth) / 2;
        int y = gameSize.Height / 2 - buttonHeight / 2;

        easyButton = new Rectangle(startX, y, buttonWidth, buttonHeight);
        hardButton = new Rectangle(startX + buttonWidth + spacing, y, buttonWidth, buttonHeight);
    }

    private void InitializeGame()
    {
        objects.Clear();
        pendingObjects.Clear();
        score = 0;
        waveNumber = 1;
        waveBannerRemainingSeconds = WaveBannerDurationSeconds;
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

        StartNextWave();

        AddBunkers();
    }

private void StartNextWave()
    {
        if (enemies is not null)
        {
            objects.Remove(enemies);
            pendingObjects.Remove(enemies);
        }

        enemies = CreateEnemyBlock();
        AddObject(enemies);
        waveBannerRemainingSeconds = WaveBannerDurationSeconds;
    }
    private void ReturnToMenu()
    {
        objects.Clear();
        pendingObjects.Clear();
        isUpdating = false;
        pKeyWasDown = false;
        spaceKeyWasDown = false;
        mKeyWasDown = false;
        escapeKeyWasDown = false;
        waveBannerRemainingSeconds = 0;
        state = GameState.Menu;
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
        int totalEnemies = 5 + (waveNumber - 1) * 5;
        int backLineCount = (int)Math.Ceiling(totalEnemies / 3.0);
        int remainingAfterBack = Math.Max(0, totalEnemies - backLineCount);
        int middleLineCount = (int)Math.Ceiling(remainingAfterBack / 2.0);
        int frontLineCount = Math.Max(0, remainingAfterBack - middleLineCount);

        int blockWidth = Math.Max(120, (int)(gameSize.Width * 0.9));
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
        if (backLineCount > 0)
        {
            block.AddLine(backLineCount, 1, frameA1, frameA2);
        }

        if (middleLineCount > 0)
        {
            block.AddLine(middleLineCount, 1, frameB1, frameB2);
        }

        if (frontLineCount > 0)
        {
            block.AddLine(frontLineCount, 1, frameC1, frameC2);
        }

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