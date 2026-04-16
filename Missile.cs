namespace SpaceInvader;

public class Missile : SimpleObject
{
    public double Vitesse { get; set; }

    private readonly Size gameSize;

    public Missile(Vecteur2d position, int lives, Bitmap image)
        : this(position, lives, image, SystemInformation.VirtualScreen.Size)
    {
    }

    public Missile(Vecteur2d position, int lives, Bitmap image, Size gameSize, double vitesse = 400)
        : base(position, lives, image)
    {
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
    }

}