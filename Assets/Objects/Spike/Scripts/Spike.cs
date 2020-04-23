using UnityEngine;

[AddComponentMenu("Freedom Engine/Objects/Spike")]
[RequireComponent(typeof(BoxCollider))]
public class Spike : FreedomObject
{
	private Player player;

	public override void OnPlayerMotorContact(PlayerMotor motor)
	{
		if (motor.TryGetComponent(out player))
		{
			var direction = (player.transform.position - transform.position).normalized;
			
			if (Vector3.Dot(transform.up, direction) > 0.7f)
			{
				player.ApplyHurt(transform.position);
			}
		}
	}
}
