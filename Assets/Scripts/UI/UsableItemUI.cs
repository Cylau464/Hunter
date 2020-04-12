using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsableItemUI : MonoBehaviour
{
    [SerializeField] Text countText = null;
    string count = "0";

    public void SetItemCount(int count)
    {
        this.count = count.ToString();
        countText.text = this.count;
    }
}
