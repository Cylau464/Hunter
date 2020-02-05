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

    void Awake()
    {
        mainWeapon      = GameObject.FindGameObjectWithTag(mainTag).transform;
        secondWeapon    = GameObject.FindGameObjectWithTag(secondTag).transform;
        statusBar       = GameObject.FindGameObjectWithTag("UI").GetComponent<StatusBar>();
        playerAttack    = GetComponent<PlayerAttack>();
        input           = GetComponent<PlayerInput>();
        playerMovement  = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (input.switchWeapon && !playerMovement.isHurt && !playerMovement.isAttacking && !playerMovement.isDead)
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
    }
}