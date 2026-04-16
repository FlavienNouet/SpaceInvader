namespace SpaceInvader;

public class Bunker : SimpleObject
{

    public Bunker(Vecteur2d position)
        : base(GameObject.Side.Neutral, position, 3, Game.CreateBunkerImage())
    {
    }

    public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        if (!IsAlive() || !missile.IsAlive() || Camp == missile.Camp || ReferenceEquals(this, missile))
        {
            return;
        }

        Rectangle bunkerRectangle = new((int)Math.Floor(Position.X), (int)Math.Floor(Position.Y), Image.Width, Image.Height);
        Rectangle missileRectangle = new((int)Math.Floor(missile.Position.X), (int)Math.Floor(missile.Position.Y), missile.Image.Width, missile.Image.Height);

        if (!bunkerRectangle.IntersectsWith(missileRectangle))
        {
            return;
        }

        bool hasCollision = false;

        for (int missileLocalY = 0; missileLocalY < missile.Image.Height && !hasCollision; missileLocalY++)
        {
            for (int missileLocalX = 0; missileLocalX < missile.Image.Width; missileLocalX++)
            {
                Color missilePixel = missile.Image.GetPixel(missileLocalX, missileLocalY);

                if (missilePixel.A == 0)
                {
                    continue;
                }

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

                Image.SetPixel(bunkerLocalX, bunkerLocalY, Color.Transparent);
                hasCollision = true;
                break;
            }
        }

        if (hasCollision)
        {
            missile.Lives = Math.Max(0, missile.Lives - 1);
        }
    }

    public override void Update(double deltaTimeSeconds)
    {
    }

}