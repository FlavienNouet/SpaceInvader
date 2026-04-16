namespace SpaceInvader;

public abstract class GameObject
{
    public abstract void Update(double deltaTimeSeconds);

    public abstract void Draw(Graphics graphics);

    public abstract bool IsAlive();

    public abstract void Collision(Missile missile);
}