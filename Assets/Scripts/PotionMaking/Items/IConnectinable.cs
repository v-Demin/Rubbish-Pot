public interface IConnectinable
{
    public ConnectionType Connection { get; }
    
    public enum ConnectionType
    {
        Ambivalent = 0,
        Solo = 1,
        Duo = 2,
    }
}
