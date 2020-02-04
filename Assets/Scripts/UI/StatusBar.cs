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
}
