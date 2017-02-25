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

    public GameObject sword;
    public Bullet bullet;
    public Texture2D healthTexture;
    public Texture2D keyTexture;

    public float acceleration;

    // Rolling
    public int rollFrames; // Number of frames it takes to roll
    public float rollForce; // Force with which to roll

    // Melee combat
    public int attackWindup; // Max number of frames to actually start attacking
    public int attackFrames; // Number of frames after attack starts before player can take another action.
    public int maxCombo; // Max number of attacks to be done in sequence
    [ReadOnly]
    public int combo; // Current combo

    public int shotFrames; // Number of frames to idle after shooting

    [ReadOnly]
    public int remStateFrames; // Remaining frames to continue current state; 0 when in idle

    public int keys = 0;
    public IItem[] Items { get { return GetComponentsInChildren<IItem>(); } }

    public Vector2 roomEntrance;

    public PlayerState state = PlayerState.Idle;

    Rigidbody2D rb2d;
    Health health;
    Gravity gravity;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        health = GetComponentInChildren<Health>();
        gravity = GetComponentInChildren<Gravity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        attackFrames = sword.GetComponentInChildren<Sword>().remainingFrames;
    }

    void Attack()
    {
        if (state != PlayerState.AttackWindup)
            return;
        Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        targetDirection.Normalize();
        Instantiate(sword, transform.position, Quaternion.LookRotation((combo % 2 == 0 ? Vector3.forward: Vector3.back), targetDirection), transform);
        state = PlayerState.Attacking;
        remStateFrames = attackFrames;
        combo++;
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
        gravity.dragModifier = 1;

        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetMotion = Vector2.right * xMotion + Vector2.up * yMotion;
        if (state == PlayerState.Attacking || state == PlayerState.Idle)
        {
            float modifiedAcceleration = (floorData ? floorData.accelerationModifier : 1) * acceleration;
            rb2d.AddForce((targetMotion.magnitude > 1 ? targetMotion.normalized : targetMotion) * modifiedAcceleration);
        }

        spriteRenderer.color = Color.white;
        if (remStateFrames > 0)
        {
            remStateFrames--;
            switch (state)
            {
                case PlayerState.Rolling:
                    if (remStateFrames < rollFrames / 4)
                        gravity.dragModifier = 5;
                    break;
            }
            if (remStateFrames > 8 || remStateFrames < 3 || combo >= maxCombo)
                return;
            spriteRenderer.color = new Color(.95f, .9f, 1f); 
        }
        else
        {
            remStateFrames = 0;
            if (state == PlayerState.AttackWindup)
            {
                Attack();
                return;
            }
            else
            {
                state = PlayerState.Idle;
                combo = 0;
            }
        }

        if (Input.GetButtonDown("Roll") && targetMotion.magnitude > 0)
        {
            remStateFrames = rollFrames;
            state = PlayerState.Rolling;
            gravity.dragModifier = 1;
            rb2d.AddForce(targetMotion.normalized * rollForce);
            combo++;
        }

        if (Input.GetButtonDown("Attack") && state != PlayerState.AttackWindup)
        {
            remStateFrames = attackWindup;
            state = PlayerState.AttackWindup;
        }

        //if (Input.GetButton("Shoot"))
        //{
        //    Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        //    targetDirection.Normalize();
        //    Instantiate(bullet, transform.position, Quaternion.LookRotation(Vector3.forward, targetDirection));
        //    remStateFrames = shotFrames;
        //}

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
