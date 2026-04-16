namespace SpaceInvader;
/// <summary>
/// Représente un objet simple dans le jeu, comme un missile ou un vaisseau. Il possède une position, des vies, une image pour l'affichage et une logique de collision basée sur les pixels. Les classes dérivées doivent implémenter la méthode OnCollision pour définir le comportement spécifique lors d'une collision avec un missile.
/// </summary>
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

    // Objet considéré comme vivant tant qu'il a au moins une vie restante
    public override bool IsAlive()
    {
        return Lives > 0;
    }

    // Collision basée sur les pixels entre ce SimpleObject et un missile
    public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        if (!IsAlive() || !missile.IsAlive() || Camp == missile.Camp || ReferenceEquals(this, missile))
        {
            return;
        }

        // Calculer les rectangles englobants de l'objet et du missile pour une vérification rapide de l'intersection
        Rectangle objectRectangle = GetObjectRectangle(Position, Image.Width, Image.Height);
        Rectangle missileRectangle = GetObjectRectangle(missile.Position, missile.Image.Width, missile.Image.Height);

        if (!objectRectangle.IntersectsWith(missileRectangle))
        {
            return;
        }

        int numberOfPixelsInCollision = 0;

        // Boucle de collision pixel par pixel entre le missile et l'objet
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