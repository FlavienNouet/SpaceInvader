namespace SpaceInvader;
/// <summary>
/// Représente un vaisseau dans le jeu, qui peut être contrôlé par le joueur ou être un ennemi. Le vaisseau peut se déplacer horizontalement, tirer des missiles et subir des collisions basées sur les pixels avec les missiles. Il dispose également d'une animation de tir et d'un son de déplacement pour améliorer l'expérience de jeu.
/// </summary>
public class SpaceShip : SimpleObject
{
    protected double PlayerSpeedPixelPerSecond { get; }
    protected Size GameSize { get; }
    private readonly Game? game;

    // Liste des missiles actifs tirés par ce vaisseau, utilisée pour limiter le nombre de missiles à l'écran et gérer leur cycle de vie
    private readonly List<Missile> activeMissiles = new();
    private Bitmap? alternateImage;
    private double animationTimer;
    private const double AnimationFrameTime = 0.5; // Changer d'image toutes les 0.5 secondes pour l'animation de tir
    private bool useAlternateFrame;

    public SpaceShip(Vecteur2d position, int lives, Bitmap image)
        : this(GameObject.Side.Enemy, null, position, lives, image, Size.Empty)
    {
        }

    public SpaceShip(Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : this(GameObject.Side.Enemy, null, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
    }

    public SpaceShip(Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200, Bitmap? alternateImage = null)
        : this(GameObject.Side.Enemy, game, position, lives, image, gameSize, playerSpeedPixelPerSecond, alternateImage)
    {
    }

    protected SpaceShip(GameObject.Side camp, Game? game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200, Bitmap? alternateImage = null)
        : base(camp, position, lives, image)
    {
        this.game = game;

        GameSize = gameSize;
        PlayerSpeedPixelPerSecond = playerSpeedPixelPerSecond;
        this.alternateImage = alternateImage;
        animationTimer = 0;
        useAlternateFrame = false;
    }
    
    // Méthode Update pour gérer l'animation de tir en alternant entre l'image normale et l'image d'animation
    public override void Update(double deltaTimeSeconds)
    {
     if (alternateImage is not null)
        {
            animationTimer += deltaTimeSeconds;
            if (animationTimer >= AnimationFrameTime)
            {
                animationTimer -= AnimationFrameTime;
                useAlternateFrame = !useAlternateFrame;
            }
        }
    }

    // Dessine le vaisseau en utilisant l'image d'animation si elle est active, sinon l'image normale
    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        Bitmap imageToUse = useAlternateFrame && alternateImage is not null ? alternateImage : Image;
        graphics.DrawImage(imageToUse, new RectangleF((float)Position.X, (float)Position.Y, imageToUse.Width, imageToUse.Height));
    }

    protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
    }

    // Gère la logique de collision spécifique au vaisseau, en infligeant des dégâts en fonction du nombre de pixels en collision et en déclenchant une explosion si le vaisseau est détruit
    public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        //Test pour éviter les collisions inutiles
        if (!IsAlive() || !missile.IsAlive() || Camp == missile.Camp || ReferenceEquals(this, missile))
        {
            return;
        }

        Rectangle objectRectangle = GetObjectRectangle(Position, Image.Width, Image.Height);
        Rectangle missileRectangle = GetObjectRectangle(missile.Position, missile.Image.Width, missile.Image.Height);

        if (!objectRectangle.IntersectsWith(missileRectangle))
        {
            return;
        }

        // Collision pixel par pixel entre le missile et le vaisseau
        int numberOfPixelsInCollision = 0;

        for (int missileLocalY = 0; missileLocalY < missile.Image.Height; missileLocalY++)
        {
            for (int missileLocalX = 0; missileLocalX < missile.Image.Width; missileLocalX++)
            {
                Color missilePixel = missile.Image.GetPixel(missileLocalX, missileLocalY);

                if (missilePixel.A == 0)
                {
                    continue;
                }

                // Calculer les coordonnées du pixel dans l'image du vaisseau
                int screenX = missileRectangle.Left + missileLocalX;
                int screenY = missileRectangle.Top + missileLocalY;
                int objectLocalX = screenX - objectRectangle.Left;
                int objectLocalY = screenY - objectRectangle.Top;

                if (!IsInsideImage(objectLocalX, objectLocalY, Image.Width, Image.Height))
                {
                    continue;
                }

                Color objectPixel = Image.GetPixel(objectLocalX, objectLocalY);

                if (objectPixel.A == 0)
                {
                    continue;
                }

                Image.SetPixel(objectLocalX, objectLocalY, Color.Transparent);
                numberOfPixelsInCollision++;
            }
        }

        if (numberOfPixelsInCollision <= 0)
        {
            return;
        }

        int damage = Math.Min(Lives, missile.Lives);
        Lives = Math.Max(0, Lives - damage);
        missile.Lives = Math.Max(0, missile.Lives - damage);

        HandleDeath();
    }

    // Gère la logique de mort du vaisseau, en déclenchant une explosion et en ajoutant des points au score si le vaisseau est détruit
    private void HandleDeath()
    {
         if (!IsAlive() && game is not null && Camp == GameObject.Side.Enemy)
        {
            Bitmap explosionImage = Game.CreateEnemyExplosionImage();
            Explosion explosion = new Explosion(Position, explosionImage);
            game.AddObject(explosion);
            game.AddEnemyKillScore();
            Game.PlayInvaderKilledSound();
        }
    }

