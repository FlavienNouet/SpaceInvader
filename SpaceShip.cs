namespace SpaceInvader;

public class SpaceShip : GameObject
{
    private readonly double playerSpeedPixelPerSecond;
    private readonly Size gameSize;
    private readonly Game game;

    private Missile? missile;

    public Vecteur2d Position { get; set; }

    public int Lives { get; set; }

    public Bitmap Image { get; }

    public SpaceShip(Game game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(image);

        this.game = game;
        Position = position;
        Lives = lives;
        Image = image;
        this.gameSize = gameSize;
        this.playerSpeedPixelPerSecond = playerSpeedPixelPerSecond;
        this.game = game;   
    }

    public override void Update(double deltaTimeSeconds)
    {
        if (IsKeyDown(Keys.Space))
        {
            Shoot();
        }
        double moveDistance = playerSpeedPixelPerSecond * deltaTimeSeconds;

        if (IsKeyDown(Keys.Left) || IsKeyDown(Keys.A))
        {
            Position = new Vecteur2d(Math.Max(0, Position.X - moveDistance), Position.Y);
        }

        if (IsKeyDown(Keys.Right) || IsKeyDown(Keys.D))
        {
            double maxX = Math.Max(0, gameSize.Width - Image.Width);
            Position = new Vecteur2d(Math.Min(maxX, Position.X + moveDistance), Position.Y);
        }
    }

 public void Shoot()
    {
        if (missile is not null && missile.IsAlive())
        {
            return;
        }

        Bitmap missileImage = CreateMissileImage();
        Vecteur2d missilePosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0,
            Position.Y - missileImage.Height);

        missile = new Missile(missilePosition, 1, missileImage, game.GameSize);
        game.AddObject(missile);
    }
    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        graphics.DrawImage(Image, (float)Position.X, (float)Position.Y, Image.Width, Image.Height);
    }

    public override bool IsAlive()
    {
        return Lives > 0;
    }

    private static bool IsKeyDown(Keys key)
    {
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    private static Bitmap CreateMissileImage()
    {
        Bitmap bitmap = new(6, 16);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        using Brush brush = new SolidBrush(Color.Yellow);
        graphics.FillRectangle(brush, 2, 0, 2, 16);

        return bitmap;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
