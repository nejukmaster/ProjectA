using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public static HPBar instance;

    [SerializeField] Image hpbar;

    private void Start()
    {
        instance = this;
    }

    public void SetHp(float hp)
    {
        hpbar.fillAmount = hp;
    }
}
