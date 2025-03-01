using UnityEngine;

public class DebugReaction : AmbivalentReaction
{
    [SerializeField] private string _message;

    public override void Execute()
    {
        _message.Log(Color.green);
    }
}