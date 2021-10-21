using UnityEngine;


public class Floor
{
    public Vector3 Point
    {
        get; private set;
    }

    public Vector3 Normal
    {
        get; private set;
    }

    public Plane Plane
    {
        get
        {
            return new Plane(Normal, Point);
        }
    }

    public Floor(Vector3 point, Vector3 normal)
    {
        Point = point;
        Normal = normal;
    }
}