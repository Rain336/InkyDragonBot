namespace InkyBot.Conversation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class RegisterCommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
    public string Description { get; set; }
}