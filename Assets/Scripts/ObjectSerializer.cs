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
            // TODO: Write by the field attribute name, not by the variable name
            PlayerEditableAttribute attr = field.GetCustomAttributes(typeof(PlayerEditableAttribute), true)[0] as PlayerEditableAttribute;
            bw.Write(attr.Name);
            object value = field.GetValue(component);
            if (field.FieldType == typeof(int))
                bw.Write((int)value);
            else if (field.FieldType == typeof(bool))
                bw.Write((bool)value);
            else if (field.FieldType == typeof(Guid))
                bw.Write((Guid)value);
            else if (field.FieldType.IsEnum)
                bw.Write(Enum.GetName(field.FieldType, value));
            else
                Debug.LogWarning("Couldn't serialize field: " + field.Name);
            }
    }

    public static void Deserialize(BinaryReader br, MonoBehaviour component)
    {
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            // Handle collections
            string fieldName = br.ReadString();
            FieldInfo[] fields = component.GetType().GetFields();
            FieldInfo field = Array.Find(fields, (f) =>
            {
                if (f.Name == fieldName)
                    return true;
                foreach (PlayerEditableAttribute attr in f.GetCustomAttributes(typeof(PlayerEditableAttribute), true))
                    if (attr.Name == fieldName)
                        return true;
                return false;
            });
            if (field == null)
                continue;
            if (field.FieldType == typeof(int))
                field.SetValue(component, br.ReadInt32());
            else if (field.FieldType == typeof(bool))
                field.SetValue(component, br.ReadBoolean());
            else if (field.FieldType == typeof(Guid))
                field.SetValue(component, br.ReadGuid());
            else if (field.FieldType.IsEnum)
                field.SetValue(component, Enum.Parse(field.FieldType, br.ReadString()));
            else
                Debug.LogWarning("Couldn't deserialize field: " + field.Name);
        }
    }

}
