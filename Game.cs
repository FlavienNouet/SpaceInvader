namespace SpaceInvader;

public class Game
{
    public enum GameState
    {
        Play,
        Pause
    }
    private readonly List<GameObject> objects = new();
    private readonly List<GameObject> pendingObjects = new();
    private readonly PlayerSpaceship playerShip;
    private readonly EnemyBlock enemies;

    private readonly Size gameSize;
    private bool isUpdating;
    private GameState state = GameState.Play;
    private bool pKeyWasDown;

    public IReadOnlyList<GameObject> Objects => objects;

    public PlayerSpaceship PlayerShip => playerShip;
    public Size GameSize => gameSize;

    public Game(Size clientSize)
    {
        gameSize = clientSize;
        Bitmap shipImage = CreatePlayerShipImage();
        Vecteur2d startPosition = new(
            (clientSize.Width - shipImage.Width) / 2.0,
            Math.Max(0, clientSize.Height - shipImage.Height - 20));

        playerShip = new PlayerSpaceship(this, startPosition, 3, shipImage, gameSize);
        objects.Add(playerShip);
         enemies = CreateEnemyBlock();
        AddObject(enemies);
        AddBunkers();
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
        if (ReleaseKey(Keys.P))
        {
            state = state == GameState.Play ? GameState.Pause : GameState.Play;
        }

        if (state == GameState.Pause)
        {
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

        objects.RemoveAll(gameObject => !gameObject.IsAlive());
    }

    public void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        foreach (GameObject gameObject in objects)
        {
            if (gameObject.IsAlive())
            {
                gameObject.Draw(graphics);
            }
        }
        string message = state == GameState.Pause ? "Pause" : "Jeu en cours";

        if (state == GameState.Pause)
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

    private bool ReleaseKey(Keys key)
    {
        bool isDown = (GetAsyncKeyState((int)key) & 0x8000) != 0;
        bool released = pKeyWasDown && !isDown;

        if (key == Keys.P)
        {
            pKeyWasDown = isDown;
        }

        return released;
    }

     private EnemyBlock CreateEnemyBlock()
    {
        int blockWidth = Math.Max(120, (int)(gameSize.Width * 0.75));
        double startX = (gameSize.Width - blockWidth) / 2.0;

        EnemyBlock block = new EnemyBlock(new Vecteur2d(startX, 60), blockWidth, gameSize);
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
        Bitmap bitmap = new(48, 48);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using Brush bodyBrush = new SolidBrush(Color.SteelBlue);
        using Brush cockpitBrush = new SolidBrush(Color.WhiteSmoke);
        using Pen outlinePen = new(Color.White, 2);

        PointF[] shipPoints =
        [
            new PointF(24, 2),
            new PointF(44, 38),
            new PointF(32, 34),
            new PointF(24, 46),
            new PointF(16, 34),
            new PointF(4, 38)
        ];

        graphics.FillPolygon(bodyBrush, shipPoints);
        graphics.DrawPolygon(outlinePen, shipPoints);
        graphics.FillEllipse(cockpitBrush, 18, 14, 12, 12);

        return bitmap;
    }

    private static Bitmap CreateEnemyShipImage()
    {
        Bitmap bitmap = new(42, 30);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using Brush bodyBrush = new SolidBrush(Color.DarkSlateGray);
        using Brush eyeBrush = new SolidBrush(Color.White);

        graphics.FillRectangle(bodyBrush, 3, 10, 36, 12);
        graphics.FillRectangle(bodyBrush, 8, 4, 26, 8);
        graphics.FillRectangle(bodyBrush, 6, 22, 6, 6);
        graphics.FillRectangle(bodyBrush, 30, 22, 6, 6);

        graphics.FillRectangle(eyeBrush, 14, 12, 4, 4);
        graphics.FillRectangle(eyeBrush, 24, 12, 4, 4);

        return bitmap;
    }
}