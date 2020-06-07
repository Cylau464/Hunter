using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargingBar : MonoBehaviour
{
    [SerializeField] Image fillImage = null;
    [SerializeField] Text castTimeText = null;
    float curTime;
    float castTime;

    void Update()
    {
        if (curTime < castTime)
        {
            curTime = Mathf.Clamp(curTime + Time.deltaTime, 0f, castTime);
            fillImage.fillAmount = curTime / castTime;
            castTimeText.text = System.Math.Round(curTime, 2).ToString("0.00") + " / " + System.Math.Round(castTime, 2).ToString("0.00");
        }
        else
            gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        curTime = 0f;
    }

    public void SetChargingTime(float castTime)
    {
        gameObject.SetActive(true);
        this.castTime = castTime;
    }
}
