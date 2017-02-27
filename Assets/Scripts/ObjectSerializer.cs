using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class ObjectSerializer
{

    public static void Serialize(BinaryWriter bw, MonoBehaviour component)
    {
        FieldInfo[] fields = Array.FindAll(component.GetType().GetFields(), (field) =>
        {
            return field.GetCustomAttributes(typeof(PlayerEditableAttribute), true).Length > 0;
        });
        bw.Write(fields.Length);
        foreach (FieldInfo field in fields)
        {
            // TODO: Would be nice to clean this up, but using an older version of C# hurts here.
            // TODO: Handle collections
            bw.Write(field.Name);
            object value = field.GetValue(component);
            if (field.FieldType == typeof(int))
                bw.Write((int)value);
            else if (field.FieldType == typeof(bool))
                bw.Write((bool)value);
            else if (field.FieldType == typeof(Guid))
                bw.Write((Guid)value);
            else if (field.FieldType == typeof(Enum))
                bw.Write((int)value);
            }
    }

    public static void Deserialize(BinaryReader br, MonoBehaviour component)
    {
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            // Handle collections
            string fieldName = br.ReadString();
            FieldInfo field = component.GetType().GetField(fieldName);
            if (field == null)
                continue;
            if (field.FieldType == typeof(int))
                field.SetValue(component, br.ReadInt32());
            else if (field.FieldType == typeof(bool))
                field.SetValue(component, br.ReadBoolean());
            else if (field.FieldType == typeof(Guid))
                field.SetValue(component, br.ReadGuid());
            else if (field.FieldType == typeof(Enum))
                field.SetValue(component, br.ReadInt32());
        }
    }

}
