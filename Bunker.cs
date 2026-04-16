namespace SpaceInvader;
/// <summary>
///  Cette classe représente un bunker dans notre jeu => bouclier x3 pour les joueurs
/// </summary>
public class Bunker : SimpleObject
{
    // Rayon de detruction en pixels autour du point d'impact
    private const int ImpactRadiusPixels = 10;
    private readonly Game? game;

    public Bunker(Vecteur2d position)
        : base(GameObject.Side.Neutral, position, 3, Game.CreateBunkerImage())
    {
    }

    public Bunker(Vecteur2d position, Game game)
        : base(GameObject.Side.Neutral, position, 3, Game.CreateBunkerImage())
    {
        this.game = game;
    }

    // Mise en place des Collisions pour les bunkers 
    protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
    }

    // Collision entre Missile et bBunker
    public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        // Vérification de départ => en vie, même camp, même objet
        if (!IsAlive() || !missile.IsAlive() || Camp == missile.Camp || ReferenceEquals(this, missile))
        {
            return;
        }

        // Mise en place de Bounding Box => Missile + Bunker
        Rectangle bunkerRectangle = new((int)Math.Floor(Position.X), (int)Math.Floor(Position.Y), Image.Width, Image.Height);
        Rectangle missileRectangle = new((int)Math.Floor(missile.Position.X), (int)Math.Floor(missile.Position.Y), missile.Image.Width, missile.Image.Height);

        if (!bunkerRectangle.IntersectsWith(missileRectangle))
        {
            return;
        }

        // Vérification pixel par pixel pour trouver le point d'impact
        bool hasCollision = false;
        int impactScreenX = 0;
        int impactScreenY = 0;

        for (int missileLocalY = 0; missileLocalY < missile.Image.Height && !hasCollision; missileLocalY++)
        {
            for (int missileLocalX = 0; missileLocalX < missile.Image.Width; missileLocalX++)
            {
                Color missilePixel = missile.Image.GetPixel(missileLocalX, missileLocalY);

                // Si le pixel du missile est transparent, on continue
                if (missilePixel.A == 0)
                {
                    continue;
                }

                // Conversion des données locales en données écrans pour trouver le pixel correspondant dans le bunker
                int screenX = missileRectangle.Left + missileLocalX;
                int screenY = missileRectangle.Top + missileLocalY;
                int bunkerLocalX = screenX - bunkerRectangle.Left;
                int bunkerLocalY = screenY - bunkerRectangle.Top;

                if (bunkerLocalX < 0 || bunkerLocalY < 0 || bunkerLocalX >= Image.Width || bunkerLocalY >= Image.Height)
                {
                    continue;
                }

                Color bunkerPixel = Image.GetPixel(bunkerLocalX, bunkerLocalY);

                if (bunkerPixel.A == 0)
                {
                    continue;
                }

                // On creuse le bunker au lieu d'impacter le missile => destruction de tous les pixels dans un rayon autour du point d'impact
                DestroyPixelsInRadius(bunkerLocalX, bunkerLocalY, ImpactRadiusPixels);
                impactScreenX = screenX;
                impactScreenY = screenY;
                hasCollision = true;
                break;
            }
        }

        // Conséquence de la collision => perte de vie du missile + explosion à l'écran
        if (hasCollision)
        {
            missile.Lives = Math.Max(0, missile.Lives - 1);

            // Explosion visuelle
            if (game is not null)
            {
                Bitmap impactImage = Game.CreateImpactExplosionImage();
                Vecteur2d explosionPosition = new(
                    impactScreenX - impactImage.Width / 2.0,
                    impactScreenY - impactImage.Height / 2.0);

                game.AddObject(new Explosion(explosionPosition, impactImage));
            }
        }
    }

    // Méthode pour détruire les pixels du bunker dans un rayon autour du point d'impact
    private void DestroyPixelsInRadius(int centerX, int centerY, int radius)
    {
        int radiusSquared = radius * radius;

        for (int y = Math.Max(0, centerY - radius); y <= Math.Min(Image.Height - 1, centerY + radius); y++)
        {
            for (int x = Math.Max(0, centerX - radius); x <= Math.Min(Image.Width - 1, centerX + radius); x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;

                if (dx * dx + dy * dy > radiusSquared)
                {
                    continue;
                }

                Color pixel = Image.GetPixel(x, y);
                if (pixel.A > 0)
                {
                    Image.SetPixel(x, y, Color.Transparent);
                }
            }
        }
    }

// Les bunkers ne bougent pas => pas de mise à jour nécessaire
    public override void Update(double deltaTimeSeconds)
    {
    }

}