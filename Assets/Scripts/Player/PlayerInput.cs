using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputsEnums { Evade, StrongAttack, LightAttack }

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
    [HideInInspector] public bool horizontalAccess = true;
    [HideInInspector] public static bool restart;
    public List<InputsEnums> lastInputs = new List<InputsEnums>(2);

    bool _readyToClear;

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

        _readyToClear = true;
    }

    void ClearInput()
    {
        if (!_readyToClear)
            return;

        horizontal = 0f;
        jumpHeld = false;
        jumpPressed = false;
        crouchHeld = false;
        crouchPressed = false;
        evade = false;
        restart = false;

        _readyToClear = false;

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

        //evade = evade || Input.GetButtonDown("Evade");
        if(Input.GetButtonDown("Evade"))
        {
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnums.Evade);
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnums.Evade);
            }
        }

        if (Input.GetButtonDown("LightAttack"))
        {
            lightAttack = true;
            if(lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnums.LightAttack);
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnums.LightAttack);
            }
        }
        if (Input.GetButtonDown("StrongAttack"))
        {
            strongAttack = true;
            if (lastInputs.Count < lastInputs.Capacity)
                lastInputs.Add(InputsEnums.StrongAttack);
            else
            {
                lastInputs.RemoveAt(0);
                lastInputs.Add(InputsEnums.StrongAttack);
            }
        }

        restart = restart || Input.GetKeyDown(KeyCode.R);
    }
}