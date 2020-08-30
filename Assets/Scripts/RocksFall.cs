using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RocksFall : MonoBehaviour
{
    [SerializeField] Tilemap m_stones = null;
    [SerializeField] Tilemap m_dynamics = null;
    [SerializeField] Sprite m_placeholder;
    [SerializeField] List<Vector3> availablePlaces;
    [SerializeField] GameObject m_fake;

    [SerializeField] Sprite m_stoneSprite = null;
    [SerializeField] Sprite m_woodSprite = null;

    private Sprite Neighbour(Vector3Int direction, Tilemap tilemap, Vector3 position)
    {
        //not-passable: stone, collectable, finish
        //all others are passable
        Sprite tileInDirection = null;
        if (tilemap.GetTile<Tile>(tilemap.WorldToCell(position) + direction) != null)
        {
            tileInDirection = tilemap.GetTile<Tile>(tilemap.WorldToCell(position) + direction).sprite;
        }
        if (tileInDirection != null)
        {
            return tileInDirection;
        }
        return m_placeholder;
    }

    //KILL ME FOR DRY
    private void ChangeTileTexture(Vector3Int coord, Sprite sprite)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        m_stones.SetTile(coord, tile);
    }

    private IEnumerator MoveStone(Vector3 startPos, Vector3 endPos)
    {
        GameObject a = Instantiate<GameObject>(m_fake, m_stones.GetCellCenterLocal(m_stones.WorldToCell(startPos)), Quaternion.identity);
        float lerpVal = 0f;
        ChangeTileTexture(m_stones.WorldToCell(startPos), null);
        
        while(lerpVal < 0.75f)
        {
            a.transform.position = Vector3.Lerp(m_stones.GetCellCenterLocal(m_stones.WorldToCell(startPos)), endPos, lerpVal);
            lerpVal += 0.1f;
            yield return new WaitForSeconds(0.1f);

        }
        
        ChangeTileTexture(m_stones.WorldToCell(endPos), m_stoneSprite);
        Destroy(a);
    }

    private void Start()
    {
        StartCoroutine(UpdateStones());
    }

    private IEnumerator UpdateStones()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.25f);
            
            availablePlaces = new List<Vector3>();

            for (int n = m_stones.cellBounds.xMin; n < m_stones.cellBounds.xMax; n++)
            {
                for (int p = m_stones.cellBounds.yMin; p < m_stones.cellBounds.yMax; p++)
                {
                    Vector3Int localPlace = (new Vector3Int(n, p, (int)m_stones.transform.position.y));
                    Vector3 place = m_stones.CellToWorld(localPlace);
                    if (m_stones.HasTile(localPlace) )
                    {
                        //Tile at "place"
                        if (m_stones.GetTile<Tile>(localPlace).sprite == m_stoneSprite)
                        {
                            availablePlaces.Add(place);
                        }
                    }
                    else
                    {
                        //No tile at "place"
                    }
                }
            }
            foreach (Vector3 a in availablePlaces)
            {
                //d)
                if (Neighbour(Vector3Int.down, m_dynamics, a) == m_woodSprite && Neighbour(Vector3Int.down, m_stones, a) != m_stoneSprite)
                {
                    
                    StartCoroutine(MoveStone(a, m_stones.GetCellCenterWorld(m_stones.WorldToCell(a)+Vector3Int.down))); //1 down
                    break;
                }
                //a)
                if (Neighbour(Vector3Int.up, m_stones, a) == m_stoneSprite)
                {
                    continue;
                }
                else
                {
                    //b)
                    if (Neighbour(Vector3Int.left, m_dynamics, a) == m_woodSprite && Neighbour(Vector3Int.left, m_stones, a) != m_stoneSprite)
                    {
                        //c1)
                        if (Neighbour(Vector3Int.down + Vector3Int.left, m_dynamics, a ) == m_woodSprite && Neighbour(Vector3Int.down + Vector3Int.left, m_stones, a) != m_stoneSprite)
                        {
                            StartCoroutine(MoveStone(a, m_stones.GetCellCenterWorld(m_stones.WorldToCell(a) + Vector3Int.left)));
                            break;
                        }
                    }
                    else
                    {
                        if (Neighbour(Vector3Int.right, m_dynamics, a) == m_woodSprite && Neighbour(Vector3Int.right, m_stones, a) != m_stoneSprite)
                        {
                            //c2)
                            if (Neighbour(Vector3Int.down + Vector3Int.right, m_dynamics, a ) == m_woodSprite && Neighbour(Vector3Int.down + Vector3Int.right, m_stones, a ) != m_stoneSprite)
                            {
                                StartCoroutine(MoveStone(a, m_stones.GetCellCenterWorld(m_stones.WorldToCell(a) + Vector3Int.right)));
                                break;
                            }

                        }
                    }
                    yield return null;
                }
            }
            yield return null;
        }
    }
}

//rules:
//a) if above there is 1 more stone - do nothing
//b) if no stone above: check right and left for movability
//c) if not movable - stay where you are, else go to first movable direction (only if under this direction there is emptiness)
//d) if there is movable tile right beneath you - move there