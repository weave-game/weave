namespace weave.Multiplayer;

public enum MessageType
{
    StartGame,
    EndGame,
    Error,
    Success
}

public struct Message
{
    public MessageType MessageType;
    public string Data;

    public Message(MessageType messageType, string data = "")
    {
        MessageType = messageType;
        Data = data;
    }
}
