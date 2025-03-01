public interface IConnectinable
{
    public ConnectionType Connection { get; }
    
    public enum ConnectionType
    {
        Ambivalent = 10,
        Solo = 20,
        Duo = 30,
    }
}
