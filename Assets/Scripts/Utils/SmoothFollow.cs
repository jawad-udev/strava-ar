using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{
	public float interpVelocity;
	public GameObject target;
	Vector3 targetPos;
	public Vector3 lowerBoundry = Vector3.zero;

	// Use this for initialization
	void Start()
	{
		targetPos = transform.position;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (target)
		{
			Vector3 posNoZ = transform.position;
			posNoZ.z = target.transform.position.z;

			Vector3 targetDirection = (target.transform.position - posNoZ);

			interpVelocity = targetDirection.magnitude * 5f;

			targetPos = transform.position + (targetDirection.normalized * interpVelocity * Time.deltaTime);

			Vector3 finalPos = Vector3.Lerp(transform.position, targetPos, 0.25f);

			if(finalPos.y > lowerBoundry.y)
				transform.position = new Vector3(transform.position.x , finalPos.y, finalPos.z);
		}
	}
}