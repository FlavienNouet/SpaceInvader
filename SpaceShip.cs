namespace SpaceInvader;

public class SpaceShip : SimpleObject
{
    protected double PlayerSpeedPixelPerSecond { get; }
    protected Size GameSize { get; }
    private readonly Game? game;

    private Missile? missile;
    private Bitmap? alternateImage;
    private double animationTimer;
    private const double AnimationFrameTime = 0.5; // Switch frame every 0.5 seconds
    private bool useAlternateFrame;

    public SpaceShip(Vecteur2d position, int lives, Bitmap image)
        : this(GameObject.Side.Enemy, null, position, lives, image, Size.Empty)
    {
        }

    public SpaceShip(Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : this(GameObject.Side.Enemy, null, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
    }

    public SpaceShip(Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200, Bitmap? alternateImage = null)
        : this(GameObject.Side.Enemy, game, position, lives, image, gameSize, playerSpeedPixelPerSecond, alternateImage)
    {
    }

    protected SpaceShip(GameObject.Side camp, Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200, Bitmap? alternateImage = null)
        : base(camp, position, lives, image)
    {
        this.game = game;

        GameSize = gameSize;
        PlayerSpeedPixelPerSecond = playerSpeedPixelPerSecond;
        this.alternateImage = alternateImage;
        animationTimer = 0;
        useAlternateFrame = false;
    }
    

    public override void Update(double deltaTimeSeconds)
    {
     if (alternateImage is not null)
        {
            animationTimer += deltaTimeSeconds;
            if (animationTimer >= AnimationFrameTime)
            {
                animationTimer -= AnimationFrameTime;
                useAlternateFrame = !useAlternateFrame;
            }
        }
    }

    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        Bitmap imageToUse = useAlternateFrame && alternateImage is not null ? alternateImage : Image;
        graphics.DrawImage(imageToUse, new RectangleF((float)Position.X, (float)Position.Y, imageToUse.Width, imageToUse.Height));
    }

    protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
        int damage = Math.Min(Lives, missile.Lives);
        Lives = Math.Max(0, Lives - damage);
        missile.Lives = Math.Max(0, missile.Lives - damage);
         if (!IsAlive() && game is not null)
        {
            Bitmap explosionImage = Game.CreateEnemyExplosionImage();
            Explosion explosion = new Explosion(Position, explosionImage);
            game.AddObject(explosion);
        }
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

        Bitmap[] animationFrames = Game.CreateMissileAnimationFrames();
        Bitmap missileImage = animationFrames[0];
        Vecteur2d missilePosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0,
            shootDownwards ? Position.Y + Image.Height : Position.Y - missileImage.Height);

        double verticalDirection = shootDownwards ? 1 : -1;

        missile = new Missile(Camp, game, missilePosition, 1, missileImage, game.GameSize, 400, verticalDirection, animationFrames);
        game.AddObject(missile);
        Game.PlayShootSound();
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
