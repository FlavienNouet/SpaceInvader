namespace SpaceInvader;

public class SpaceShip : SimpleObject
{
    protected double PlayerSpeedPixelPerSecond { get; }
    protected Size GameSize { get; }
    private readonly Game? game;

    private Missile? missile;

    public SpaceShip(Vecteur2d position, int lives, Bitmap image)
        : this(null, position, lives, image, Size.Empty)
    {
        }

    public SpaceShip(Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : this(null, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
    }

    public SpaceShip(Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : base(position, lives, image)
    {

        GameSize = gameSize;
        PlayerSpeedPixelPerSecond = playerSpeedPixelPerSecond;
    }
    

    public override void Update(double deltaTimeSeconds)
    {
    
    }

 public void Shoot()
    {
        if (game is null)
        {
            return;
        }

        if (missile is not null && missile.IsAlive())
        {
            return;
        }

        Bitmap missileImage = CreateMissileImage();
        Vecteur2d missilePosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0,
            Position.Y - missileImage.Height);

        missile = new Missile(game, missilePosition, 1, missileImage, game.GameSize);
        game.AddObject(missile);
    }
    protected static bool IsKeyDown(Keys key)
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
