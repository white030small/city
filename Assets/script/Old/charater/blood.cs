using UnityEngine;

public class blood : MonoBehaviour
{
    [Header("血量設定")]
    public int maxBlood = 4;
    public GameObject[] films;
    private int nowBlood;

    void Start()
    {
        nowBlood = maxBlood;
        UpdateUI();
    }

    public void damage(int much)
    {
        nowBlood -= much;
        if (nowBlood < 0) nowBlood = 0;
        DamageUI();

        if (nowBlood <= 0)
        {
            Die();
        }
    }

    public void upper(int much)
    {
        nowBlood += much;
        if (nowBlood > maxBlood) nowBlood = maxBlood;
        UpdateUI();
    }

    public void IncreaseMax(int amount)
    {
        maxBlood += amount;
        if (maxBlood > 10) maxBlood = 10;
        nowBlood += amount;
        if (nowBlood > maxBlood) nowBlood = maxBlood;
        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < films.Length; i++)
        {
            films[i].SetActive(i < nowBlood); // 整個物件開關
        }
    }

    void DamageUI()
    {
        for (int i = 0; i < films.Length; i++)
        {
            if (i >= nowBlood && films[i].activeSelf)
            {
                Animator anim = films[i].GetComponent<Animator>();
                anim.SetTrigger("Burn");
            }
        }
    }

    void Die()
    {
        Time.timeScale = 0;
    }
}