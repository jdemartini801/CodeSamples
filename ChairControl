/* This is the update function for controlling the chair. The function UpdateValues() uses shared memory to communicate the changed values to the motion platform.

This uses custom kinematics to slowly accelerate the motion platform to its desired velocity to smoothly change the pitch, roll, and yaw of the platform.

*/

void Update()
{

    // If program has read the last used telemetry values from shared memory
    if (readCurrentValues)
    {

        // Provided for smoother data
        packetTimeValue += Time.deltaTime * 1000;

        // Spinning on Z axis (yaw)
        if (spinning)
        {

            // Increase velocity by acceleration
            yawVel += yawAccel * Time.deltaTime;

            if(maxYawVel != 0)
            yawVel = Mathf.Clamp(yawVel, -maxYawVel, maxYawVel);

            // Increase yaw value over time
            yawValue += yawVel * Time.deltaTime;

            // Yaw is bounded by -pi and pi, so it has to skip to the other bound.
            if (yawValue > 3.14f)
            {
                yawValue = -3.14f;
            }
            else
            if (yawValue < -3.14f)
            {
                yawValue = 3.14f;
            }

        } else
        {
            // If it is not spinning, then slow down to a stop
            yawVel = Mathf.MoveTowards(yawVel, 0, yawAccel * Time.deltaTime);
            yawValue += yawVel * Time.deltaTime;

            if (yawValue > 3.14f)
            {
                yawValue = -3.14f;
            }
            else
            if (yawValue < -3.14f)
            {
                yawValue = 3.14f;
            }

        }

        // Move rollValue towards the desiredRollValue
        rollValue = Mathf.MoveTowards(rollValue, desiredRollValue, rollVel * Time.deltaTime);
        Debug.Log(rollValue + " : " + desiredRollValue);

        // Move pitchValue towards the desiredRollValue
        pitchValue = Mathf.MoveTowards(pitchValue, desiredPitchValue, pitchVel * Time.deltaTime);

        // Update telemetry values for the chair
        UpdateValues();

    }

}
