using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Item Box/RingBox")]
public class RingBox : ItemBox
{
	public int ringAmount;

	protected override void OnCollect(Player player)
	{
		ScoreManager.Instance.Rings += ringAmount;
	}
}
