using UnityEngine;

public class RollPlayerState : PlayerState
{
	public override void Enter(Player player)
	{
		player.attacking = true;
		player.ChangeBounds(1);
		player.particles.brakeSmoke.Play();
	}

	public override void Step(Player player, float deltaTime)
	{
		player.HandleSlopeFactor(deltaTime);
		player.HandleFriction(deltaTime);
		player.HandleDeceleration(deltaTime);
		player.HandleGravity(deltaTime);
		player.HandleFall();

		if (player.grounded)
		{
			if (player.input.actionDown)
			{
				player.HandleJump();
			}
			else if (Mathf.Abs(player.velocity.x) < player.stats.minSpeedToUnroll)
			{
				player.state.ChangeState<WalkPlayerState>();
			}
		}
		else
		{
			player.state.ChangeState<JumpPlayerState>();
		}
	}

	public override void Exit(Player player)
	{
		player.particles.brakeSmoke.Stop();
	}
}