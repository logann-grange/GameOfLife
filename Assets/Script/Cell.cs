using UnityEngine;

public class Cell : MonoBehaviour
{

    public bool isAlive = false;
    public bool isBuffer = false;
    public int numNeighboors = 0;

    public void SetAlive(bool alive)
    {
        isAlive = alive;
        GetComponent<SpriteRenderer>().enabled = alive && !isBuffer;
    }
}
