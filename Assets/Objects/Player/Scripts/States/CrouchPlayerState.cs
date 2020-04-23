public class CrouchPlayerState : PlayerState
{
	public override void Enter(Player player)
	{
		player.lookingDown = true;
		player.attacking = false;
		player.velocity.x = 0;
		player.ChangeBounds(1);
	}

	public override void Step(Player player, float deltaTime)
	{
		player.HandleGravity(deltaTime);
		player.HandleFall();

		if (player.grounded && player.input.down)
		{
			if (player.input.actionDown)
			{
				player.state.ChangeState<SpindashPlayerState>();
			}
		}
		else
		{
			player.state.ChangeState<WalkPlayerState>();
		}
	}

	public override void Exit(Player player)
	{
		player.lookingDown = false;
	}
}
