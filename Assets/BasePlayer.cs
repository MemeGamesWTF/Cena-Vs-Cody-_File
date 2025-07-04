using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BasePlayer : MonoBehaviour
{
    public Transform spawnPointCenter;

    [Header("Assign one or more bullet prefabs here")]
    public GameObject[] bullets;
    public float Bulletspeed = 5f;

    public AudioSource[] taps;

    void Update()
    {
        if (!GameManager.Instance.GameState)
            return;

        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            PlayRandomTap();
            RotateToMousePosition();
            if (Random.value > 0.4f) // 70% chance to spawn
            {
                SpawnBullet();
            }
        }

        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                PlayRandomTap();
                RotateToTouchPosition(touch.position);
                if (Random.value > 0.4f) // 70% chance to spawn
                {
                    SpawnBullet();
                }
            }
        }
    }

    void RotateToMousePosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float clampedX = Mathf.Clamp(mousePos.x, -2.5f, 2.5f);
        float normalized = (clampedX + 2.5f) / 5f;
        float rotationZ = -Mathf.Lerp(-60f, 60f, normalized);
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    void RotateToTouchPosition(Vector2 touchPos)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(touchPos);
        float clampedX = Mathf.Clamp(worldPos.x, -2.5f, 2.5f);
        float normalized = (clampedX + 2.5f) / 5f;
        float rotationZ = -Mathf.Lerp(-60f, 60f, normalized);
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    void SpawnBullet()
    {
        if (bullets == null || bullets.Length == 0)
        {
            Debug.LogWarning("No bullets assigned!");
            return;
        }

        // pick one of the bullets at random
        int rnd = Random.Range(0, bullets.Length);
        GameObject prefabToSpawn = bullets[rnd];

        GameObject spawnedBullet = Instantiate(
            prefabToSpawn,
            spawnPointCenter.position,
            transform.rotation
        );
        StartCoroutine(MoveUpwards(spawnedBullet));
    }

    IEnumerator MoveUpwards(GameObject obj)
    {
        float initialY = obj.transform.position.y;
        while (obj != null)
        {
            obj.transform.Translate(Vector3.up * Bulletspeed * Time.deltaTime, Space.Self);
            if (obj.transform.position.y - initialY >= 8f)
            {
                Destroy(obj);
                break;
            }
            yield return null;
        }
    }

    private void PlayRandomTap()
    {
        if (taps == null || taps.Length == 0) return;
        int idx = Random.Range(0, taps.Length);
        taps[idx].Play();
    }

    public void GameOver()
    {
        // GameManager.Instance.GameOVer();
    }

    public void Reset()
    {

    }
}
