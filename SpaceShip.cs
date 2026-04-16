namespace SpaceInvader;

public class SpaceShip : GameObject
{
    private readonly double speedPixelPerSecond;

    public Vecteur2d Position { get; set; }

    public int Lives { get; set; }

    public Bitmap Image { get; }

    public SpaceShip(Vecteur2d position, int lives, Bitmap image, double speedPixelPerSecond = 200)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(image);

        Position = position;
        Lives = lives;
        Image = image;
        this.speedPixelPerSecond = speedPixelPerSecond;
    }

    public override void Update(double deltaTimeSeconds)
    {
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