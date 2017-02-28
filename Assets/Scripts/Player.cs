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

public struct PlayerAction
{
    public PlayerState type;
    public int frames;
    public Vector2 direction;
    public bool combo;

    public override string ToString()
    {
        return "[" + type + "|" + frames + "|" + direction + "]";
    }
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
    public int postComboCooldown; // Number of frames to idle per combo executed after comboing.

    [ReadOnly]
    public int remStateFrames; // Remaining frames to continue current state; 0 when in idle

    public int keys = 0;
    public IItem[] Items { get { return GetComponentsInChildren<IItem>(); } }
    public string[] keybinds;

    public Vector2 roomEntrance;

    public PlayerAction currentAction;
    public Queue<PlayerAction> actions = new Queue<PlayerAction>();

    GameObject playerPanel;
    HealthUI healthIndicator;
    KeyUI keyIndicator;

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
        playerPanel = LevelEditor.main.playerPanel;
        healthIndicator = playerPanel.GetComponentInChildren<HealthUI>(true);
        keyIndicator = playerPanel.GetComponentInChildren<KeyUI>(true);
    }

    /// <summary>
    /// Called the first frame an action becomes active
    /// </summary>
    void TriggerAction(PlayerAction action)
    {
        remStateFrames = action.frames;
        if (action.combo)
            combo++;
        switch (action.type)
        {
            case PlayerState.AttackWindup:
                gravity.dragModifier = 1;
                if (action.direction.magnitude > 0)
                    rb2d.AddForce(action.direction.normalized * attackForce * (combo + 3) / 3f);
                break;

            case PlayerState.Attacking:
                if (action.direction.magnitude > 0)
                {
                    Quaternion rotation = Quaternion.LookRotation((combo % 2 == 1 ? Vector3.forward : Vector3.back), action.direction.normalized);
                    GameObject swordObj = Instantiate(sword, transform.position, rotation, transform);
                    swordObj.GetComponentInChildren<Sword>().owner = gameObject;
                }
                break;

            case PlayerState.Rolling:
                gravity.dragModifier = 1;
                if (action.direction.magnitude > 0)
                    rb2d.AddForce(action.direction.normalized * rollForce);
                break;
        }
    }

    /// <summary>
    /// Backswing can be cancelled in a certain frame window if there are no additional actions queued.
    /// </summary>
    bool CanCancelBackswing()
    {
        return (3 < remStateFrames && remStateFrames < 11 && combo < maxCombo && currentAction.type != PlayerState.Idle && actions.Count == 0);
    }

    void TryCancelBackswing()
    {
        if (CanCancelBackswing())
            remStateFrames = 0;
    }

    void FixedUpdate()
    {
        if (currentAction.type == PlayerState.Victory)
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
        if (currentAction.type == PlayerState.AttackWindup || currentAction.type == PlayerState.Attacking || currentAction.type == PlayerState.Idle)
        {
            float modifiedAcceleration = (floorData ? floorData.accelerationModifier : 1) * acceleration;
            rb2d.AddForce((targetMotion.magnitude > 1 ? targetMotion.normalized : targetMotion) * modifiedAcceleration);
        }

        spriteRenderer.color = Color.white;
        if (remStateFrames > 0)
        {
            remStateFrames--;
            switch (currentAction.type)
            {
                case PlayerState.Rolling:
                    if (remStateFrames < rollFrames / 4)
                        gravity.dragModifier = 5;
                    break;
            }
            if (CanCancelBackswing())
                // Visual effect for combo opportunity
                spriteRenderer.color = new Color(.95f, .9f, 1f);
        }
        else
        {
            remStateFrames = 0;
            if (combo >= maxCombo)
                actions.Clear();
            if (actions.Count > 0)
            {
                currentAction = actions.Dequeue();
                TriggerAction(currentAction);
            }
            else if (combo > 0)
            {
                currentAction = new PlayerAction();
                remStateFrames = combo * postComboCooldown;
                combo = 0;
            }
        }

        if (Input.GetButtonUp("Roll") && targetMotion.magnitude > 0)
        {
            TryCancelBackswing();
            actions.Enqueue(new PlayerAction() { type = PlayerState.Rolling, frames = rollFrames, direction = targetMotion.normalized, combo = true });

        }

        if (Input.GetButtonUp("Attack"))
        {
            TryCancelBackswing();
            Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            actions.Enqueue(new PlayerAction() { type = PlayerState.AttackWindup, frames = attackWindup, direction = targetDirection.normalized });
            actions.Enqueue(new PlayerAction() { type = PlayerState.Attacking, frames = attackFrames, direction = targetDirection.normalized, combo = true });
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
