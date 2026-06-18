using UnityEngine;

public class BoatSailing : MonoBehaviour
{
    public float speed = 2f;
    public float pathRadius = 15f;
    public float turnSpeed = 0.3f;

    Vector3 center;

    void Start()
    {
        center = transform.position;
    }

    void Update()
    {
        float angle = Time.time * turnSpeed;
        Vector3 target = center + new Vector3(Mathf.Sin(angle) * pathRadius, 0f, Mathf.Cos(angle) * pathRadius);

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 1.5f);
        }

        Vector3 move = transform.forward * speed * Time.deltaTime;
        Vector3 pos = transform.position;
        pos.x += move.x;
        pos.z += move.z;
        transform.position = pos;
    }
}
