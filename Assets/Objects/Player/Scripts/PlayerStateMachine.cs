using System;
using System.Collections.Generic;

public class PlayerStateMachine
{
	private readonly Player player;
	
	private readonly Dictionary<Type, PlayerState> states = new Dictionary<Type, PlayerState>();

	private Type currentState;

	public int stateId => states[currentState].animationID;

	public PlayerStateMachine(Player player)
	{
		this.player = player;
	}

	public void AddState(PlayerState state)
	{
		var type = state.GetType();

		if (!states.ContainsKey(type))
		{
			states.Add(type, state);
		}
	}

	public void ChangeState<T>() where T : PlayerState
	{
		var type = typeof(T);

		if (states.ContainsKey(type))
		{
			if (currentState != null)
			{
				states[currentState].Exit(player);
			}

			currentState = type;
			states[currentState].Enter(player);
		}
	}

	public void UpdateState(float deltaTime)
	{
		if (currentState != null)
		{
			states[currentState].Step(player, deltaTime);
		}
	}
}