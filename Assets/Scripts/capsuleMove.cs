using UnityEngine;
using Fusion;

public class CapsuleMove : NetworkBehaviour
{
    public float speed = 5f;

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);

        transform.Translate(move * speed * Time.deltaTime);
    }
}