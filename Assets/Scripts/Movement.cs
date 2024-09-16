using UnityEngine;

//This script is a clean powerful solution to a top-down movement player
public class Movement : MonoBehaviour
{
	//Public variables that wer can edit in the editor
	public float maxSpeed = 5; //Our max speed
	public float acceleration = 20; //How fast we accelerate
	public float deceleration = 4; //brake power

	//Private variables for internal logic
	Vector3 velocity; //Our current velocity

	void Update()
	{
		//Create/clear a vector for  our input.
		Vector3 rawInput = new();

		//Get the raw input
		rawInput.x = Input.GetAxisRaw("Horizontal");
		rawInput.y = Input.GetAxisRaw("Vertical");

		//If we have a square magnitude over one, normalize the length of the vector to 1
		//if it's shorter then one we don't need this step.
		if (rawInput.sqrMagnitude > 1)
		{
			rawInput.Normalize();
		}

		//add our input to our velocity
		//This provides acceleration +10m/s/s
		velocity += rawInput * acceleration * Time.deltaTime;

		//Check our max speed, if our magnitude is faster them max speed
		if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
		{
			//Normalize our velocity and change the magnitude to our max speed.
			velocity.Normalize();
			velocity *= maxSpeed;
		}

		//If we have zero input from the player
		if (rawInput.sqrMagnitude == 0)
		{
			//Reduce our speed based on how fast we are going
			//A value of 0.9 would remove 10% or our speed
			velocity *= 1 - deceleration * Time.deltaTime;
		}

		//Move our transform based to our updated position.
		transform.position += velocity * Time.deltaTime;
	}
}