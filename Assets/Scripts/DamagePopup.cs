using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float lifetime = 1f;
    private float timer;
    private Vector3 moveUp = new Vector3(0, 2f, 0);

    public static void Create(Vector3 position, int damage)
    {
        GameObject popupObj = Instantiate(
            Resources.Load<GameObject>("DamagePopup"),
            position + Vector3.up * 1.5f,
            Quaternion.identity
        );

        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        popup.Setup(damage);
    }

    public void Setup(int damageAmount)
    {
        textMesh.text = "-" + damageAmount;
        textMesh.color = Color.red;
    }

    void Update()
    {
        transform.position += moveUp * Time.deltaTime; // Felúszik
        timer += Time.deltaTime;

        if (timer > lifetime)
            Destroy(gameObject);
    }
}
