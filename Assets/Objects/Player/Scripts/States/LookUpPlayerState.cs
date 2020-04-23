public class LookUpPlayerState : PlayerState
{
	public override void Enter(Player player)
	{
		player.lookingUp = true;
		player.attacking = false;
		player.velocity.x = 0;
		player.ChangeBounds(1);
	}

	public override void Step(Player player, float deltaTime)
	{
		player.HandleGravity(deltaTime);
		player.HandleFall();

		if (player.grounded)
		{
			if (player.input.actionDown)
			{
				player.HandleJump();
			}
			else if (!player.input.up)
			{
				player.state.ChangeState<WalkPlayerState>();
			}
		}
		else
		{
			player.state.ChangeState<WalkPlayerState>();
		}
	}

	public override void Exit(Player player)
	{
		player.lookingUp = false;
	}
}
