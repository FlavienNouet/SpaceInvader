namespace SpaceInvader;

public class Missile : SimpleObject
{
    public double Vitesse { get; set; }

    private readonly Size gameSize;
    private readonly Game? game;

    public Missile(Vecteur2d position, int lives, Bitmap image)
        : this(null, position, lives, image, SystemInformation.VirtualScreen.Size)
    {
    }

    public Missile(Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double vitesse = 400)
        : base(position, lives, image)
    {
        this.game = game;
        this.gameSize = gameSize;
        Vitesse = vitesse;
    }

    public override void Update(double deltaTimeSeconds)
    {
        if (!IsAlive())
        {
            return;
        }

        Position = new Vecteur2d(Position.X, Position.Y - Vitesse * deltaTimeSeconds);

        if (Position.Y + Image.Height < 0 || Position.X > gameSize.Width || Position.Y > gameSize.Height)
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

}