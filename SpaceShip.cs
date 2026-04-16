namespace SpaceInvader;

public class SpaceShip : SimpleObject
{
    protected double PlayerSpeedPixelPerSecond { get; }
    protected Size GameSize { get; }
    private readonly Game? game;

    private Missile? missile;

    public SpaceShip(Vecteur2d position, int lives, Bitmap image)
        : this(GameObject.Side.Enemy, null, position, lives, image, Size.Empty)
    {
        }

    public SpaceShip(Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : this(GameObject.Side.Enemy, null, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
    }

    public SpaceShip(Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : this(GameObject.Side.Enemy, game, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
    }

    protected SpaceShip(GameObject.Side camp, Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : base(camp, position, lives, image)
    {
        this.game = game;

        GameSize = gameSize;
        PlayerSpeedPixelPerSecond = playerSpeedPixelPerSecond;
    }
    

    public override void Update(double deltaTimeSeconds)
    {
    
    }

    protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
        int damage = Math.Min(Lives, missile.Lives);
        Lives = Math.Max(0, Lives - damage);
        missile.Lives = Math.Max(0, missile.Lives - damage);
    }

 public void Shoot(bool shootDownwards = false)
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
            shootDownwards ? Position.Y + Image.Height : Position.Y - missileImage.Height);

        double verticalDirection = shootDownwards ? 1 : -1;

        missile = new Missile(Camp, game, missilePosition, 1, missileImage, game.GameSize, 400, verticalDirection);
        game.AddObject(missile);
    }
    protected static bool IsKeyDown(Keys key)
    {
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    private static Bitmap CreateMissileImage()
    {
       return Game.CreateMissileImage();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
