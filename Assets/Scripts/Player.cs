using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Rolling,
    AttackWindup,
    Attacking,
    Victory
}

public class Player : MonoBehaviour
{

    Rigidbody2D rb2d;
    public GameObject sword;
    public Bullet bullet;
    public Texture2D healthTexture;
    public Texture2D keyTexture;
    Health health;

    public float acceleration;
    public int rollFrames; // Number of frames it takes to roll
    public float rollForce; // Force with which to roll
    public int minAttackWindup; //Min number of frames to actually start attacking
    public int maxAttackWindup; // Max number of frames to hold attack.
    [ReadOnly]
    public int attackFrames; // Number of frames to attack.
    public int shotFrames; // Number of frames to idle after shooting

    [ReadOnly]
    public int remStateFrames; // Remaining frames to continue current state; 0 when in idle

    public int keys = 0;
    public IItem[] Items { get { return GetComponentsInChildren<IItem>(); } }

    public Vector2 roomEntrance;

    public PlayerState state = PlayerState.Idle;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        health = GetComponentInChildren<Health>();
        attackFrames = sword.GetComponentInChildren<Sword>().remainingFrames;
    }

    void Attack()
    {
        if (state != PlayerState.AttackWindup)
            return;
        Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        targetDirection.Normalize();
        Instantiate(sword, transform.position, Quaternion.LookRotation(Vector3.forward, targetDirection), transform);
        state = PlayerState.Attacking;
        remStateFrames = attackFrames;
    }

    void FixedUpdate()
    {
        if (state == PlayerState.Victory)
            return;

        if (LevelEditor.main.SetCurrentRoom(transform.position))
            roomEntrance = transform.position;

        GameObject floor = LevelEditor.main.GetGameObjectAtPointWithType(transform.position, ObjectType.Floor);
        FloorData floorData = null;
        if (floor != null)
            floorData = floor.GetComponent<FloorData>();

        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetMotion = Vector2.right * xMotion + Vector2.up * yMotion;

        if (remStateFrames > 0)
        {
            remStateFrames--;
            switch (state)
            {
                case PlayerState.Rolling:
                    rb2d.AddForce(targetMotion.normalized * rollForce / (rollFrames - remStateFrames + 1));
                    break;
                case PlayerState.AttackWindup:
                    if (remStateFrames <= maxAttackWindup - minAttackWindup && !Input.GetButton("Attack"))
                    {
                        Attack();
                        break;
                    }
                    Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    targetDirection.Normalize();
                    rb2d.AddForce(targetDirection * 7 * remStateFrames);
                    break;
            }
            return;
        }
        else
        {
            remStateFrames = 0;
            if (state == PlayerState.AttackWindup)
                Attack();
            state = PlayerState.Idle;
        }

        if (Input.GetButtonDown("Roll") && state == PlayerState.Idle)
        {
            remStateFrames = rollFrames;
            state = PlayerState.Rolling;
        }
        else
        {
            float modifiedAcceleration = (floorData ? floorData.accelerationModifier : 1) * acceleration;
            rb2d.AddForce((targetMotion.magnitude > 1 ? targetMotion.normalized : targetMotion) * modifiedAcceleration);
        }

        if (Input.GetButtonDown("Attack") && state == PlayerState.Idle)
        {
            remStateFrames = maxAttackWindup;
            state = PlayerState.AttackWindup;
        }

        if (Input.GetButton("Shoot"))
        {
            Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            targetDirection.Normalize();
            Bullet newBullet = Instantiate(bullet, transform.position, Quaternion.LookRotation(Vector3.forward, targetDirection));
            remStateFrames = shotFrames;
        }

        if (Input.GetButtonDown("Use item") && Items.Length >= 1)
        {
            Items[0].Activate(this);
        }
    }

    void OnGUI()
    {
        if (LevelEditor.main.mode == EditMode.Play)
        {
            for (int i = 1; i <= health.currentHealth; i++)
                GUI.DrawTexture(new Rect(new Vector2(Screen.width - 20 - i * healthTexture.width, 20), new Vector2(healthTexture.width, healthTexture.height)), healthTexture);
            for (int i = 1; i <= keys; i++)
                GUI.DrawTexture(new Rect(new Vector2(Screen.width - 20 - i * keyTexture.width, 20), new Vector2(keyTexture.width, keyTexture.height)), keyTexture);
        }
    }
}
