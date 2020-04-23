using UnityEngine;

public struct FreedomCollision
{
	public Vector3 point;
	public Vector3 normal;

	public float distance;

	public Collider collider;

	public FreedomCollision(Vector3 point, Vector3 normal, float distance, Collider collider)
	{
		this.point = point;
		this.normal = normal;
		this.distance = distance;
		this.collider = collider;
	}
}