using UnityEngine;

public class SpindashPlayerState : PlayerState
{
	private float power;

	public override void Enter(Player player)
	{
		power = 0;
		player.attacking = true;
		player.ChangeBounds(1);
		player.skin.ActiveBall(true);
		player.PlayAudio(player.audios.spinDashCharge, 0.5f);
		player.particles.spindashSmoke.Play();
	}

	public override void Step(Player player, float deltaTime)
	{
		player.HandleGravity(deltaTime);
		player.HandleFall();

		power -= ((power / player.stats.powerLoss) / 256f) * deltaTime;

		if (player.grounded)
		{
			if (player.input.down)
			{
				if (player.input.actionDown)
				{
					power += player.stats.chargePower;
					power = Mathf.Min(power, player.stats.maxChargePower);
					player.PlayAudio(player.audios.spinDashCharge, 0.5f);
				}
			}
			else
			{
				player.state.ChangeState<RollPlayerState>();
			}
		}
	}

	public override void Exit(Player player)
	{
		player.skin.ActiveBall(false);
		player.velocity.x = (player.stats.minReleasePower + (Mathf.Floor(power) / 2)) * player.direction;
		player.PlayAudio(player.audios.spinDashRelease, 0.5f);
		player.particles.spindashSmoke.Stop();
	}
}
