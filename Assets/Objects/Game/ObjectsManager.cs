using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Freedom Engine/Game/Objects Manager")]
public class ObjectsManager : MonoBehaviour
{
	private static ObjectsManager instance;

	public static ObjectsManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<ObjectsManager>();
				instance.StartSingleton();
			}

			return instance;
		}
	}

	private readonly List<FreedomObject> objects = new List<FreedomObject>();

	private void StartSingleton()
	{
		GetFreedomObjects();
	}

	private void GetFreedomObjects()
	{
		foreach (FreedomObject freedomObject in FindObjectsOfType<FreedomObject>())
		{
			objects.Add(freedomObject);
		}
	}

	public void RespawnFreedomObjects()
	{
		foreach (FreedomObject freedomObject in objects)
		{
			freedomObject.OnRespawn();
		}
	}
}
