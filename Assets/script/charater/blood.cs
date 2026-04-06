using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class blood : MonoBehaviour
{
    public int now_blood;
    public int live = 1;
    public Slider UI_health;
    public TMP_Text hpText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetHp();
    }

    // Update is called once per frame
    void Update()
    {
        hpText.text = $"{now_blood}%";//顯示目前數值
    }

    public void ResetHp()
    {

        if(live == 1)
        {
            now_blood = 100;//賦予上限值
        }
        else
        {
            now_blood = 0;//歸零
        }

        UI_health.value = now_blood;//更新UI
    }

    public void damage(int much)
    {

        now_blood -= much;//減去受到傷害
        UI_health.value = now_blood;//更新UI

        if(now_blood <= 0)
        {
            live -= 1;//死亡重新並且復活
            ResetHp();//更新生命值狀態
        }

    }

    public void upper(int much)
    {
        now_blood += much;//增加生命值
        if (now_blood > 100) now_blood = 100;//防生命值過100
        UI_health.value = now_blood;//更新UI
    }
}
