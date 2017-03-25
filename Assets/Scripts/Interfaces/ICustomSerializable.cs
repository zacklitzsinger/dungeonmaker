using System.IO;
using UnityEngine;

public interface ICustomSerializable
{
    void Serialize(BinaryWriter bw);

    void Deserialize(BinaryReader br);
}
