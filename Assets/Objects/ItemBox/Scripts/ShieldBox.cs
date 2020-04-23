using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Item Box/Shield Box")]
public class ShieldBox : ItemBox
{
    public PlayerShields shield;

    protected override void OnCollect(Player player)
    {
        player.SetShield(shield);
    }
}
