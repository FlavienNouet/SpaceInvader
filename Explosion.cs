namespace SpaceInvader;

public class Explosion : GameObject
{
    private readonly Bitmap image;
    private double displayTime;
    private const double ExplosionDuration = 0.3; // Display for 0.3 seconds

    public Vecteur2d Position { get; private set; }

    public Explosion(Vecteur2d position, Bitmap explosionImage)
        : base(GameObject.Side.Neutral)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(explosionImage);

        Position = position;
        image = explosionImage;
        displayTime = 0;
    }

    public override void Update(double deltaTimeSeconds)
    {
        displayTime += deltaTimeSeconds;
    }

    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        graphics.DrawImage(image, (float)Position.X, (float)Position.Y, image.Width, image.Height);
    }

    public override bool IsAlive()
    {
        return displayTime < ExplosionDuration;
    }

    public override void Collision(Missile missile)
    {
        // Explosions don't collide with missiles
    }
}