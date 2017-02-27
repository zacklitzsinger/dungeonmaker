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
    public Sprite dodgeIcon;

    public float acceleration;

    // Rolling
    public int rollFrames; // Number of frames it takes to roll
    public float rollForce; // Force with which to roll

    // Melee combat
    public int attackWindup; // Max number of frames to actually start attacking
    public int attackFrames; // Number of frames after attack starts before player can take another action.
    public float attackForce; // Force to apply when attacking

    // Combo
    public int maxCombo; // Max number of attacks to be done in sequence
    [ReadOnly]
    public int combo; // Current combo

    [ReadOnly]
    public int remStateFrames; // Remaining frames to continue current state; 0 when in idle

    public int keys = 0;
    public IItem[] Items { get { return GetComponentsInChildren<IItem>(); } }
    public string[] keybinds;

    public Vector2 roomEntrance;

    public PlayerState state = PlayerState.Idle;

    public GameObject playerPanel;
    public HealthUI healthIndicator;
    public KeyUI keyIndicator;

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
        playerPanel = LevelEditor.main.playerPanel;
        healthIndicator = playerPanel.GetComponentInChildren<HealthUI>(true);
        keyIndicator = playerPanel.GetComponentInChildren<KeyUI>(true);
    }

    void Attack()
    {
        if (state != PlayerState.AttackWindup)
            return;
        Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        targetDirection.Normalize();
        Instantiate(sword, transform.position, Quaternion.LookRotation((combo % 2 == 1 ? Vector3.forward: Vector3.back), targetDirection), transform);
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
            if (remStateFrames > 10 || remStateFrames < 2 || combo >= maxCombo || state == PlayerState.Idle)
            {
                // If the player isn't intentionally inputting buttons and just mashing, we should prevent the combo.
                if (Input.GetButtonDown("Attack") || Input.GetButtonDown("Roll"))
                    combo = maxCombo;
                return;
            }
            // Don't allow player to cancel attack windup
            if (state == PlayerState.AttackWindup)
                return;
            // Visual effect for combo opportunity
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
                remStateFrames = combo * 2;
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
            gravity.dragModifier = 1;
            rb2d.AddForce(targetMotion.normalized * attackForce * (combo + 3) / 3f);
            combo++;
        }

        if (Input.GetButtonDown("Use item") && Items.Length >= 1)
        {
            Items[0].Activate(this);
        }
    }

    void OnGUI()
    {
        if (LevelEditor.main.mode >= EditMode.Create)
        {
            playerPanel.SetActive(false);
            return;
        }
        playerPanel.SetActive(true);
        healthIndicator.Amount = health.currentHealth;
        keyIndicator.Amount = keys;
        ItemSlot[] itemSlots = playerPanel.GetComponentsInChildren<ItemSlot>();
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < keybinds.Length)
                itemSlots[i].slot.text = keybinds[i];
            if (i == 0)
                itemSlots[i].ItemSprite = sword.GetComponentInChildren<SpriteRenderer>().sprite;
            else if (i == 1)
                itemSlots[i].ItemSprite = dodgeIcon;
            else if (i - 2 < Items.Length)
            {
                itemSlots[i].ItemSprite = Items[i - 2].Icon;
            }
        }
    }
}
