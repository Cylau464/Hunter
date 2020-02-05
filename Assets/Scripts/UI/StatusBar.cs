using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    [SerializeField] Text healthText = null;
    [SerializeField] Image healthBar = null;
    [HideInInspector] public int maxHealth;
    [SerializeField] Text staminaText = null;
    [SerializeField] Image staminaBar = null;
    [HideInInspector] public int maxStamina;
    [SerializeField] Image mainWeaponIcon = null;
    [SerializeField] Image secondWeaponIcon = null;

    public void HealthChange(int health)
    {
        if (health < 0)
            health = 0;

        healthText.text = health.ToString();
        healthBar.fillAmount = (float) health / maxHealth;
    }

    public void StaminaChange(int stamina)
    {
        if (stamina < 0)
            stamina = 0;

        staminaText.text = stamina.ToString();
        staminaBar.fillAmount = (float) stamina / maxStamina;
    }

    public void WeaponIconSet(Sprite icon, string tag)
    {
        if (tag == "Main Weapon")
            mainWeaponIcon.sprite = icon;
        else if (tag == "Second Weapon")
            secondWeaponIcon.sprite = icon;
        else
            Debug.LogError("Incorrect tag");
    }

    public void WeaponIconChange()
    {
        Image _mainIcon = mainWeaponIcon;
        Image _secondIcon = secondWeaponIcon;
        _mainIcon.sprite = secondWeaponIcon.sprite;
        _secondIcon.sprite = mainWeaponIcon.sprite;
    }
}