// Tire de missile depuis le vaisseau 
 public void Shoot(bool shootDownwards = false)
    {
        if (game is null)
        {
            return;
        }

        CleanupMissiles();
        if (activeMissiles.Count > 0)
        {
            return;
        }

        bool isPlayerShot = Camp == GameObject.Side.Ally && !shootDownwards;

        if (isPlayerShot && game.IsDoubleShotActive)
        {
            ShootDouble();
            return;
        }

        // Créer un missile avec une animation de tir et une option de homing pour les tirs du joueur
        Bitmap[] animationFrames = Game.CreateMissileAnimationFrames();
        Bitmap missileImage = animationFrames[0];
        Vecteur2d missilePosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0,
            shootDownwards ? Position.Y + Image.Height : Position.Y - missileImage.Height);

        double verticalDirection = shootDownwards ? 1 : -1;
        bool homingEnabled = isPlayerShot && game.IsHomingShotActive;

        Missile missile = new Missile(Camp, game, missilePosition, 1, missileImage, game.GameSize, 400, 0, verticalDirection, animationFrames, homingEnabled);
        activeMissiles.Add(missile);
        game.AddObject(missile);
        Game.PlayShootSound();
    }

    // Tire un missile vers une position cible spécifique, utilisé pour les tirs spéciaux du joueur
     public void ShootAt(Vecteur2d targetPosition)
    {
        if (game is null)
        {
            return;
        }

        CleanupMissiles();
        if (activeMissiles.Count > 0)
        {
            return;
        }

        // Créer un missile avec une animation de tir et une option de homing pour les tirs du joueur
        Bitmap missileImage = CreateMissileImage();
        Bitmap[] animationFrames = Game.CreateMissileAnimationFrames();
        Vecteur2d missilePosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0,
            Position.Y + Image.Height);

        double missileCenterX = missilePosition.X + missileImage.Width / 2.0;
        double missileCenterY = missilePosition.Y + missileImage.Height / 2.0;
        double directionX = targetPosition.X - missileCenterX;
        double directionY = targetPosition.Y - missileCenterY;

        Missile missile = new Missile(Camp, game, missilePosition, 1, missileImage, game.GameSize, 400, directionX, directionY, animationFrames, false);
        activeMissiles.Add(missile);
        game.AddObject(missile);
        Game.PlayShootSound();
    }

    // Bonus : Tire deux missiles simultanément avec un léger décalage horizontal, utilisé pour les tirs doubles du joueur
    private void ShootDouble()
    {
        if (game is null)
        {
            return;
        }

        Bitmap missileImage = CreateMissileImage();
        Bitmap[] animationFrames = Game.CreateMissileAnimationFrames();
        const double sideOffset = 10;
        bool homingEnabled = game.IsHomingShotActive;

        Vecteur2d leftPosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0 - sideOffset,
            Position.Y - missileImage.Height);
        Vecteur2d rightPosition = new(
            Position.X + (Image.Width - missileImage.Width) / 2.0 + sideOffset,
            Position.Y - missileImage.Height);

        Missile leftMissile = new Missile(Camp, game, leftPosition, 1, missileImage, game.GameSize, 400, 0, -1, animationFrames, homingEnabled);
        Missile rightMissile = new Missile(Camp, game, rightPosition, 1, missileImage, game.GameSize, 400, 0, -1, animationFrames, homingEnabled);

        activeMissiles.Add(leftMissile);
        activeMissiles.Add(rightMissile);
        game.AddObject(leftMissile);
        game.AddObject(rightMissile);
        Game.PlayShootSound();
    }

    // Nettoie la liste des missiles actifs en supprimant ceux qui ne sont plus vivants, pour éviter d'avoir des références à des missiles détruits
    private void CleanupMissiles()
    {
        activeMissiles.RemoveAll(m => !m.IsAlive());
    }

    private static Rectangle GetObjectRectangle(Vecteur2d position, int width, int height)
    {
        return new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), width, height);
    }

    private static bool IsInsideImage(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    protected static bool IsKeyDown(Keys key)
    {
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    private static Bitmap CreateMissileImage()
    {
       return Game.CreateMissileImage();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
