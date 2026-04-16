namespace SpaceInvader;

public class Bomb : SimpleObject
{
    private readonly Game game;
    private readonly EnemyBlock enemies;
    private readonly double explosionRadius = 120;
    private double detonationTimer = 2.5; // Explose après 2.5s ou en haut de l'écran

    public Bomb(Game game, EnemyBlock enemies, Vecteur2d position, Bitmap bombImage)
        : base(GameObject.Side.Ally, position, 1, bombImage)
    {
        this.game = game;
        this.enemies = enemies;
    }

    public override void Update(double deltaTimeSeconds)
    {
        if (!IsAlive())
            return;

        // Montée verticale de la bombe
        Position = new Vecteur2d(Position.X, Position.Y - 300 * deltaTimeSeconds);

        // Compte à rebours avant détonation
        detonationTimer -= deltaTimeSeconds;

        // Détone si atteint le haut ou timer écoulé
        if (Position.Y < -50 || detonationTimer <= 0)
        {
            Detonate();
        }
    }

    public override void Draw(Graphics g)
    {
        if (!IsAlive())
            return;

        // Dessine un gros carré jaune
        int size = 24;
        int x = (int)(Position.X - size / 2);
        int y = (int)(Position.Y - size / 2);

        g.FillRectangle(Brushes.Yellow, x, y, size, size);
        g.DrawRectangle(Pens.Orange, x, y, size, size);
    }

    protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
        // La bombe n'interagit pas avec les missiles normales
    }

    private void Detonate()
    {
        // Détruit les ennemis dans un rayon autour de la bombe
        enemies.DestroyEnemiesInRadius(Position, explosionRadius);

        // Crée une explosion visuelle
        var explosionImage = Game.CreateEnemyExplosionImage();
        var explosion = new Explosion(Position, explosionImage);
        game.AddObject(explosion);

        // Joue le son de l'explosion
        Game.PlayInvaderKilledSound();

        // La bombe disparaît
        TakeDamage(1);
    }

    private void TakeDamage(int damage)
    {
        Lives = Math.Max(0, Lives - damage);
    }
}