namespace SpaceInvader;

public class SpaceShip : GameObject
{
    private readonly double playerSpeedPixelPerSecond;
    private readonly Size gameSize;

    public Vecteur2d Position { get; set; }

    public int Lives { get; set; }

    public Bitmap Image { get; }

    public SpaceShip(Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(image);

        Position = position;
        Lives = lives;
        Image = image;
        this.gameSize = gameSize;
        this.playerSpeedPixelPerSecond = playerSpeedPixelPerSecond;
    }

    public override void Update(double deltaTimeSeconds)
    {
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

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
