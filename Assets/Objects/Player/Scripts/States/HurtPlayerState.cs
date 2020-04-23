public class HurtPlayerState : PlayerState
{
	public override void Enter(Player player)
	{
		player.GroundExit();
		player.ChangeBounds(0);
		player.invincible = true;
		player.halfGravity = true;
		player.attacking = false;
	}

	public override void Step(Player player, float deltaTime)
	{
		player.HandleGravity(deltaTime);

		if (player.grounded)
		{
			player.velocity.x = 0;
			player.state.ChangeState<WalkPlayerState>();
		}
	}

	public override void Exit(Player player)
	{
		player.halfGravity = false;
		player.skin.StartBlinking(player.stats.invincibleTime);
		player.invincibleTimer = player.stats.invincibleTime;
	}
}
