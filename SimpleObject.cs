namespace SpaceInvader;

public abstract class SimpleObject : GameObject
{
    public Vecteur2d Position { get; set; }

    public int Lives { get; set; }

    public Bitmap Image { get; }

    protected SimpleObject(Side camp, Vecteur2d position, int lives, Bitmap image)
        : base(camp)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(image);

        Position = position;
        Lives = lives;
        Image = image;
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