namespace SpaceInvader;

public class Missile : SimpleObject
{
    public double Vitesse { get; set; }

    private readonly Size gameSize;
    private readonly Game? game;
    private readonly double verticalDirection;
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

    public Missile(GameObject.Side camp, Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double vitesse, double verticalDirection, Bitmap[]? animationFrames = null)
        : base(camp, position, lives, image)
    {
        this.game = game;
        this.gameSize = gameSize;
        Vitesse = vitesse;
        this.verticalDirection = verticalDirection;
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

        Position = new Vecteur2d(Position.X, Position.Y + verticalDirection * Vitesse * deltaTimeSeconds);

        bool outOfTop = Position.Y + animationFrames[currentFrameIndex].Height < 0;
        bool outOfBottom = Position.Y > gameSize.Height;

        if (outOfTop || outOfBottom || Position.X > gameSize.Width)
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
     protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
        Lives = 0;
        missile.Lives = 0;
    }

}