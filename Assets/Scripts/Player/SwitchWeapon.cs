using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchWeapon : MonoBehaviour
{
    Transform mainWeapon;
    Transform secondWeapon;
    string mainTag = "Main Weapon";
    string secondTag = "Second Weapon";

    PlayerInput input;
    PlayerAttack playerAttack;
    PlayerMovement playerMovement;
    StatusBar statusBar;

    float cooldown = .5f;
    float curCooldown;

    void Awake()
    {
        mainWeapon      = GameObject.FindGameObjectWithTag(mainTag).transform;
        secondWeapon    = GameObject.FindGameObjectWithTag(secondTag).transform;
        playerAttack    = GetComponent<PlayerAttack>();
        input           = GetComponent<PlayerInput>();
        playerMovement  = GetComponent<PlayerMovement>();

        GameObject[] _go = GameObject.FindGameObjectsWithTag("UI");

        foreach (GameObject go in _go)
        {
            if (go.TryGetComponent(out StatusBar _sb) && statusBar == null)
                statusBar = _sb;
        }
    }

    void Update()
    {
        if (curCooldown <= Time.time && input.switchWeapon && !playerMovement.isHurt && !playerMovement.isAttacking && !playerMovement.isDead)
            Switch();
    }

    void Switch()
    {
        Transform _main = mainWeapon;
        Transform _second = secondWeapon;
        _second.tag = mainTag;
        mainWeapon = _second;
        mainWeapon.gameObject.SetActive(true);
        _main.tag = secondTag;
        secondWeapon = _main;
        secondWeapon.gameObject.SetActive(false);

        playerAttack.weapon = mainWeapon;
        playerAttack.GetWeapon();
        statusBar.WeaponIconChange();

        input.switchWeapon = false;
        curCooldown = cooldown + Time.time;
    }
}