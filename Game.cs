namespace SpaceInvader;
/// <summary>
///  Classe principale du jeu, gérant l'état global, les objets du jeu, les vagues d'ennemis, le score et les interactions utilisateur.
/// </summary>
public class Game
{
     private static readonly Rectangle PlayerShipSourceRect = new(355, 1163, 104, 64);
    private static readonly Rectangle BunkerSourceRect = new(402, 965, 176, 128);
    private static readonly Rectangle MissileSourceRect = new(350, 864, 4, 103);
    private static System.Media.SoundPlayer? shootSound;
    private static System.Media.SoundPlayer? invaderKilledSound;
    private static bool backgroundMusicStarted;
    private static bool isMuted;
    private const string BackgroundMusicAlias = "bgm_track";
    private const int MaxWaves = 5;
    private const double WaveBannerDurationSeconds = 1.8;
    private const int DoubleShotScoreThreshold = 500; // Bonus : double tir à partir de 500 points
    private const int HomingShotScoreThreshold = 1000;// Bonus : tirs guidés à partir de 1000 points
    private const double DoubleShotDurationSeconds = 20; // Durée du bonus double tir
    private const double HomingShotDurationSeconds = 10; // Durée du bonus tirs guidés
    public enum GameState
    {
        Menu,
        Play,
        Pause,
        Win,
        Lost
    }

    // Difficulté du jeu, influençant le nombre d'ennemis et leur comportement
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
    private int waveNumber = 1; // La vague actuelle, commence à 1 et augmente à chaque nouvelle vague d'ennemis
    public int WaveNumber => waveNumber;

    public bool IsDoubleShotActive => doubleShotRemainingSeconds > 0;

    public bool IsHomingShotActive => homingShotRemainingSeconds > 0;
    private double waveBannerRemainingSeconds;
    private bool isUpdating;
    private GameState state = GameState.Menu;

    private double doubleShotRemainingSeconds;
    private double homingShotRemainingSeconds;

    private int lastDoubleShotTriggerIndex = -1;
    private int lastHomingShotTriggerIndex = -1;
    private bool pKeyWasDown;
    private bool spaceKeyWasDown;
    private Difficulty selectedDifficulty = Difficulty.Easy;
    public IReadOnlyList<GameObject> Objects => objects;

    public PlayerSpaceship PlayerShip => playerShip;
    public Size GameSize => gameSize;
    public int Score => score;
    public EnemyBlock Enemies => enemies;
    private bool mKeyWasDown;
    private bool escapeKeyWasDown;
    private bool vKeyWasDown;
    public Difficulty SelectedDifficulty => selectedDifficulty;
    public static bool IsMuted => isMuted;

    // Le constructeur initialise la taille du jeu et démarre la musique de fond
    public Game(Size clientSize)
    {
        gameSize = clientSize;
        StartBackgroundMusic();
    }

    // Permet d'ajouter un nouvel objet au jeu. Si une mise à jour est en cours, l'objet est ajouté à une liste en attente pour éviter les problèmes de modification de collection pendant l'itération.
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

    // Incrémente le score du joueur lorsqu'un ennemi est tué. Chaque ennemi rapporte 100 points.
    public void AddEnemyKillScore()
    {
        score += 100;
    }

