namespace SpaceInvader;

public class SpaceShip : SimpleObject
{
    protected double PlayerSpeedPixelPerSecond { get; }
    protected Size GameSize { get; }
    private readonly Game? game;

    private readonly List<Missile> activeMissiles = new();
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

    public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        if (!IsAlive() || !missile.IsAlive() || Camp == missile.Camp || ReferenceEquals(this, missile))
        {
            return;
        }

        Rectangle objectRectangle = GetObjectRectangle(Position, Image.Width, Image.Height);
        Rectangle missileRectangle = GetObjectRectangle(missile.Position, missile.Image.Width, missile.Image.Height);

        if (!objectRectangle.IntersectsWith(missileRectangle))
        {
            return;
        }

        int numberOfPixelsInCollision = 0;

        for (int missileLocalY = 0; missileLocalY < missile.Image.Height; missileLocalY++)
        {
            for (int missileLocalX = 0; missileLocalX < missile.Image.Width; missileLocalX++)
            {
                Color missilePixel = missile.Image.GetPixel(missileLocalX, missileLocalY);

                if (missilePixel.A == 0)
                {
                    continue;
                }

                int screenX = missileRectangle.Left + missileLocalX;
                int screenY = missileRectangle.Top + missileLocalY;
                int objectLocalX = screenX - objectRectangle.Left;
                int objectLocalY = screenY - objectRectangle.Top;

                if (!IsInsideImage(objectLocalX, objectLocalY, Image.Width, Image.Height))
                {
                    continue;
                }

                Color objectPixel = Image.GetPixel(objectLocalX, objectLocalY);

                if (objectPixel.A == 0)
                {
                    continue;
                }

                Image.SetPixel(objectLocalX, objectLocalY, Color.Transparent);
                numberOfPixelsInCollision++;
            }
        }

        if (numberOfPixelsInCollision <= 0)
        {
            return;
        }

        int damage = Math.Min(Lives, missile.Lives);
        Lives = Math.Max(0, Lives - damage);
        missile.Lives = Math.Max(0, missile.Lives - damage);

        HandleDeath();
    }

    private void HandleDeath()
    {
         if (!IsAlive() && game is not null && Camp == GameObject.Side.Enemy)
        {
            Bitmap explosionImage = Game.CreateEnemyExplosionImage();
            Explosion explosion = new Explosion(Position, explosionImage);
            game.AddObject(explosion);
            game.AddEnemyKillScore();
            Game.PlayInvaderKilledSound();
        }
    }

 public void Shoot(bool shootDownwards = false)
    {
        if (game is null)
        {
            return;
        }

        CleanupMissiles();
        if (activeMissiles.Count > 0)
        {
            return;
        }

        bool isPlayerShot = Camp == GameObject.Side.Ally && !shootDownwards;

        if (isPlayerShot && game.IsDoubleShotActive)
        {
            ShootDouble();
            return;
        }

        Bitmap[] animationFrames = Game.CreateMissileAnimationFrames();
        Bitmap missileImage = animationFrames[0];
        Vecteur2d missilePosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0,
            shootDownwards ? Position.Y + Image.Height : Position.Y - missileImage.Height);

        double verticalDirection = shootDownwards ? 1 : -1;
        bool homingEnabled = isPlayerShot && game.IsHomingShotActive;

        Missile missile = new Missile(Camp, game, missilePosition, 1, missileImage, game.GameSize, 400, 0, verticalDirection, animationFrames, homingEnabled);
        activeMissiles.Add(missile);
        game.AddObject(missile);
        Game.PlayShootSound();
    }

     public void ShootAt(Vecteur2d targetPosition)
    {
        if (game is null)
        {
            return;
        }

        CleanupMissiles();
        if (activeMissiles.Count > 0)
        {
            return;
        }

        Bitmap missileImage = CreateMissileImage();
        Bitmap[] animationFrames = Game.CreateMissileAnimationFrames();
        Vecteur2d missilePosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0,
            Position.Y + Image.Height);

        double missileCenterX = missilePosition.X + missileImage.Width / 2.0;
        double missileCenterY = missilePosition.Y + missileImage.Height / 2.0;
        double directionX = targetPosition.X - missileCenterX;
        double directionY = targetPosition.Y - missileCenterY;

        Missile missile = new Missile(Camp, game, missilePosition, 1, missileImage, game.GameSize, 400, directionX, directionY, animationFrames, false);
        activeMissiles.Add(missile);
        game.AddObject(missile);
        Game.PlayShootSound();
    }

    private void ShootDouble()
    {
        if (game is null)
        {
            return;
        }

        Bitmap missileImage = CreateMissileImage();
        Bitmap[] animationFrames = Game.CreateMissileAnimationFrames();
        const double sideOffset = 10;
        bool homingEnabled = game.IsHomingShotActive;

        Vecteur2d leftPosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0 - sideOffset,
            Position.Y - missileImage.Height);
        Vecteur2d rightPosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0 + sideOffset,
            Position.Y - missileImage.Height);

        Missile leftMissile = new Missile(Camp, game, leftPosition, 1, missileImage, game.GameSize, 400, 0, -1, animationFrames, homingEnabled);
        Missile rightMissile = new Missile(Camp, game, rightPosition, 1, missileImage, game.GameSize, 400, 0, -1, animationFrames, homingEnabled);

        activeMissiles.Add(leftMissile);
        activeMissiles.Add(rightMissile);
        game.AddObject(leftMissile);
        game.AddObject(rightMissile);
        Game.PlayShootSound();
    }

    private void CleanupMissiles()
    {
        activeMissiles.RemoveAll(m => !m.IsAlive());
    }

    private static Rectangle GetObjectRectangle(Vecteur2d position, int width, int height)
    {
        return new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), width, height);
    }

    private static bool IsInsideImage(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
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
