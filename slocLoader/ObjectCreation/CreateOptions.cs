namespace slocLoader.ObjectCreation;

public struct CreateOptions
{

    public Vector3 Position;

    public Quaternion Rotation;

    public Vector3 Scale;

    public byte? MovementSmoothing;

    public bool IsStatic;

    public byte RootSmoothing;

    public bool StaticRoot;

    public GameObject Parent;

    public CreateOptions()
    {
        Position = Vector3.one;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;
        MovementSmoothing = null;
        IsStatic = false;
        RootSmoothing = 0;
        StaticRoot = false;
        Parent = null;
    }

}
