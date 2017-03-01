using UnityEngine;

public class GustItem : MonoBehaviour, IItem {

    public Sprite icon;
    public Sprite Icon {  get { return icon; } }
    public GameObject prefabGust;
    public int cooldownFrames = 90;
    [ReadOnly]
    public int remFrames = 0;
    public float force = 1200f;

    public float CheckCooldown()
    {
        return (float)remFrames / cooldownFrames;
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (remFrames > 0)
        {
            remFrames--;
            return;
        }
	}

    public void Activate(Player player)
    {
        if (remFrames > 0)
            return;
        remFrames = cooldownFrames;
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position).normalized;
        GameObject gust = Instantiate(prefabGust, (Vector2)transform.position, Quaternion.LookRotation(Vector3.forward, direction));
        gust.GetComponent<Rigidbody2D>().AddForce(direction * force);
        player.GetComponent<Rigidbody2D>().AddForce(-direction * force);
    }
}
