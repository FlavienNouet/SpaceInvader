namespace SpaceInvader;

/// <summary>
///  Mise en place des explosions lors de la destruction d'un vaisseau ou d'un missile.
/// </summary>
public class Explosion : GameObject
{
    private readonly Bitmap image;
    private double displayTime;
    private const double ExplosionDuration = 0.3; // Explosion dure 0.3 secondes

    public Vecteur2d Position { get; private set; }

    // Constructeur de l'explosion
    public Explosion(Vecteur2d position, Bitmap explosionImage)
        : base(GameObject.Side.Neutral)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(explosionImage);

        Position = position;
        image = explosionImage;
        displayTime = 0;
    }

    // Mise à jour du temps d'affichage de l'explosion
    public override void Update(double deltaTimeSeconds)
    {
        displayTime += deltaTimeSeconds;
    }

    // Dessine l'explosion à sa position actuelle
    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        graphics.DrawImage(image, (float)Position.X, (float)Position.Y, image.Width, image.Height);
    }

    // L'explosion est considérée comme vivante tant que son temps d'affichage est inférieur à la durée définie
    public override bool IsAlive()
    {
        return displayTime < ExplosionDuration;
    }

    public override void Collision(Missile missile)
    {
    }
}