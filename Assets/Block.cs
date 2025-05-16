using UnityEngine;
using UnityEngine.Tilemaps;

public class BrickTilemap : MonoBehaviour
{
    private Tilemap tilemap;
    private bool isAnimating = false;
    public float bumpHeight = 0.2f;
    public float bumpDuration = 0.2f;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAnimating)
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.05f)
                {
                    Player player = collision.gameObject.GetComponent<Player>();
                    if (player != null && !player.IsBig)
                    {
                        Vector3 hitPosition = contact.point;
                        Vector3Int tilePosition = tilemap.WorldToCell(hitPosition - new Vector3(0.05f, 0.1f, 0f));
                        if (tilemap.HasTile(tilePosition))
                        {
                            Vector3 tileCenter = tilemap.GetCellCenterWorld(tilePosition);
                            player.BumpBlock();
                            StartCoroutine(BumpAnimation(tilePosition));
                        }
                        else
                        {
                            Vector3Int tileAbove = tilePosition + new Vector3Int(0, 1, 0);
                            if (tilemap.HasTile(tileAbove))
                            {
                                Vector3 tileCenter = tilemap.GetCellCenterWorld(tileAbove);
                                player.BumpBlock();
                                StartCoroutine(BumpAnimation(tileAbove));
                            }
                        }
                    }
                    break;
                }
            }
        }
    }

    System.Collections.IEnumerator BumpAnimation(Vector3Int tilePosition)
    {
        isAnimating = true;
        float elapsed = 0f;
        Vector3 startPosition = tilemap.GetCellCenterWorld(tilePosition);
        Vector3 targetPosition = startPosition + new Vector3(0f, bumpHeight, 0f);
        Matrix4x4 originalMatrix = tilemap.GetTransformMatrix(tilePosition);

        while (elapsed < bumpDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (bumpDuration / 2);
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
            Vector3 offset = currentPosition - startPosition;
            tilemap.SetTransformMatrix(tilePosition, originalMatrix * Matrix4x4.Translate(offset));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < bumpDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (bumpDuration / 2);
            Vector3 currentPosition = Vector3.Lerp(targetPosition, startPosition, t);
            Vector3 offset = currentPosition - startPosition;
            tilemap.SetTransformMatrix(tilePosition, originalMatrix * Matrix4x4.Translate(offset));
            yield return null;
        }

        tilemap.SetTransformMatrix(tilePosition, originalMatrix);
        isAnimating = false;
    }
}