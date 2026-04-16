namespace SpaceInvader;

public abstract class SimpleObject : GameObject
{
    public Vecteur2d Position { get; set; }

    public int Lives { get; set; }

    public Bitmap Image { get; }

    protected SimpleObject(Vecteur2d position, int lives, Bitmap image)
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

    public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        if (!IsAlive() || !missile.IsAlive() || ReferenceEquals(this, missile))
        {
            return;
        }

        Rectangle objectRectangle = GetObjectRectangle(Position, Image.Width, Image.Height);
        Rectangle missileRectangle = GetObjectRectangle(missile.Position, missile.Image.Width, missile.Image.Height);

        if (!objectRectangle.IntersectsWith(missileRectangle))
        {
            return;
        }

        int numberOfPixelsInCollision = 0;

        for (int missileLocalY = 0; missileLocalY < missile.Image.Height; missileLocalY++)
        {
            for (int missileLocalX = 0; missileLocalX < missile.Image.Width; missileLocalX++)
            {
                Color missilePixel = missile.Image.GetPixel(missileLocalX, missileLocalY);

                if (!IsOpaquePixel(missilePixel))
                {
                    continue;
                }

                int screenX = missileRectangle.Left + missileLocalX;
                int screenY = missileRectangle.Top + missileLocalY;
                int objectLocalX = screenX - objectRectangle.Left;
                int objectLocalY = screenY - objectRectangle.Top;

                if (!IsInsideImage(objectLocalX, objectLocalY, Image.Width, Image.Height))
                {
                    continue;
                }

                Color objectPixel = Image.GetPixel(objectLocalX, objectLocalY);

                if (!IsOpaquePixel(objectPixel))
                {
                    continue;
                }

                Image.SetPixel(objectLocalX, objectLocalY, Color.Transparent);
                numberOfPixelsInCollision++;
            }
        }

        if (numberOfPixelsInCollision > 0)
        {
            OnCollision(missile, numberOfPixelsInCollision);
        }
    }

    protected abstract void OnCollision(Missile missile, int numberOfPixelsInCollision);

    private static Rectangle GetObjectRectangle(Vecteur2d position, int width, int height)
    {
        return new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), width, height);
    }

    private static bool IsInsideImage(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    private static bool IsOpaquePixel(Color pixel)
    {
        return pixel.A > 0;
    }
}