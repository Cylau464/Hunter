using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Text text = null;
    [SerializeField] Image bar = null;
    public int maxHealth;

    public void HealthChange(int health)
    {
        if (health < 0)
            health = 0;

        text.text = health + " / " + maxHealth;
        bar.fillAmount = (float) health / maxHealth;
    }
}
