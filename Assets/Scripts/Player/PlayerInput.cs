using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputsEnum { Evade, StrongAttack, LightAttack }

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
    [HideInInspector] public bool evade;
    [HideInInspector] public bool hook;
    [HideInInspector] public bool horizontalAccess = true;
    [HideInInspector] public static bool restart;
    public List<InputsEnum> lastInputs = new List<InputsEnum>(2);           //Create new list for 2 elements for writting 2 last inputs

    bool readyToClear;

    PlayerAttack attack;

    private void Start()
    {
        attack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
        ClearInput();

        //if (GameManager.IsGameOver())
        //    return;

        ProcessInputs();

        if ((Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow)))
            horizontal = 0f;
        else
            horizontal = Mathf.Clamp(horizontal, -1f, 1f);

        readyToClear = true;
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
        hook = false;

        readyToClear = false;

        if (!attack.canAttack)
            lastInputs.Clear();
    }

    void ProcessInputs()
    {
        horizontal = Input.GetAxis("Horizontal");

        jumpPressed = jumpPressed || Input.GetButtonDown("Jump");
        jumpHeld = jumpHeld || Input.GetButton("Jump");

        crouchPressed = crouchPressed || Input.GetButtonDown("Crouch");
        crouchHeld = crouchHeld || Input.GetButton("Crouch");

        hook = hook || Input.GetButtonDown("Hook");

        //evade = evade || Input.GetButtonDown("Evade");
        if(Input.GetButtonDown("Evade"))
        {
            //If list have a free slot for new input
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnum.Evade);
            //If haven't - remove the first element and add a new input at the end
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnum.Evade);
            }
        }

        if (Input.GetButtonDown("LightAttack"))
        {
            lightAttack = true;
            if(lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnum.LightAttack);
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnum.LightAttack);
            }
        }
        if (Input.GetButtonDown("StrongAttack"))
        {
            strongAttack = true;
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnum.StrongAttack);
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnum.StrongAttack);
            }
        }

        restart = restart || Input.GetKeyDown(KeyCode.R);
    }
}