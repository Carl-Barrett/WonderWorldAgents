using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithinBounds : MonoBehaviour
{
    public float step_size;
    private Vector3 direction;
    private Vector3 acceleration;

    private Vector3 initialPosition;

    private float directionX;
    private float directionY;
    private float directionZ;

    public BoxCollider bounds;

    private Vector3 possible_pos;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void Init()
    {
        initialPosition = transform.position;

        directionX = Random.Range(-1, 2);
        directionY = Random.Range(-1, 2);
        directionZ = Random.Range(-1, 2);

        direction = new Vector3(directionX, directionY, directionZ);

        acceleration = new Vector3(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
    }

    public void Move()
    {
        possible_pos = Vector3.zero;
        direction += acceleration;
        possible_pos = transform.position + direction * step_size;
        Constrain();

        transform.position += direction * step_size;

        acceleration = Vector3.zero;
    }

    public void ResetAttractorLocation()
    {
        transform.position = initialPosition;
        Init();
    }

    public void Constrain()
    {
        float boundsX = bounds.size.x;
        float boundsY = bounds.size.y;
        float boundsZ = bounds.size.x;

        float posX = bounds.transform.position.x;
        float posY = bounds.transform.position.y;
        float posZ = bounds.transform.position.z;

        float minX = posX - boundsX/2;
        float maxX = posX + boundsX/2;
        float minY = posY - boundsY/2;
        float maxY = posY + boundsY/2;
        float minZ = posZ - boundsZ/2;
        float maxZ = posZ + boundsZ/2;

        Vector3 size = transform.localScale;

        if (possible_pos.x > maxX - size.x/2)
        {
            direction.x = -direction.x;
        }

        if (possible_pos.x < minX + size.x/2 )
        {
            direction.x = -direction.x;
        }

        if (possible_pos.z > maxZ - size.z/2 )
        {
            direction.z = -direction.z;
        }

        if (possible_pos.z < minZ + size.z/2)
        {
            direction.z = -direction.z;
        }

        if (possible_pos.y > maxY - size.y/2 )
        {
            direction.y = -direction.y;
        }

        if (possible_pos.y < minY + size.y/2 )
        {
            direction.y = -direction.y;
        }
    }
}