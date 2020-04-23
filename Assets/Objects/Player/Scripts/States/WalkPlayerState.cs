using UnityEngine;

public class WalkPlayerState : PlayerState
{
	public override void Enter(Player player)
	{
		player.attacking = false;
		player.ChangeBounds(0);
	}

	public override void Step(Player player, float deltaTime)
	{
		player.UpdateDirection(player.input.horizontal);
		player.HandleSlopeFactor(deltaTime);
		player.HandleAcceleration(deltaTime);
		player.HandleFriction(deltaTime);
		player.HandleGravity(deltaTime);
		player.HandleFall();

		if (player.grounded)
		{
			if (player.input.actionDown)
			{
				player.HandleJump();
			}
			else if (player.input.down)
			{
				if (Mathf.Abs(player.velocity.x) > player.stats.minSpeedToRoll)
				{
					player.state.ChangeState<RollPlayerState>();
					player.PlayAudio(player.audios.spin, 0.5f);
				}
				else if (player.angle < player.stats.minAngleToSlide)
				{
					player.state.ChangeState<CrouchPlayerState>();
				}
			}
			else if (player.input.up)
			{
				if (Mathf.Abs(player.velocity.x) < player.stats.minSpeedToRoll)
				{
					player.state.ChangeState<LookUpPlayerState>();
				}
			}
			else if (Mathf.Sign(player.velocity.x) != Mathf.Sign(player.input.horizontal) && player.input.horizontal != 0)
			{
				if (Mathf.Abs(player.velocity.x) >= player.stats.minSpeedToBrake)
				{
					player.state.ChangeState<BrakePlayerState>();
				}
				else
				{
					if (player.velocity.x > 0)
					{
						player.velocity.x = -player.stats.turnSpeed;
					}
					else
					{
						player.velocity.x = player.stats.turnSpeed;
					}
				}
			}
		}
	}
}