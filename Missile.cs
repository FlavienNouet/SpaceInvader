namespace SpaceInvader;
/// <summary>
///  Représente un missile dans le jeu. Le missile peut être dirigé vers une cible ennemie si le homing est activé.
/// </summary>
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
    private const double AnimationFrameTime = 0.1;

    // Constructeur simplifié pour les missiles alliés sans homing et sans animation
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

    // Constructeur principal pour les missiles avec toutes les options
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
            this.directionY = -1; // Par défaut, le missile se dirige vers le haut
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

    // Gestion de l'animation et du homing dans la méthode Update
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

        // Logique de homing : ajuster la direction du missile vers la cible ennemie la plus proche
         if (homingEnabled && Camp == GameObject.Side.Ally && game is not null && game.TryGetNearestEnemyTarget(Position, out Vecteur2d target))
        {
            // Calculer la direction souhaitée vers la cible
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

                // Normaliser la direction après l'ajustement
                double newLength = Math.Sqrt(directionX * directionX + directionY * directionY);
                if (newLength > double.Epsilon)
                {
                    directionX /= newLength;
                    directionY /= newLength;
                }
            }
        }


        // Mettre à jour la position du missile en fonction de sa direction et de sa vitesse
        Position = new Vecteur2d(
            Position.X + directionX * Vitesse * deltaTimeSeconds,
            Position.Y + directionY * Vitesse * deltaTimeSeconds);

        // Le missile meurt quand il sort de l'écran
        bool outOfTop = Position.Y + animationFrames[currentFrameIndex].Height < 0;
        bool outOfBottom = Position.Y > gameSize.Height;
        bool outOfLeft = Position.X + animationFrames[currentFrameIndex].Width < 0;
        bool outOfRight = Position.X > gameSize.Width;

        if (outOfTop || outOfBottom || outOfLeft || outOfRight)
        {
            Lives = 0;
        }

        // Vérifier les collisions avec les autres objets du jeu
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

    // Dessiner le missile en utilisant l'image de l'animation actuelle
    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        Bitmap currentFrame = animationFrames[currentFrameIndex];
        graphics.DrawImage(currentFrame, (float)Position.X, (float)Position.Y, currentFrame.Width, currentFrame.Height);
    }

    // Gérer les collisions avec d'autres missiles : les deux missiles sont détruits
     protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
        Lives = 0;
        missile.Lives = 0;
    }

}