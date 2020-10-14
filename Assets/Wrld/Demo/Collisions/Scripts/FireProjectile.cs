using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [SerializeField]
    private GameObject m_projectile = null;

    [SerializeField]
    private Camera m_camera = null;

    [SerializeField]
    private float m_velocity = 500.0f;

    [SerializeField]
    private float m_lifetime = 10.0f;

    void Update ()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            var screenSpaceMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_camera.nearClipPlane);
            var rayThroughCamera = m_camera.ScreenPointToRay(screenSpaceMousePosition);            
            GameObject projectile = Instantiate(m_projectile, rayThroughCamera.origin, Quaternion.LookRotation(rayThroughCamera.direction)) as GameObject;
            projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * m_velocity;
            projectile.GetComponent<Projectile>().Lifetime = m_lifetime;
        }
    }
}
