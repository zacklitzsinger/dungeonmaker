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

public class PlayerAction
{
    public PlayerState type;
    public int frames;
    public Vector2 vector;
    public bool sticky; // Should action stay around even when frames == 0?

    public override string ToString()
    {
        return "[" + type + "|" + frames + "|" + vector + "]";
    }
}

public class Player : MonoBehaviour, IActionQueue
{

    public GameObject bulletPrefab;
    public Sprite dodgeIcon;

    public float acceleration;

    // Rolling
    public int rollFrames; // Number of frames it takes to roll
    public float rollForce; // Force with which to roll
    public float energyPerRoll;
    public float rollGravityModifier;

    // Energy
    public float maxEnergy;
    [ReadOnly]
    public float currentEnergy;
    public float energyRechargeRate;

    // Combat
    public int attackWindup; // Max number of frames to actually start attacking
    public int attackFrames; // Number of frames after attack starts before player can take another action.
    public float attackForce; // Force to apply when attacking
    public float energyPerShot; // Energy to consume per shot
    public float scatterAngle; // Degrees by which to randomly scatter shots
    public float attackMovementModifier; // % modifier on movement while attacking

    // Charging attack
    public float energyPerCharge; // energy consumed per frame by charging up an attack
    public int maxChargeFrames; // max frames to charge

    [ReadOnly]
    public bool aiming = false;
    public float aimingMovementModifier = 0.5f; // % modifier on movement while aiming
    public float aimingScatterModifier = 0.2f; // % modifier on bullet scatter range while aiming

    [ReadOnly]
    public int remStateFrames; // Remaining frames to continue current state; 0 when in idle
    [ReadOnly]
    public int currentActionFrames; // How many frames have passed under the current action
    [ReadOnly]
    public int framesSinceEnergyUsed;

    public int keys = 0;
    public IItem[] Items { get { return GetComponentsInChildren<IItem>(); } }
    public string[] keybinds;

    public Vector2 roomEntrance;

    public PlayerAction currentAction;
    public PlayerAction LastAction { get { return actions.Count > 0 ? actions[actions.Count - 1] : currentAction; } }
    public List<PlayerAction> actions = new List<PlayerAction>();

    // State
    [ReadOnly]
    public bool shadow = false;

    GameObject playerPanel;
    HealthUI healthIndicator;
    KeyUI keyIndicator;
    ProgressBar energyIndicator;

