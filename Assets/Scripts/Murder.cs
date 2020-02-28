using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Murder : MonoBehaviour
{
    [Range(10, 500)]
    public int startingCount = 250;

    public float minVelocity = 5;
    public float maxVelocity = 100;
    public float randomness = 1;
    public int flockSize = 5;


    public Crow prefab;

    internal Vector2 target;
    internal Vector2 flockCenter;
    internal Vector2 flockVelocity;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 initialPosition = new Vector3(
                            Random.value * GetComponent<Collider2D>().bounds.size.x,
                            Random.value * GetComponent<Collider2D>().bounds.size.y,
                            Random.value-0.5f);
            Crow boid = Instantiate(prefab, initialPosition, transform.rotation) as Crow;

            boid.maxForce = boid.maxForce * (1 + (Random.value * 0.1f));
            boid.maxSpeed = boid.maxSpeed * (1 + (Random.value * 0.1f));

            boid.transform.parent = transform;
            
            //boid.controller = this;            
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Ray screenPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
            target = new Vector2(screenPoint.origin.x, screenPoint.origin.y);            
        }

        Vector2 center = Vector2.zero;
        Vector2 velocity = Vector2.zero;

        Crow[] crows = GetComponentsInChildren<Crow>();
        //foreach (Crow crow in crows)
        //{
        //    center += crow.body.position;
        //    velocity += crow.body.velocity;
        //}
        //flockCenter = center / flockSize;
        //flockVelocity = velocity / flockSize;

    }
}
