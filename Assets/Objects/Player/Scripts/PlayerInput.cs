using UnityEngine;

[System.Serializable]
public class PlayerInput
{
	[SerializeField] private string horizontalName = "Horizontal";
	[SerializeField] private string verticalName = "Vertical";
	[SerializeField] private string actionName = "Action";

	public float horizontal { get; private set; }
	public float vertical { get; private set; }

	public bool right { get; private set; }
	public bool left { get; private set; }
	public bool up { get; private set; }
	public bool down { get; private set; }

	public bool action { get; private set; }
	public bool actionDown { get; private set; }
	public bool actionUp { get; private set; }

	private bool controlLocked;
	private float unlockTimer;

	public void InputUpdate()
	{
		UpdateAxes();
		UpdateAction();
	}

	private void UpdateAxes()
	{
		horizontal = !controlLocked ? Input.GetAxis(horizontalName) : 0;
		vertical = Input.GetAxis(verticalName);
		actionDown = actionUp = false;
		right = horizontal > 0;
		left = horizontal < 0;
		up = vertical > 0;
		down = vertical < 0;
	}

	private void UpdateAction()
	{
		if (Input.GetButton(actionName))
		{
			if (!action)
			{
				action = true;
				actionDown = true;
			}
		}
		else
		{
			if (action)
			{
				action = false;
				actionUp = true;
			}
		}
	}

	public void LockHorizontalControl(float time)
	{
		unlockTimer = time;
		controlLocked = true;
	}

	public void UnlockHorizontalControl(float deltaTime)
	{
		if (unlockTimer > 0)
		{
			unlockTimer -= deltaTime;

			if (unlockTimer <= 0)
			{
				unlockTimer = 0;
				controlLocked = false;
			}
		}
	}
}