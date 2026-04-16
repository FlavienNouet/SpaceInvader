namespace SpaceInvader;

public class Missile : GameObject
{
    public Vecteur2d Position { get; set; }

    public double Vitesse { get; set; }

    public int Lives { get; set; }

    public Bitmap Image { get; }

    private readonly Size gameSize;

    public Missile(Vecteur2d position, int lives, Bitmap image)
        : this(position, lives, image, SystemInformation.VirtualScreen.Size)
    {
    }

    public Missile(Vecteur2d position, int lives, Bitmap image, Size gameSize, double vitesse = 400)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(image);

        Position = position;
        Lives = lives;
        Image = image;
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

    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        graphics.DrawImage(Image, (float)Position.X, (float)Position.Y, Image.Width, Image.Height);
    }

    public override bool IsAlive()
    {
        return Lives > 0;
    }
}