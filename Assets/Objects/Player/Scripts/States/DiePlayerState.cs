using UnityEngine;

public class DiePlayerState : PlayerState
{
	public override void Enter(Player player)
	{
		player.GroundExit();
		player.EnableCollision(false);
		player.skin.StopBlinking();
		player.attacking = false;
		player.disableSkinRotation = true;
		player.disableCameraFollow = true;
		player.velocity = Vector3.zero;
		player.velocity.y = player.stats.diePushUp;
		player.PlayAudio(player.audios.dead);
	}

	public override void Step(Player player, float deltaTime)
	{
		player.HandleGravity(deltaTime);
	}
}
