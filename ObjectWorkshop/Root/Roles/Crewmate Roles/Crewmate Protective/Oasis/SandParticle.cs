using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class SandParticle(IntPtr ptr) : MonoBehaviour(ptr)
{
    public float speed = 10f;
    public float lifetime = 2f;
    private float timer;
    private SpriteRenderer myRend;

    private void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        timer += Time.deltaTime;

        if (timer >= lifetime) Destroy(gameObject);
        else UpdateColor();
    }

    private bool Visibility() => true;
    private void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }
}