using System;

public class PlayerEditableAttribute : Attribute
{
    private string name;
    public string Name { get { return name; } }

    public PlayerEditableAttribute(string name)
    {
        this.name = name;
    }
}

public class PlayerEditableRangeAttribute : PlayerEditableAttribute
{
    private int min;
    public int Min { get { return min; } }

    private int max;
    public int Max { get { return max; } }

    public PlayerEditableRangeAttribute(string name, int min, int max) : base(name)
    {
        this.min = min;
        this.max = max;
    }
}

public class PlayerEditableEnumAttribute : PlayerEditableAttribute
{
    private Type type;
    public string[] Choices {  get { return Enum.GetNames(type); } }

    public PlayerEditableEnumAttribute(string name, Type type) : base(name)
    {
        this.type = type;
    }

}
