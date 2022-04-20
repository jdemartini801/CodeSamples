/*
 This coroutine compares what the player drew with a reference shape (in this case the fire spell). It averages a group of vectors between the trail points and
 calculates the angle of the vector from the positive X axis. How close this angle is to the reference trails angle determines if the spell is a "match"
*/

public IEnumerator Cast(Transform reference)
{

    bool match = false;

    Vector3[] positions = new Vector3[trail.positionCount];
    trail.GetPositions(positions);

    // Check if it matches the fire spell shape
    Vector3[] compPositions = SpellReference.fire;

    GameObject parent = Instantiate(emptyPrefab);
    parent.transform.position = reference.transform.position;

    parent.transform.rotation = Quaternion.LookRotation(reference.forward);

    for(int i = 0; i < positions.Length; i++)
    {
        Vector3 pos = positions[i];
        GameObject child = Instantiate(emptyPrefab);
        child.transform.parent = parent.transform;
        child.transform.position = pos;
    }

    /* The point of creating empty transforms is to create a parent object that can use Quaternion.LookRotation to
     change the direction of the drawn shape to face world forward. This ensures that the spells that the player
     draws and the reference spells are facing the same direction so their vectors can be compared. */

    parent.transform.rotation = Quaternion.LookRotation(Vector3.forward);

    // Fill up the positions vector with the position objects
    int index = -1;
    foreach(Transform t in parent.GetComponentsInChildren<Transform>())
    {

        if(index == -1)
        {
            index++;
            continue;
        }

        positions[index] = t.position;
        index++;

    }

    Destroy(parent);

    int errors = 0;
    yield return new WaitForSeconds(.005f);

    // This for loop takes an average of positions in a trail and compares the angles between them.
    for (float i = 0; i < 99; i++)
    {

        float index1 = (i / 100f) * (float)positions.Length;
        float index2 = ((i + 1f) / 100f) * (float)positions.Length;

        int startIndex = (int) (index1 - (positions.Length * .05f));
        int endIndex = (int)(index2 + (positions.Length * .05f));

        if(startIndex < 0)
        {
            startIndex = 0;
        }

        if(endIndex > positions.Length - 1)
        {
            endIndex = positions.Length - 1;
        }

        float xAvg = 0, yAvg = 0, total = 0;

        // Create average
        for (int j = startIndex; j < endIndex; j++)
        {
            Vector3 pos1 = positions[j];
            Vector3 pos2 = positions[j + 1];
            Vector3 dir = pos2 - pos1;
            xAvg += dir.x;
            yAvg += dir.y;
            total++;
        }

        xAvg /= total;
        yAvg /= total;

        Vector3 dir1 = new Vector3(xAvg, yAvg, 0);

        // Comparison 
        float index3 = (i / 100f) * (float)compPositions.Length;
        float index4 = ((i + 1f) / 100f) * (float)compPositions.Length;

        int startIndex2 = (int)(index3 - (compPositions.Length * .05f));
        int endIndex2 = (int)(index4 + (compPositions.Length * .05f));

        if (startIndex2 < 0)
        {
            startIndex2 = 0;
        }

        if (endIndex2 > compPositions.Length - 1)
        {
            endIndex2 = compPositions.Length - 1;
        }

        float xAvg2 = 0, yAvg2 = 0, total2 = 0;

        // Create average
        for (int j = startIndex2; j < endIndex2; j++)
        {
            Vector3 pos1 = compPositions[j];
            Vector3 pos2 = compPositions[j + 1];
            Vector3 dir = pos2 - pos1;
            xAvg2 += dir.x;
            yAvg2 += dir.y;
            total2++;
        }

        xAvg2 /= total2;
        yAvg2 /= total2;

        Vector3 dir2 = new Vector3(xAvg2, yAvg2, 0);

        // First vector
        float angleFromPosX1 = 0;
        if (dir1.x > 0 && dir1.y > 0)
        {
            angleFromPosX1 = Mathf.Atan(dir1.y / dir1.x) * (180 / Mathf.PI);
        }
        else
        if (dir1.x < 0 && dir1.y > 0)
        {
            angleFromPosX1 = Mathf.Atan(dir1.x / dir1.y) * (180 / Mathf.PI);
            angleFromPosX1 += 90;
        }
        else
        if (dir1.x < 0 && dir1.y < 0)
        {
            angleFromPosX1 = Mathf.Atan(dir1.y / dir1.x) * (180 / Mathf.PI);
            angleFromPosX1 += 180;
        }
        else
        if (dir1.x > 0 && dir1.y < 0)
        {
            angleFromPosX1 = Mathf.Atan(dir1.x / dir1.y) * (180 / Mathf.PI);
            angleFromPosX1 += 270;
        }
        else
        if (dir1.x == 0 && dir1.y > 0)
        {
            angleFromPosX1 = 90;
        }
        else
        if (dir1.x == 0 && dir1.y < 0)
        {
            angleFromPosX1 = 270;
        }
        else
        if (dir1.x > 0 && dir1.y == 0)
        {
            angleFromPosX1 = 0;
        }
        else
        if (dir1.x < 0 && dir1.y == 0)
        {
            angleFromPosX1 = 180;
        }

        // Comparison vector
        float angleFromPosX2 = 0;
        if (dir2.x > 0 && dir2.y > 0)
        {
            angleFromPosX2 = Mathf.Atan(dir2.y / dir2.x) * (180 / Mathf.PI);
        }
        else
        if (dir2.x < 0 && dir2.y > 0)
        {
            angleFromPosX2 = Mathf.Atan(dir2.x / dir2.y) * (180 / Mathf.PI);
            angleFromPosX2 += 90;
        }
        else
        if (dir2.x < 0 && dir2.y < 0)
        {
            angleFromPosX2 = Mathf.Atan(dir2.y / dir2.x) * (180 / Mathf.PI);
            angleFromPosX2 += 180;
        }
        else
        if (dir2.x > 0 && dir2.y < 0)
        {
            angleFromPosX2 = Mathf.Atan(dir2.x / dir2.y) * (180 / Mathf.PI);
            angleFromPosX2 += 270;
        }
        else
        if (dir2.x == 0 && dir2.y > 0)
        {
            angleFromPosX2 = 90;
        }
        else
        if (dir2.x == 0 && dir2.y < 0)
        {
            angleFromPosX2 = 270;
        }
        else
        if (dir2.x > 0 && dir2.y == 0)
        {
            angleFromPosX2 = 0;
        }
        else
        if (dir2.x < 0 && dir2.y == 0)
        {
            angleFromPosX2 = 180;
        }

        if (Mathf.Abs(angleFromPosX2 - angleFromPosX1) > threshold)
        {
            if ((dir1.x != 0 || dir1.y != 0 || dir1.z != 0) && (dir2.x != 0 || dir2.y != 0 || dir2.z != 0))
            {
                errors++;
            }
        }

    }

    if (errors < allowedErrors)
    {
        match = true;
    }

    if (match == true)
    {
        StartCoroutine(FadeTrail(Color.green));
        Debug.Log("MATCH:" + errors);

    }
    else
    {
        StartCoroutine(FadeTrail(Color.red));
        Debug.Log("FAIL:" + errors);
    }

}

public IEnumerator FadeTrail(Color finalColor)
{

    Color start = trail.startColor;
    Color end = trail.endColor;

    for (float i = 0; i < 100f; i++)
    {
        yield return new WaitForSeconds(0.01f);

        trail.startColor = Color.Lerp(start, finalColor, i / 100f);
        trail.endColor = Color.Lerp(end, finalColor, i / 100f);

    }

}
