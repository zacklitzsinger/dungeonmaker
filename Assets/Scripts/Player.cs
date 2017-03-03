using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Rolling,
    AttackWindup,
    Attacking,
    // Below are time independent states (not dictated by frame count)
    Shadow,
    Victory
}

public struct PlayerAction
{
    public PlayerState type;
    public int frames;
    public Vector2 vector;
    public bool combo;
    public string variation;

    public override string ToString()
    {
        return "[" + type + "|" + frames + "|" + vector + "]";
    }
}

public class Player : MonoBehaviour
{

    public GameObject swordPrefab;
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
    [ReadOnly]
    public int currentActionFrames; // How many frames have passed under the current action

    public int keys = 0;
    public IItem[] Items { get { return GetComponentsInChildren<IItem>(); } }
    public string[] keybinds;

    public Vector2 roomEntrance;

    public PlayerAction currentAction;
    public PlayerAction LastAction { get { return actions.Count > 0 ? actions.Last.Value : currentAction; } }
    public LinkedList<PlayerAction> actions = new LinkedList<PlayerAction>();

    // Items
    [ReadOnly]
    public bool shadow = false;

    GameObject playerPanel;
    HealthUI healthIndicator;
    KeyUI keyIndicator;

    Rigidbody2D rb2d;
    Health health;
    Gravity gravity;
    SpriteRenderer spriteRenderer;
    Animator animator;
    Shield shield;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        health = GetComponentInChildren<Health>();
        gravity = GetComponentInChildren<Gravity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        shield = GetComponentInChildren<Shield>(true);
        animator = GetComponent<Animator>();
        playerPanel = LevelEditor.main.playerPanel;
        healthIndicator = playerPanel.GetComponentInChildren<HealthUI>(true);
        keyIndicator = playerPanel.GetComponentInChildren<KeyUI>(true);
    }

    /// <summary>
    /// Called the first frame an action becomes active
    /// </summary>
    public void TriggerAction(PlayerAction action)
    {
        if (action.combo)
            combo++;
        if (action.type != PlayerState.Shadow)
            shadow = false;
        switch (action.type)
        {
            case PlayerState.AttackWindup:
                gravity.dragModifier = 1;
                if (action.vector.magnitude > 0)
                    rb2d.AddForce(action.vector * (combo + 2) / 3f);
                break;

            case PlayerState.Attacking:
                if (action.vector.magnitude > 0)
                {
                    Quaternion rotation = Quaternion.LookRotation((combo % 2 == 1 ? Vector3.forward : Vector3.back), action.vector.normalized);
                    Sword sword = Instantiate(swordPrefab, transform.position, rotation, transform).GetComponentInChildren<Sword>();
                    sword.owner = gameObject;
                    if (action.variation == "thrust" || combo > 2)
                        sword.style = Sword.Style.Thrust;
                }
                break;

            case PlayerState.Rolling:
                gravity.dragModifier = 1;
                if (action.vector.magnitude > 0)
                    rb2d.AddForce(action.vector);
                break;

            case PlayerState.Shadow:
                shadow = !shadow;
                break;
        }
        currentActionFrames = 0;
        remStateFrames = action.frames;
    }

    bool CanQueueActions()
    {
        return (remStateFrames <= 15 && actions.Count == 0);
    }

    /// <summary>
    /// Backswing can be cancelled in a certain frame window if there are no additional actions queued.
    /// </summary>
    bool CanCancelBackswing()
    {
        return (1 < remStateFrames && remStateFrames < 10 && combo < maxCombo && currentAction.type != PlayerState.Idle && actions.Count == 0);
    }

    void TryCancelBackswing()
    {
        if (CanCancelBackswing())
            remStateFrames = 0;
    }

    void Update()
    {
        animator.SetBool("shadow", shadow);
    }

    void FixedUpdate()
    {
        currentActionFrames++;
        if (currentAction.type == PlayerState.Victory)
            return;

        UpdateLayer();

        if (LevelEditor.main.SetCurrentRoom(transform.position))
            roomEntrance = transform.position;

        // Handle floor
        GameObject floor = LevelEditor.main.GetGameObjectAtPointWithType(transform.position, ObjectType.Floor);
        FloorData floorData = null;
        if (floor != null)
            floorData = floor.GetComponent<FloorData>();
        gravity.dragModifier = 1;

        //Handle movement
        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetMotion = Vector2.right * xMotion + Vector2.up * yMotion;
        if (currentAction.type == PlayerState.AttackWindup || currentAction.type == PlayerState.Attacking || currentAction.type == PlayerState.Idle)
        {
            float modifiedAcceleration = acceleration;
            modifiedAcceleration *= (floorData ? floorData.accelerationModifier : 1);
            modifiedAcceleration *= (shield.enabled ? 0.5f : 1f);
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
                currentAction = actions.First.Value;
                actions.RemoveFirst();
                TriggerAction(currentAction);
            }
            else
            {
                currentAction = new PlayerAction();
                if (combo > 0)
                {
                    remStateFrames = combo * postComboCooldown;
                    combo = 0;
                }
            }
            currentActionFrames = 0;
        }

        shield.gameObject.SetActive(false);

        if (!CanQueueActions())
            return;

        if (Input.GetButtonUp("Roll") && targetMotion.magnitude > 0)
        {
            TryCancelBackswing();
            actions.AddLast(new PlayerAction() { type = PlayerState.Rolling, frames = rollFrames, vector = targetMotion.normalized * rollForce, combo = true });
        }

        if (Input.GetButtonUp("Attack"))
        {
            TryCancelBackswing();
            string variation = "";
            if (LastAction.type == PlayerState.Rolling)
                variation = "thrust";
            Vector2 targetDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
            actions.AddLast(new PlayerAction() { type = PlayerState.AttackWindup, frames = attackWindup, vector = targetDirection * attackForce });
            actions.AddLast(new PlayerAction() { type = PlayerState.Attacking, frames = attackFrames, vector = targetDirection, combo = true, variation = variation });
        }

        if (Input.GetButton("Shield") && currentAction.type == PlayerState.Idle && currentAction.frames <= 0)
        {
            shield.gameObject.SetActive(true);
        }

        if (Input.GetButtonDown("Use item") && Items.Length >= 1)
        {
            Items[0].Activate(this);
        }
    }

    void UpdateLayer()
    {
        LayerMask targetLayer = shadow ? LayerMask.NameToLayer("Shadow") : LayerMask.NameToLayer("Player");
        gameObject.layer = targetLayer;
        foreach (Transform t in transform)
            t.gameObject.layer = targetLayer;
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
                itemSlots[i].ItemSprite = swordPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
            else if (i == 1)
                itemSlots[i].ItemSprite = shield.GetComponent<SpriteRenderer>().sprite;
            else if (i == 2)
                itemSlots[i].ItemSprite = dodgeIcon;
            else if (i - 3 < Items.Length)
            {
                itemSlots[i].ItemSprite = Items[i - 3].Icon;
            }
        }
    }
}
