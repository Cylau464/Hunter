using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;

[DefaultExecutionOrder(-100)]

public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public float horizontal;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool crouchHeld;
    [HideInInspector] public bool crouchPressed;
    [HideInInspector] public bool lightAttack;
    [HideInInspector] public bool strongAttack;
    [HideInInspector] public bool jointAttack;
    [HideInInspector] public bool topDownAttack;
    [HideInInspector] public bool attackPressed;
    [HideInInspector] public bool evade;
    [HideInInspector] public bool hook;
    [HideInInspector] public bool horizontalAccess = true;
    [HideInInspector] public bool switchWeapon;
    [HideInInspector] public bool[] spell = new bool[3];
    [HideInInspector] public bool healingPotionHeld;
    [HideInInspector] public bool healingPotionPressed;
    [HideInInspector] public static bool restart;
    public List<InputsEnum> lastInputs = new List<InputsEnum>(1);           //Create new list for 2 elements for writting 2 last inputs
    public bool[] spellKeyUp = new bool[3];
    [SerializeField] float clearInputsDelay = .3f;
    float curClearDelay;

    bool readyToClear;

    PlayerAttack attack;
    PlayerMovement movement;
    PlayerAttributes attributes;

    Coroutine lastInputsCoroutine;

    private void Start()
    {
        attack = GetComponent<PlayerAttack>();
        movement = GetComponent<PlayerMovement>();
        attributes = GetComponent<PlayerAttributes>();
    }

    private void FixedUpdate()
    {
        readyToClear = true;
    }

    void Update()
    {
        if (restart)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            restart = false;
        }

        ClearInput();

        //if (GameManager.IsGameOver())
        //    return;

        ProcessInputs();

        if ((Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow)))
            horizontal = 0f;
        else
            horizontal = Mathf.Clamp(horizontal, -1f, 1f);

        if (lastInputs[0] == InputsEnum.LightAttack || lastInputs[0] == InputsEnum.StrongAttack || lastInputs[0] == InputsEnum.JointAttack || lastInputs[0] == InputsEnum.TopDownAttack)
            attackPressed = true;
        else
            attackPressed = false;
    }

    void ClearInput()
    {
        if (!readyToClear)
            return;

        horizontal = 0f;
        jumpHeld = false;
        jumpPressed = false;
        crouchHeld = false;
        crouchPressed = false;
        evade = false;
        restart = false;
        switchWeapon = false;
        healingPotionHeld = false;
        healingPotionPressed = false;
        hook = false;
        spellKeyUp = new bool[3];

        readyToClear = false;

        if (curClearDelay <= Time.time || !attack.canAttack || (attributes.Stamina <= 0 && lastInputs.Count > 0 && lastInputs[0] != InputsEnum.FirstSpell && lastInputs[0] != InputsEnum.SecondSpell && lastInputs[0] != InputsEnum.ThirdSpell))
        {
            lastInputs.Clear();
        }
    }

    void ProcessInputs()
    {
        horizontal = Input.GetAxis("Horizontal");

        jumpPressed = jumpPressed || Input.GetButtonDown("Jump");
        jumpHeld = jumpHeld || Input.GetButton("Jump");

        crouchPressed = crouchPressed || Input.GetButtonDown("Crouch");
        crouchHeld = crouchHeld || Input.GetButton("Crouch");

        switchWeapon = switchWeapon || Input.GetButtonDown("Switch Weapon");
        healingPotionHeld = healingPotionHeld || Input.GetButton("Healing Potion");
        healingPotionPressed = healingPotionPressed || Input.GetButtonDown("Healing Potion");

        hook = hook || Input.GetButtonDown("Hook");

        spellKeyUp[0] = spellKeyUp[0] || Input.GetButtonUp("First Spell");
        spellKeyUp[1] = spellKeyUp[1] || Input.GetButtonUp("Second Spell");
        spellKeyUp[2] = spellKeyUp[2] || Input.GetButtonUp("Third Spell");

        if (Input.GetButtonDown("Evade"))
        {
            if (attributes.Stamina <= 0) return;

            //If list have a free slot for new input
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnum.Evade);
            //If haven't - remove the first element and add a new input at the end
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnum.Evade);
            }

            curClearDelay = Time.time + clearInputsDelay;
        }

        if(Input.GetButtonDown("First Spell"))
        {
            //If list have a free slot for new input
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnum.FirstSpell);
            //If haven't - remove the first element and add a new input at the end
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnum.FirstSpell);
            }

            curClearDelay = Time.time + clearInputsDelay;
        }

        if (Input.GetButtonDown("Second Spell"))
        {
            //If list have a free slot for new input
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnum.SecondSpell);
            //If haven't - remove the first element and add a new input at the end
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnum.SecondSpell);
            }

            curClearDelay = Time.time + clearInputsDelay;
        }

        if (Input.GetButtonDown("Third Spell"))
        {
            //If list have a free slot for new input
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnum.ThirdSpell);
            //If haven't - remove the first element and add a new input at the end
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnum.ThirdSpell);
            }

            curClearDelay = Time.time + clearInputsDelay;
        }

        if (Input.GetButtonDown("LightAttack") && !GameManager.UIOverlapsMouse)
        {
            if (attributes.Stamina <= 0) return;

            //Top-down attack
            if (crouchHeld && !movement.isOnGround)
            {
                strongAttack = false;
                lightAttack = false;
                jointAttack = false;
                topDownAttack = true;
                lastInputs.Clear();

                if (lastInputsCoroutine != null)
                    StopCoroutine(lastInputsCoroutine);

                lastInputs.Add(InputsEnum.TopDownAttack);
            }
            else
            {
                if (strongAttack)
                {
                    strongAttack = false;
                    jointAttack = true;
                    StopCoroutine(lastInputsCoroutine);
                    lastInputsCoroutine = StartCoroutine(SetLastInputs(InputsEnum.JointAttack, .07f));
                }
                else
                {
                    lightAttack = true;
                    //StopCoroutine(lastInputsCoroutine);
                    lastInputsCoroutine = StartCoroutine(SetLastInputs(InputsEnum.LightAttack, .07f));
                }
            }

            curClearDelay = Time.time + clearInputsDelay;
        }

        if (Input.GetButtonDown("StrongAttack") && !GameManager.UIOverlapsMouse)
        {
            if (attributes.Stamina <= 0) return;

            //Top-down attack
            if (crouchHeld && !movement.isOnGround)
            {
                strongAttack = false;
                lightAttack = false;
                jointAttack = false;
                topDownAttack = true;
                lastInputs.Clear();

                if (lastInputsCoroutine != null)
                    StopCoroutine(lastInputsCoroutine);

                lastInputs.Add(InputsEnum.TopDownAttack);
            }
            else
            {
                if (lightAttack)
                {
                    lightAttack = false;
                    jointAttack = true;
                    StopCoroutine(lastInputsCoroutine);
                    lastInputsCoroutine = StartCoroutine(SetLastInputs(InputsEnum.JointAttack, .07f));
                }
                else
                {
                    strongAttack = true;
                    //StopCoroutine(lastInputsCoroutine);
                    lastInputsCoroutine = StartCoroutine(SetLastInputs(InputsEnum.StrongAttack, .07f));
                }
            }

            curClearDelay = Time.time + clearInputsDelay;
        }

        restart = restart || Input.GetKeyDown(KeyCode.R);
    }

    IEnumerator SetLastInputs(InputsEnum key, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (lastInputs.Count < lastInputs.Capacity)
            lastInputs.Add(key);
        else
        {
            lastInputs.RemoveAt(0);
            lastInputs.Add(key);
        }
    }
}