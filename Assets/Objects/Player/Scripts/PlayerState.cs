using UnityEngine;

public abstract class PlayerState : MonoBehaviour
{
	public int animationID;

	public virtual void Enter(Player player) { }

	public virtual void Step(Player player, float deltaTime) { }

	public virtual void Exit(Player player) { }
}