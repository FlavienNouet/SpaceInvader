namespace SpaceInvader;

public class Missile : SimpleObject
{
    public double Vitesse { get; set; }

    private readonly Size gameSize;
    private readonly Game? game;
     private double directionX;
    private double directionY;
    private readonly bool homingEnabled;
    private readonly Bitmap[] animationFrames;
    private double animationTimer;
    private int currentFrameIndex;
    private const double AnimationFrameTime = 0.1; // Switch frame every 0.1 seconds

    public Missile(Vecteur2d position, int lives, Bitmap image)
        : this(GameObject.Side.Ally, null, position, lives, image, SystemInformation.VirtualScreen.Size, 400, -1)
    {
    }

    public Missile(Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double vitesse = 400)
            : this(GameObject.Side.Ally, game, position, lives, image, gameSize, vitesse, -1, null)
    {
    }

   public Missile(GameObject.Side camp, Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double vitesse, double verticalDirection, Bitmap[]? animationFrames = null, bool homingEnabled = false)
        : this(camp, game, position, lives, image, gameSize, vitesse, 0, verticalDirection, animationFrames, homingEnabled)
    {
    }

    public Missile(GameObject.Side camp, Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double vitesse, double directionX, double directionY, Bitmap[]? animationFrames = null, bool homingEnabled = false)
        : base(camp, position, lives, image)
    {
        this.game = game;
        this.gameSize = gameSize;
        Vitesse = vitesse;
        this.homingEnabled = homingEnabled;
         double length = Math.Sqrt(directionX * directionX + directionY * directionY);
        if (length <= double.Epsilon)
        {
            this.directionX = 0;
            this.directionY = -1;
        }
        else
        {
            this.directionX = directionX / length;
            this.directionY = directionY / length;
        }
        this.animationFrames = animationFrames ?? new[] { image };
        animationTimer = 0;
        currentFrameIndex = 0;
    }

    public override void Update(double deltaTimeSeconds)
    {
        if (!IsAlive())
        {
            return;
        }

        animationTimer += deltaTimeSeconds;
        if (animationTimer >= AnimationFrameTime)
        {
            animationTimer -= AnimationFrameTime;
            currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
        }

         if (homingEnabled && Camp == GameObject.Side.Ally && game is not null && game.TryGetNearestEnemyTarget(Position, out Vecteur2d target))
        {
            double centerX = Position.X + animationFrames[currentFrameIndex].Width / 2.0;
            double centerY = Position.Y + animationFrames[currentFrameIndex].Height / 2.0;
            double desiredX = target.X - centerX;
            double desiredY = target.Y - centerY;
            double desiredLength = Math.Sqrt(desiredX * desiredX + desiredY * desiredY);

            if (desiredLength > double.Epsilon)
            {
                desiredX /= desiredLength;
                desiredY /= desiredLength;

                double turnFactor = Math.Clamp(7 * deltaTimeSeconds, 0, 1);
                directionX = directionX + (desiredX - directionX) * turnFactor;
                directionY = directionY + (desiredY - directionY) * turnFactor;

                double newLength = Math.Sqrt(directionX * directionX + directionY * directionY);
                if (newLength > double.Epsilon)
                {
                    directionX /= newLength;
                    directionY /= newLength;
                }
            }
        }



        Position = new Vecteur2d(
            Position.X + directionX * Vitesse * deltaTimeSeconds,
            Position.Y + directionY * Vitesse * deltaTimeSeconds);

        bool outOfTop = Position.Y + animationFrames[currentFrameIndex].Height < 0;
        bool outOfBottom = Position.Y > gameSize.Height;
        bool outOfLeft = Position.X + animationFrames[currentFrameIndex].Width < 0;
        bool outOfRight = Position.X > gameSize.Width;

        if (outOfTop || outOfBottom || outOfLeft || outOfRight)
        {
            Lives = 0;
        }
        if (game is not null)
        {
            foreach (GameObject gameObject in game.Objects)
            {
                if (ReferenceEquals(gameObject, this) || !IsAlive())
                {
                    continue;
                }

                gameObject.Collision(this);
            }
        }
    }

    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        Bitmap currentFrame = animationFrames[currentFrameIndex];
        graphics.DrawImage(currentFrame, (float)Position.X, (float)Position.Y, currentFrame.Width, currentFrame.Height);
    }
    public override void Collision(Missile missile)
    {
        // Missile collisions are handled by the target object.
    }

}