namespace DesktopFly.Simulation.Environment;

/// <summary>Small immutable vector in virtual-desktop pixel coordinates.</summary>
public readonly record struct Vector2(double X, double Y)
{
    public static Vector2 Zero => new(0, 0);
    public double Length => Math.Sqrt(X * X + Y * Y);
    public Vector2 Normalized => Length < 1e-9 ? Zero : this / Length;
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 a, double k) => new(a.X * k, a.Y * k);
    public static Vector2 operator /(Vector2 a, double k) => new(a.X / k, a.Y / k);
    public static double Distance(Vector2 a, Vector2 b) => (a - b).Length;
    public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max) =>
        new(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
}
