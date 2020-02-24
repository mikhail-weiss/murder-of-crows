using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Crow : MonoBehaviour
{
    public Vector3 baseRotation;

    [Range(0, 10)]
    public float maxSpeed = 1f;

    [Range(.1f, .5f)]
    public float maxForce = .03f;

    [Range(.1f, 10)]
    public float neighborhoodRadius = 3f;

    [Range(0, 50)]
    public float separationAmount = 100f;

    [Range(0, 3)]
    public float cohesionAmount = 1f;

    [Range(0, 3)]
    public float alignmentAmount = 1f;

    public Vector3 acceleration;
    public Vector3 velocity;

    private Vector3 Position
    {
        get
        {
            return gameObject.transform.position;
        }
        set
        {
            gameObject.transform.position = value;
        }
    }

    private void Start()
    {
        var animator = GetComponent<Animator>();
        animator.SetFloat("animationStart", Random.value);
        
        float angle = Random.Range(0, Mathf.PI/2);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
        velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void Update()
    {
        Collider[] boidColliders = Physics.OverlapSphere(Position, neighborhoodRadius);
        var boids = boidColliders.Select((o)=>o.GetComponent<Crow>()).Where((o)=>o != null && o != this).ToList();

        //boids.Remove(this);

        Flock(boids);
        UpdateVelocity();
        UpdatePosition();
        UpdateRotation();
        WrapAround();
    }

    private void OnDrawGizmos()
    {
        Collider[] boidColliders = Physics.OverlapSphere(Position, neighborhoodRadius);
        var boids = boidColliders.Select((o) =>
        {
            return o.GetComponent<Crow>();
        }).Where((o) => o != null && o != this).ToList();

        Vector3 alignment = Alignment(boids) * 10;
        Vector3 separation = Separation(boids) * 10;
        Vector3 cohesion = Cohesion(boids) * 10;

        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + alignment.x, transform.position.y + alignment.y, transform.position.z));

        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + separation.x, transform.position.y + separation.y, transform.position.z));

        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + cohesion.x, transform.position.y + cohesion.y, transform.position.z));


        Vector3 direction = Vector3.zero;
        var tooCloseBoids = boids.Where(o => DistanceTo(o) <= neighborhoodRadius / 2);

        foreach (var boid in tooCloseBoids)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(boid.transform.position, 0.2f);

            Vector3 difference = Position - boid.Position;
            direction += difference.normalized / difference.magnitude;
            Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + direction.x, transform.position.y + direction.y, transform.position.z));

        }
        direction /= tooCloseBoids.Count();
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + direction.x, transform.position.y + direction.y, transform.position.z));

        //Gizmos.DrawWireSphere(transform.position, neighborhoodRadius / 2);
        Gizmos.DrawSphere(transform.position, transform.position.z);
    }

    private void Flock(IEnumerable<Crow> boids)
    {
        Vector3 alignment = Alignment(boids);
        Vector3 separation = Separation(boids);
        Vector3 cohesion = Cohesion(boids);

        acceleration = alignmentAmount * alignment + cohesionAmount * cohesion + separationAmount * separation;
    }

    public void UpdateVelocity()
    {
        velocity += acceleration;
        velocity = LimitMagnitude(velocity, maxSpeed);
    }

    private void UpdatePosition()
    {
        Position += velocity * Time.deltaTime;
    }

    private void UpdateRotation()
    {
        var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
    }

    private Vector2 Alignment(IEnumerable<Crow> boids)
    {
        var velocity = Vector3.zero;
        if (!boids.Any()) return velocity;

        foreach (var boid in boids)
        {
            velocity += boid.velocity;
        }
        velocity /= boids.Count();

        var steer = Steer(velocity.normalized * maxSpeed);
        return steer;
    }

    private Vector2 Cohesion(IEnumerable<Crow> boids)
    {
        if (!boids.Any()) return Vector2.zero;

        Vector3 sumPositions = Vector3.zero;
        foreach (var boid in boids)
        {
            sumPositions += boid.Position;
        }
        var average = sumPositions / boids.Count();
        var direction = average - Position;

        var steer = Steer(direction.normalized * maxSpeed);
        return steer;
    }

    private Vector2 Separation(IEnumerable<Crow> boids)
    {
        Vector3 direction = Vector3.zero;
        var tooCloseBoids = boids.Where(o => DistanceTo(o) <= neighborhoodRadius*3);
        if (!tooCloseBoids.Any()) return direction;

        foreach (var boid in tooCloseBoids)
        {
            var difference = Position - boid.Position;
            direction += difference.normalized / difference.magnitude;
        }
        direction /= tooCloseBoids.Count();

        var steer = Steer(direction.normalized * maxSpeed);
        return steer;
    }

    private Vector2 Steer(Vector3 desired)
    {
        var steer = desired - velocity;
        steer = LimitMagnitude(steer, maxForce);

        return steer;
    }

    private float DistanceTo(Crow boid)
    {
        return Vector3.Distance(boid.transform.position, Position);
    }

    private Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
    {
        if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            baseVector = baseVector.normalized * maxMagnitude;
        }
        return baseVector;
    }

    private void WrapAround()
    {
        if (Position.x < -10) Position = new Vector2(10, Position.y);
        if (Position.y < -6) Position = new Vector2(Position.x, 6);
        if (Position.x > 10) Position = new Vector2(-10, Position.y);
        if (Position.y > 6) Position = new Vector2(Position.x, -6);
    }
}