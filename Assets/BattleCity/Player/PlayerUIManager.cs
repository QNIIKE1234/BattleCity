using UnityEngine;
using TMPro;
using System;

public class PlayerUIManager : MonoBehaviour
{

    public TextMeshProUGUI BulletCount;
    public TextMeshProUGUI TankHp;
    public TextMeshProUGUI SpecialCount;
    public TextMeshProUGUI Result;

    public Action OnReload;
    public Action OnSpecial;

    public void UpdateUIHp(int hp, int maxHp)
    {
        if (TankHp == null) return;
        TankHp.text = $"HP: {hp} / {maxHp}";
    }

    public void UpdateBulletCount(int bullet, int maxBullet)
    {
        if (BulletCount == null) return;
        BulletCount.text = $"Bullet: {bullet} / {maxBullet}";
    }

    public void UpdateSpecialCount(int special, int maxSpecial)
    {
        if (SpecialCount == null) return;
        SpecialCount.text = $"P : {special} / {maxSpecial}";
    }

    public void ShowResult(bool won)
    {
        if (Result == null) return;
        Result.gameObject.SetActive(true);
        Result.text = won ? "YOU WIN" : "GGEZ YOU NOOB";
    }

    public void OnReloadBullet()
    {
        OnReload?.Invoke();
    }

    public void SpecialSkill()
    {
        OnSpecial?.Invoke();
    }
}
