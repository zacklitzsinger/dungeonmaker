using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BinaryExtensions {

    public static void Write(this BinaryWriter bw, Guid guid)
    {
        bw.Write(guid.ToByteArray());
    }

    public static Guid ReadGuid(this BinaryReader br)
    {
        return new Guid(br.ReadBytes(16));
    }
}
