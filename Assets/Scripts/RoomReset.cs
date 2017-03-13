using System.IO;
using UnityEngine;

public class RoomReset : MonoBehaviour, ICustomSerializable
{

    [PlayerEditable("Reset when entering room")]
    public bool reset;

    Vector2 initialPosition;
    Health health;

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

        health = GetComponentInChildren<Health>();
    }

    void ResetSelf()
    {
        gameObject.SetActive(true);
        transform.position = initialPosition;
        health.currentHealth = health.maxHealth;
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
