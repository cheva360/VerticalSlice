using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handColliderPos : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Transform handPos;
    [SerializeField] private Transform shoulder;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(handPos.position.x, handPos.position.y, transform.position.z);
        
        // Calculate angle from shoulder to hand on x-y plane
        Vector2 direction = new Vector2(
            handPos.position.x - shoulder.position.x,
            handPos.position.y - shoulder.position.y
        );
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
