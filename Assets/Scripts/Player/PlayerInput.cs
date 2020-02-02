using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InputsEnum { Evade, StrongAttack, LightAttack, JointAttack }

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
    [HideInInspector] public bool evade;
    [HideInInspector] public bool hook;
    [HideInInspector] public bool horizontalAccess = true;
    [HideInInspector] public static bool restart;
    public List<InputsEnum> lastInputs = new List<InputsEnum>(2);           //Create new list for 2 elements for writting 2 last inputs

    bool readyToClear;

    PlayerAttack attack;

    Coroutine lastInputsCoroutine;

    private void Start()
    {
        attack = GetComponent<PlayerAttack>();
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
        if (Input.GetButtonDown("StrongAttack"))
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