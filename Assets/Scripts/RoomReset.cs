using System.IO;
using UnityEngine;

public class RoomReset : MonoBehaviour, ICustomSerializable
{

    [PlayerEditable("Reset when entering room")]
    public bool reset;

    Vector2 initialPosition;
    IDamageable health;

    // Use this for initialization
    void Start()
    {
        initialPosition = transform.position;
        LevelEditor.main.onRoomChanged.AddListener(() =>
        {
            if (!reset)
                return;
            // If the player moved into a room that was not previously active, reset
            if (!LevelEditor.main.previousRoom.Contains(initialPosition) && LevelEditor.main.currentRoom.Contains(initialPosition))
                ResetSelf();
        });

        health = GetComponentInChildren<IDamageable>();
    }

    void ResetSelf()
    {
        gameObject.SetActive(true);
        transform.position = initialPosition;
        health.FullHeal();
    }

    public void Serialize(BinaryWriter bw)
    {
        ObjectSerializer.Serialize(bw, this);
    }

    public void Deserialize(BinaryReader br)
    {
        ObjectSerializer.Deserialize(br, this);
    }
}
