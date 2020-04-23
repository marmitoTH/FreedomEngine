public class JumpPlayerState : PlayerState
{
	public override void Enter(Player player)
	{
		player.attacking = true;
		player.ChangeBounds(1);
	}

	public override void Step(Player player, float deltaTime)
	{
		player.UpdateDirection(player.input.horizontal);
		player.HandleAcceleration(deltaTime);
		player.HandleGravity(deltaTime);

		if (!player.grounded && player.attacking)
		{
			if (player.input.actionUp && player.velocity.y > player.stats.minJumpHeight)
			{
				player.velocity.y = player.stats.minJumpHeight;
			}
		}
		else
		{
			player.state.ChangeState<WalkPlayerState>();
		}
	}
}