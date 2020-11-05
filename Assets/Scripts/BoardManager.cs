using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public const int MinCandiesToMatch = 2;
    public static BoardManager sharedInstance;
    public List<Sprite> prefabs = new List<Sprite>();
    public GameObject currentCandy;
    public int xSize, ySize;

    private GameObject[,] candies;
    private Candy selectedCandy;

    public bool isShifting { get; set; }

    void Start()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Vector2 offset = currentCandy.GetComponent<BoxCollider2D>().size;
        CreateInitialBoard(offset);
    }

    private void CreateInitialBoard(Vector2 offset)
    {
        candies = new GameObject[xSize, ySize];

        float startX = this.transform.position.x;
        float startY = this.transform.position.y;

        int idx = -1;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                var position = new Vector3(startX + (offset.x * x),
                                           startY + (offset.y * y),
                                           0);
                GameObject newCandy = Instantiate(currentCandy,
                                                  position,
                                                  currentCandy.transform.rotation);
                newCandy.name = $"Candy [{x}] [{y}]";

                do
                {
                    idx = Random.Range(0, prefabs.Count);
                } while (x > 0 && idx == candies[x - 1, y].GetComponent<Candy>().id
                      || (y > 0 && idx == candies[x, y - 1].GetComponent<Candy>().id));

                var sprite = prefabs[idx];
                newCandy.GetComponent<SpriteRenderer>().sprite = sprite;
                newCandy.GetComponent<Candy>().id = idx;
                newCandy.transform.parent = transform;
                candies[x, y] = newCandy;
            }
        }
    }

    public IEnumerator FindCrushedCandies()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (candies[x, y].GetComponent<Candy>().id == -1)
                {
                    yield return StartCoroutine(MakeCandiesFall(x, y));
                    break;
                }
            }
        }
    }

    private IEnumerator MakeCandiesFall(int x, int yStart, float shiftDelay = 0.05f)
    {
        isShifting = true;

        List<Candy> fallingCandies = new List<Candy>();
        int nullCandies = 0;

        for (int y = yStart; y < ySize; y++)
        {
            Candy candy = candies[x, y].GetComponent<Candy>();
            if (candy.id == -1)
            {
                nullCandies++;
            }
            fallingCandies.Add(candy);
        }

        for (int i = 0; i < nullCandies; i++)
        {
            yield return new WaitForSeconds(shiftDelay);
            for (int j = 0; j < fallingCandies.Count - 1; j++)
            {
                fallingCandies[j].ChangeType(fallingCandies[j + 1]);
                fallingCandies[j + 1].Clear();
            }
        }

        isShifting = false;
    }
}
