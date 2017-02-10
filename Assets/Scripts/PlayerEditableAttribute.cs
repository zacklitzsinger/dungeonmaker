using System;

public class PlayerEditableAttribute : Attribute {
    private string name;
    public string Name { get { return name; } }

    public PlayerEditableAttribute(string name)
    {
        this.name = name;
    }
}