    Rigidbody2D rb2d;
    Health health;
    Gravity gravity;
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        health = GetComponentInChildren<Health>();
        gravity = GetComponentInChildren<Gravity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerPanel = LevelEditor.main.playerPanel;
        healthIndicator = playerPanel.GetComponentInChildren<HealthUI>(true);
        keyIndicator = playerPanel.GetComponentInChildren<KeyUI>(true);
        energyIndicator = playerPanel.GetComponentInChildren<ProgressBar>(true);
        currentEnergy = maxEnergy;
        currentAction = new PlayerAction();
    }

    /// <summary>
    /// Called the first frame an action becomes active
    /// </summary>
    public void TriggerAction(PlayerAction action)
    {
        remStateFrames = action.frames;
        if (action.type != PlayerState.Shadow)
            shadow = false;
        switch (action.type)
        {
            case PlayerState.AttackWindup:
                gravity.dragModifier = 1;
                if (action.vector.magnitude > 0)
                    rb2d.AddForce(action.vector);
                break;

            case PlayerState.Attacking:
                Vector2 targetDirection = (LevelEditor.main.GetXYPlanePosition(Input.mousePosition) - (Vector2)transform.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
                // Hacky way of doing a bell curve
                float angle = GetScatter() / 2 + GetScatter() / 2;
                Quaternion scatter = Quaternion.AngleAxis(angle, Vector3.forward);
                Bullet bullet = Instantiate(bulletPrefab, transform.position, rotation * scatter, transform).GetComponentInChildren<Bullet>();
                bullet.friendly = true;
                bullet.charge = Mathf.Clamp01((currentActionFrames - attackWindup) / (float)maxChargeFrames);
                UseEnergy(energyPerShot);
                break;

            case PlayerState.Rolling:
                gravity.dragModifier = rollGravityModifier;
                if (action.vector.magnitude > 0)
                    rb2d.AddForce(action.vector);
                UseEnergy(energyPerRoll);
                break;

            case PlayerState.Shadow:
                shadow = !shadow;
                break;
        }
        currentActionFrames = 0;
    }

    float GetScatter()
    {
        return UnityEngine.Random.Range(-scatterAngle / 2, scatterAngle / 2) * (aiming ? aimingScatterModifier : 1f);
    }

    public void Interrupt(int frames)
    {
        currentAction = new PlayerAction() { type = PlayerState.Idle, frames = frames };
        TriggerAction(currentAction);
    }

    bool CanQueueActions()
    {
        return (remStateFrames <= 15 && actions.Count <= 1);
    }

    /// <summary>
    /// Backswing can be cancelled in a certain frame window if there are no additional actions queued.
    /// </summary>
    bool CanCancelBackswing()
    {
        return false;
        //return (1 < remStateFrames && remStateFrames < 10 && currentAction.type != PlayerState.Idle && actions.Count == 0);
    }

    void TryCancelBackswing()
    {
        if (CanCancelBackswing())
            remStateFrames = 0;
    }

    // Consumes a certain amount of energy. Can trigger overdraw inactivity period.
    void UseEnergy(float amt)
    {
        framesSinceEnergyUsed = 0;
        currentEnergy = Mathf.Max(0, currentEnergy - amt);
        if (currentEnergy == 0)
            TriggerOverdraw();
    }

    void TriggerOverdraw()
    {
        Interrupt(Mathf.CeilToInt(maxEnergy / energyRechargeRate));
    }

    void Update()
    {
        if (animator)
            animator.SetBool("shadow", shadow);
    }

    void FixedUpdate()
    {
        currentEnergy = Mathf.Min(currentEnergy + energyRechargeRate * framesSinceEnergyUsed / 60f, maxEnergy);
        framesSinceEnergyUsed++;
        energyIndicator.percentage = currentEnergy / maxEnergy;
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

        //Handle movement
        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetMotion = Vector2.right * xMotion + Vector2.up * yMotion;
        if (currentAction.type == PlayerState.AttackWindup || currentAction.type == PlayerState.Attacking || currentAction.type == PlayerState.Idle)
        {
            float modifiedAcceleration = acceleration;
            modifiedAcceleration *= (floorData ? floorData.accelerationModifier : 1f);
            modifiedAcceleration *= (aiming ? aimingMovementModifier : 1f);
            modifiedAcceleration *= (currentAction.type == PlayerState.AttackWindup ? attackMovementModifier : 1f);
            rb2d.AddForce((targetMotion.magnitude > 1 ? targetMotion.normalized : targetMotion) * modifiedAcceleration);
        }

        if (spriteRenderer)
            spriteRenderer.color = Color.white;
        if (remStateFrames > 0)
        {
            remStateFrames--;
            switch (currentAction.type)
            {
                case PlayerState.Rolling:
                    if (remStateFrames == 0)
                        gravity.dragModifier = 1;
                    else if (remStateFrames < rollFrames / 5)
                        gravity.dragModifier = 5;
                    break;
            }
            if (CanCancelBackswing() && spriteRenderer)
                // Visual effect for combo opportunity
                spriteRenderer.color = new Color(.95f, .9f, 1f);
        }
        else if (!currentAction.sticky)
        {
            remStateFrames = 0;
            if (actions.Count > 0)
            {
                currentAction = actions[0];
                actions.RemoveAt(0);
                TriggerAction(currentAction);
            }
            else
            {
                currentAction = new PlayerAction();
            }
            currentActionFrames = 0;
        }
        else
        {
            if (currentAction.type == PlayerState.AttackWindup)
            {
                if (currentEnergy <= energyPerCharge || currentActionFrames - attackWindup >= maxChargeFrames)
                    currentAction.sticky = false;
                else
                    UseEnergy(energyPerCharge);
            }
        }

        if (Input.GetButtonUp("Attack"))
        {
            PlayerAction firstAttackWindup;
            if (currentAction.sticky && currentAction.type == PlayerState.AttackWindup)
                firstAttackWindup = currentAction;
            else
                firstAttackWindup = actions.Find((action) => { return action.sticky && action.type == PlayerState.AttackWindup; });
            if (firstAttackWindup != null)
                firstAttackWindup.sticky = false;
        }

        if (!CanQueueActions())
            return;

        if (Input.GetButtonDown("Roll") && targetMotion.magnitude > 0)
        {
            TryCancelBackswing();
            actions.Add(new PlayerAction() { type = PlayerState.Rolling, frames = rollFrames, vector = targetMotion.normalized * rollForce });
        }

        if (Input.GetButtonDown("Attack"))
        {
            TryCancelBackswing();
            Vector2 targetDirection = (LevelEditor.main.GetXYPlanePosition(Input.mousePosition) - (Vector2)transform.position).normalized;
            actions.Add(new PlayerAction() { type = PlayerState.AttackWindup, frames = attackWindup, vector = -targetDirection * attackForce, sticky = true });
            actions.Add(new PlayerAction() { type = PlayerState.Attacking, frames = attackFrames });
        }

        aiming = Input.GetButton("Aim");

        if (Input.GetButtonDown("Use item 1") && Items.Length >= 1)
        {
            Items[0].Activate(this);
        }

        if (Input.GetButtonDown("Use item 2") && Items.Length >= 2)
        {
            Items[1].Activate(this);
        }

        if (Input.GetButtonDown("Use item 3") && Items.Length >= 3)
        {
            Items[2].Activate(this);
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
        healthIndicator.Amount = (int)health.currentHealth;
        keyIndicator.Amount = keys;
        ItemSlot[] itemSlots = playerPanel.GetComponentsInChildren<ItemSlot>();
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < keybinds.Length)
                itemSlots[i].slot.text = keybinds[i];
            if (i == 0)
                continue;
            //    itemSlots[i].ItemSprite = swordPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
            else if (i == 1)
                continue;
            //itemSlots[i].ItemSprite = shield.GetComponent<SpriteRenderer>().sprite;
            else if (i == 2)
                itemSlots[i].ItemSprite = dodgeIcon;
            else if (i - 3 < Items.Length)
            {
                itemSlots[i].ItemSprite = Items[i - 3].Icon;
            }
        }
    }
}
