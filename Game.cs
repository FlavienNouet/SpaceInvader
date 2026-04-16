namespace SpaceInvader;

public class Game
{
    private readonly List<GameObject> objects = new();
    private readonly SpaceShip playerShip;

    private readonly Size gameSize;

    public IReadOnlyList<GameObject> Objects => objects;

    public SpaceShip PlayerShip => playerShip;

    public Game(Size clientSize)
    {
        gameSize = clientSize;
        Bitmap shipImage = CreatePlayerShipImage();
        Vecteur2d startPosition = new(
            (clientSize.Width - shipImage.Width) / 2.0,
            Math.Max(0, clientSize.Height - shipImage.Height - 20));

        playerShip = new SpaceShip(startPosition, 3, shipImage, gameSize);
        objects.Add(playerShip);
    }

    public void Update(double deltaTimeSeconds)
    {
        foreach (GameObject gameObject in objects)
        {
            gameObject.Update(deltaTimeSeconds);
        }
    }

    public void Draw(Graphics graphics)
    {
        foreach (GameObject gameObject in objects)
        {
            gameObject.Draw(graphics);
        }
    }

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
}