    // Gère les clics de souris pour sélectionner la difficulté du jeu depuis le menu principal. Si le clic se trouve dans les limites du bouton "Facile" ou "Difficile", la difficulté correspondante est sélectionnée et le jeu est initialisé.
     public void HandleMouseClick(Point location)
    {
        if (state != GameState.Menu)
        {
            return;
        }

        // Vérifie si le clic se trouve dans les limites des boutons de difficulté
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

    // Ajoute des bunkers de protection entre le joueur et les ennemis. Les bunkers sont espacés uniformément et positionnés à une distance fixe du joueur.
    private void AddBunkers()
    {
        const int bunkerWidth = 60; // Largeur approximative d'un bunker après mise à l'échelle
        const int bunkerHeight = 40;
        const int playerGap = 40;
        const int count = 3;

        // Calcule la position Y des bunkers en fonction de la position
        double y = Math.Max(0, playerShip.Position.Y - bunkerHeight - playerGap);
        double availableWidth = Math.Max(0, gameSize.Width - (count * bunkerWidth));
        double gap = availableWidth / (count + 1);


        for (int i = 0; i < count; i++)
        {
            double x = gap * (i + 1) + bunkerWidth * i;
            AddObject(new Bunker(new Vecteur2d(x, y), this));
        }
    }

    // La méthode Update est appelée à chaque frame pour mettre à jour l'état du jeu. Elle gère les entrées utilisateur, les mises à jour des objets du jeu, les transitions d'état (pause, victoire, défaite) et le déroulement des vagues d'ennemis.
     public void Update(double deltaTimeSeconds)
    {
        if (KeyPressed(Keys.V, ref vKeyWasDown))
        {
            isMuted = !isMuted;

            if (isMuted)
            {
                PauseBackgroundMusic();
            }
            else
            {
                ResumeBackgroundMusic();
            }
        }

        if (state == GameState.Menu)
        {
            return;
        }

        // Gère les entrées pour les états de victoire et de défaite
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

        // Gère les entrées pour le mode pause
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

        // Permet de mettre le jeu en pause lorsque la touche P est pressée pendant le jeu
        if (KeyPressed(Keys.P, ref pKeyWasDown))
        {
            state = GameState.Pause;
            return;
        }

        // Met à jour les timers pour les bannières de vague et les bonus de tir
        if (waveBannerRemainingSeconds > 0)
        {
            waveBannerRemainingSeconds = Math.Max(0, waveBannerRemainingSeconds - deltaTimeSeconds);
        }

        // Met à jour les timers des bonus de tir et s'assure qu'ils ne deviennent pas négatifs
        if (doubleShotRemainingSeconds > 0)
        {
            doubleShotRemainingSeconds = Math.Max(0, doubleShotRemainingSeconds - deltaTimeSeconds);
        }

        // Met à jour les timers des bonus de tir guidé et s'assure qu'ils ne deviennent pas négatifs
        if (homingShotRemainingSeconds > 0)
        {
            homingShotRemainingSeconds = Math.Max(0, homingShotRemainingSeconds - deltaTimeSeconds);
        }

        isUpdating = true;

        // Met à jour tous les objets du jeu. Chaque objet gère sa propre logique de mise à jour, comme le mouvement, les collisions, les animations, etc.
        foreach (GameObject gameObject in objects)
        {
            gameObject.Update(deltaTimeSeconds);
        }

        isUpdating = false;

        // Ajoute les objets en attente à la liste principale après la mise à jour pour éviter les problèmes de modification de collection pendant l'itération
        if (pendingObjects.Count > 0)
        {
            objects.AddRange(pendingObjects);
            pendingObjects.Clear();
        }

        CheckScoreBonuses();

        // Verifie les conditions de fin de partie 
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

    // La méthode Draw est responsable de dessiner tous les éléments du jeu à l'écran. Elle gère l'affichage du menu, des objets du jeu, du score, des bonus actifs et des messages de fin de partie.
    public void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        graphics.Clear(Color.Black);

        if (state == GameState.Menu)
        {
            DrawMenu(graphics);
            return;
        }

        // Dessine tt les objets du jeu qui sont encore en vie
        foreach (GameObject gameObject in objects)
        {
            if (gameObject.IsAlive())
            {
                gameObject.Draw(graphics);
            }
        }
        graphics.DrawString($"Score: {score} | Vague: {waveNumber}", SystemFonts.DefaultFont, Brushes.White, 10f, gameSize.Height - 24f);

         float bonusX = 10f;
        float bonusY = gameSize.Height - 42f;
        if (IsDoubleShotActive)
        {
            // Affiche le bonus de double tir avec une couleur distincte et le temps restant arrondi à l'entier supérieur
            graphics.DrawString($"Double tir: {Math.Ceiling(doubleShotRemainingSeconds)}s", SystemFonts.DefaultFont, Brushes.LightBlue, bonusX, bonusY);
            bonusY -= 18f;
        }

        if (IsHomingShotActive)
        {
            // Affiche le bonus de tirs guidés avec une couleur distincte et le temps restant arrondi à l'entier supérieur
            graphics.DrawString($"Tirs guides: {Math.Ceiling(homingShotRemainingSeconds)}s", SystemFonts.DefaultFont, Brushes.Gold, bonusX, bonusY);
        }

        // Affiche un message central en fonction de l'état du jeu (pause, victoire, défaite) ou un message de jeu en cours
        string message = state switch
        {
            GameState.Pause => "Pause",
            GameState.Win => "Gagné",
            GameState.Lost => "Perdu",
            _ => "Jeu en cours"
        };

        // Si le jeu est en pause, gagné ou perdu, affiche un message central avec des instructions pour reprendre ou retourner au menu
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

        // Si une bannière de vague doit être affichée, affiche le numéro de la vague au centre de l'écran pendant la durée spécifiée
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

// Dessine le menu principal du jeu, affichant le titre, les boutons de sélection de difficulté et une invite pour cliquer sur une difficulté. Les boutons sont dessinés avec des couleurs distinctes et des bordures pour les différencier visuellement.
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

// Calcule les rectangles de délimitation pour les boutons de sélection de difficulté dans le menu principal. Les boutons sont centrés horizontalement et espacés uniformément.
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

    // Initialise une nouvelle partie en réinitialisant tous les paramètres du jeu, en créant le vaisseau du joueur, en lançant la première vague d'ennemis et en ajoutant les bunkers de protection. Cette méthode est appelée au début d'une nouvelle partie ou après une victoire/défaite pour recommencer.
    private void InitializeGame()
    {
        objects.Clear();
        pendingObjects.Clear();
        score = 0;
        waveNumber = 1;
        waveBannerRemainingSeconds = WaveBannerDurationSeconds;
        doubleShotRemainingSeconds = 0;
        homingShotRemainingSeconds = 0;
        lastDoubleShotTriggerIndex = -1;
        lastHomingShotTriggerIndex = -1;
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

// Démarre la prochaine vague d'ennemis en créant un nouveau bloc d'ennemis et en l'ajoutant au jeu. Si un bloc d'ennemis précédent existe, il est retiré avant d'ajouter le nouveau. La bannière de vague est également réinitialisée pour afficher le numéro de la nouvelle vague.
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

    // Retourne au menu principal en réinitialisant tous les paramètres du jeu, en supprimant tous les objets et en remettant l'état du jeu à Menu. Cette méthode est appelée lorsque le joueur choisit de retourner au menu depuis les états de pause, victoire ou défaite.
    private void ReturnToMenu()
    {
        objects.Clear();
        pendingObjects.Clear();
        isUpdating = false;
        pKeyWasDown = false;
        spaceKeyWasDown = false;
        mKeyWasDown = false;
        escapeKeyWasDown = false;
        vKeyWasDown = false;
        waveBannerRemainingSeconds = 0;
        doubleShotRemainingSeconds = 0;
        homingShotRemainingSeconds = 0;
        lastDoubleShotTriggerIndex = -1;
        lastHomingShotTriggerIndex = -1;
        
        state = GameState.Menu;
    }

    // Vérifie si le score du joueur a atteint les seuils pour les bonus de double tir et de tirs guidés. Si un seuil est atteint et que le bonus n'est pas déjà actif, le bonus est activé pour une durée spécifiée. Les indices de déclenchement sont utilisés pour s'assurer que les bonus ne sont réactivés que lorsque le score atteint des multiples supplémentaires du seuil.
    private void CheckScoreBonuses()
    {
        if (score >= DoubleShotScoreThreshold)
        {
            int triggerIndex = (score - DoubleShotScoreThreshold) / 1000;
            if (triggerIndex > lastDoubleShotTriggerIndex)
            {
                lastDoubleShotTriggerIndex = triggerIndex;
                doubleShotRemainingSeconds = DoubleShotDurationSeconds;
            }
        }

        // Vérifie si le score a atteint le seuil pour les tirs guidés et active le bonus si nécessaire
        if (score >= HomingShotScoreThreshold)
        {
            int triggerIndex = (score - HomingShotScoreThreshold) / 1000;
            if (triggerIndex > lastHomingShotTriggerIndex)
            {
                lastHomingShotTriggerIndex = triggerIndex;
                homingShotRemainingSeconds = HomingShotDurationSeconds;
            }
        }
    }

    // Tente de trouver la position du centre de l'ennemi le plus proche à partir d'une position donnée. Cette méthode est utilisée pour les tirs guidés afin de cibler automatiquement l'ennemi le plus proche du vaisseau du joueur.
    public bool TryGetNearestEnemyTarget(Vecteur2d fromPosition, out Vecteur2d targetPosition)
    {
        if (enemies is null || !enemies.IsAlive())
        {
            targetPosition = new Vecteur2d();
            return false;
        }

        return enemies.TryGetNearestEnemyCenter(fromPosition, out targetPosition);
    }

    // Vérifie si une touche spécifique a été pressée en comparant son état actuel avec son état précédent. Cette méthode est utilisée pour détecter les pressions de touches individuelles, comme la mise en pause ou le tir, sans déclencher plusieurs fois pour une seule pression.
     private static bool KeyPressed(Keys key, ref bool wasDown)
    {
        bool isDown = (GetAsyncKeyState((int)key) & 0x8000) != 0;
        bool pressed = isDown && !wasDown;
        wasDown = isDown;
        return pressed;
    }

    // Crée un nouveau bloc d'ennemis pour la vague actuelle en fonction du numéro de la vague. Le nombre total d'ennemis augmente à chaque vague, et ils sont répartis en trois lignes (arrière, milieu, avant) avec des animations différentes. La largeur du bloc est ajustée pour s'adapter à la taille du jeu, et les ennemis sont positionnés de manière à être centrés horizontalement.
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
        // Charge les images d'animation pour les différentes lignes d'ennemis et les met à l'échelle pour correspondre à la taille du jeu
        Bitmap frameA1 = ScaleImage(LoadEnemyAnimationFrame("space__0000_A1"), 2.0);
        Bitmap frameA2 = ScaleImage(LoadEnemyAnimationFrame("space__0001_A2"), 2.0);
        Bitmap frameB1 = ScaleImage(LoadEnemyAnimationFrame("space__0002_B1"), 2.0);
        Bitmap frameB2 = ScaleImage(LoadEnemyAnimationFrame("space__0003_B2"), 2.0);
        Bitmap frameC1 = ScaleImage(LoadEnemyAnimationFrame("space__0004_C1"), 2.0);
        Bitmap frameC2 = ScaleImage(LoadEnemyAnimationFrame("space__0005_C2"), 2.0);

        // Ajoute les lignes d'ennemis au bloc en fonction du nombre calculé pour chaque ligne, en utilisant les images d'animation correspondantes
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

    // Démarre la musique de fond du jeu en utilisant l'API MCI de Windows pour lire un fichier audio. La musique est configurée pour se répéter en continu. Si la musique est déjà en cours de lecture ou si le jeu est en mode muet, cette méthode ne fait rien.
    internal static void StartBackgroundMusic()
    {
        if (backgroundMusicStarted || isMuted)
        {
            return;
        }

        string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Audio", "spaceinvaders1.mpeg"); // Chemin vers le fichier audio de la musique de fond
        if (!File.Exists(filePath))
        {
            return;
        }

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

    // Met en pause la musique de fond.
    private static void PauseBackgroundMusic()
    {
        if (!backgroundMusicStarted)
        {
            return;
        }

        mciSendString($"pause {BackgroundMusicAlias}", null, 0, IntPtr.Zero);
    }

    // Reprend la lecture de la musique de fond si elle a été mise en pause.
    private static void ResumeBackgroundMusic()
    {
        if (!backgroundMusicStarted)
        {
            return;
        }

        mciSendString($"resume {BackgroundMusicAlias}", null, 0, IntPtr.Zero);
    }

    // Arrête la musique de fond et libère les ressources associées. Cette méthode est appelée lorsque le jeu est mis en mode muet ou lorsque le joueur retourne au menu principal.
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

    // Charge une image d'animation d'ennemi à partir du dossier des ressources. Le nom de l'image est passé en paramètre, et l'image est mise à l'échelle pour correspondre à la taille du jeu. Si le fichier de l'image n'est pas trouvé, une exception est levée.
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

        // Crée une image de bunker à partir du fichier de sprite correspondant. L'image est mise à l'échelle pour être légèrement plus grande que la taille d'origine afin de mieux correspondre à la taille du jeu. Si le fichier de sprite n'est pas trouvé, une exception est levée.
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

    // Création de la frame de l'explosion
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

    // Création de la frame de l'explosion d'impact (lorsque le missile touche un bunker)
    internal static Bitmap CreateImpactExplosionImage()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "assets", "Sprites", "Invaders", "space__0009_EnemyExplosion.png");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Sprite file not found: {filePath}");
        }

        Bitmap original = new Bitmap(Image.FromFile(filePath));
        return ScaleImage(original, 0.7); // Smaller impact effect for bunker hits
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

    // Animation avec les frames du missile por créer le mouvement du missile
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

    // Son du tir du vaisseau 
    internal static void PlayShootSound()
    {
        if (isMuted)
        {
            return;
        }

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

    // Son joué lorsqu'un envahisseur est tué
     internal static void PlayInvaderKilledSound()
    {
        if (isMuted)
        {
            return;
        }

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