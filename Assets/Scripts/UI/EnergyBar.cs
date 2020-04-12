using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    [SerializeField] RectTransform fillTransform = null;
    [SerializeField] RectTransform[] pointsTransform = null;
    Image fillImage;
    Image[] pointsImage = new Image[2];
    Animator fillAnim;
    Sprite sprite;

    float fillCount;
    int maxEnergy;
    int pointsCount;

    // Start is called before the first frame update
    void Start()
    {
        fillImage = fillTransform.GetComponent<Image>();
        fillAnim = fillTransform.GetComponent<Animator>();
        sprite = fillImage.sprite;

        for (int i = 0; i < pointsTransform.Length; i++)
        {
            pointsImage[i] = pointsTransform[i].GetComponent<Image>();
        }
    }

    public void GetParameters(int energy, int maxEnergy, int energyPoints)
    {
        fillCount = energy / 100f;
        fillImage.fillAmount = fillCount;
        pointsCount = energyPoints;

        pointsImage[0].enabled = false;
        pointsImage[1].enabled = false;

        fillImage.sprite = sprite;
        fillAnim.enabled = false;

        for (int i = 0; i < pointsCount; i++)
        {
            if (i == 2)
            {
                fillAnim.enabled = true;
                
            }
            else
                pointsImage[i].enabled = true;
        }
    }
}
