using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Tilemap m_statics;
    [SerializeField] Tilemap m_dynamics;
    [SerializeField] Tilemap m_stones;

    [SerializeField] Sprite m_stoneSprite;
    [SerializeField] Sprite m_finishSprite;
    [SerializeField] Sprite m_collectableSprite;
    [SerializeField] Sprite m_foodSprite;
    [SerializeField] Sprite m_woodSprite;
    [SerializeField] float m_movementError = 0.25f;

    [SerializeField] int m_maxCollectables = 5;
    [SerializeField] int m_currentCollectables = 0;

    [SerializeField] Tile m_woodTile = null;

    private bool isMoving = false;
    private Animator m_animator = null;

    private void Start()
    {
        //Debug.Log("Vibe check");
        //Debug.Log(string.Format("Player is in {0}", m_dynamics.GetCellCenterLocal(m_dynamics.WorldToCell(transform.position))));
        //Debug.Log(string.Format("Trying to move to {0}", m_dynamics.GetCellCenterLocal(m_dynamics.WorldToCell(transform.position) + Vector3Int.left)));
        //Debug.Log(CanMoveThroughTilemap(Vector3Int.left, m_dynamics, transform.position));
        //Debug.Log(string.Format("Trying to move to {0}", m_dynamics.GetCellCenterLocal(m_dynamics.WorldToCell(transform.position) + Vector3Int.right)));
        //Debug.Log(CanMoveThroughTilemap(Vector3Int.right, m_dynamics, transform.position));
        //Debug.Log(string.Format("Trying to move to {0}", m_dynamics.GetCellCenterLocal(m_dynamics.WorldToCell(transform.position) + Vector3Int.up)));
        //Debug.Log(CanMoveThroughTilemap(Vector3Int.up, m_dynamics, transform.position));
        //Debug.Log(string.Format("Trying to move to {0}", m_dynamics.GetCellCenterLocal(m_dynamics.WorldToCell(transform.position) + Vector3Int.down)));
        //Debug.Log(CanMoveThroughTilemap(Vector3Int.down, m_dynamics, transform.position));

        m_animator = GetComponent<Animator>();

    }

    private void ChangeTileTexture(Vector3Int coord, Sprite sprite)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        m_dynamics.SetTile(coord, tile);
    }

    private IEnumerator MovePlayer(Vector3 startPos, Vector3 endPos)
    {
        isMoving = true;
        float lerpVal = 0f;
        //moves, and also eats if moves on food
        Tile endTileSprite = m_dynamics.GetTile<Tile>(m_dynamics.WorldToCell(endPos));
        if (endTileSprite != null)
        {
            if (endTileSprite.sprite == m_foodSprite)
            {
                ChangeTileTexture(m_dynamics.WorldToCell(endPos), m_woodSprite);
            }
        }

        while (lerpVal < 1f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, lerpVal);
            lerpVal += 0.1f;
            yield return new WaitForSeconds(0.1f);

        }

        if (Vector3.Distance(transform.position, endPos) > m_movementError)
        {
            transform.position = startPos;
        }

        isMoving = false;

    }

    private bool CanMoveThroughTilemap(Vector3Int direction, Tilemap tilemap, Vector3 position)
    {
        //not-passable: stone, collectable, finish
        //all others are passable
        if (tilemap.GetTile<Tile>(tilemap.WorldToCell(position) + direction) == null)
        {
            return true;
        }
        else
        {
            Sprite tileInDirection = tilemap.GetTile<Tile>(tilemap.WorldToCell(position) + direction).sprite;
            if (tileInDirection != null)
            {
                Debug.Log(tileInDirection);
                if (tileInDirection != m_stoneSprite && tileInDirection != m_collectableSprite && tileInDirection != m_finishSprite)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

    private void Interact(Vector3Int direction, Tilemap tilemap, Vector3 position)
    {
        //interactable: finish, collectable
        //all others are passable
        if(tilemap.GetTile<Tile>(tilemap.WorldToCell(position) + direction) == null)
        {
            return;
        }
        else { 
        Sprite tileInDirection = tilemap.GetTile<Tile>(tilemap.WorldToCell(position) + direction).sprite;
            if (tileInDirection != null)
            {
                Debug.Log(tileInDirection);
                if (tileInDirection == m_finishSprite)
                {
                    if (m_currentCollectables >= m_maxCollectables)
                    {
                        Debug.Log("You win. Application.Quit does not work in Editor Mode!");
                        Application.Quit();
                    }
                }
                if (tileInDirection == m_collectableSprite)
                {
                    m_currentCollectables += 1;
                    ChangeTileTexture(tilemap.WorldToCell(position) + direction, m_woodSprite);
                }
            }
        }
        
    }

    private void Update()
    {
        
        if (!isMoving)
        {
            if (Input.GetAxis("Interact") > 0)
            {
                Interact(Vector3Int.left, m_dynamics, transform.position);
                Interact(Vector3Int.right, m_dynamics, transform.position);
                Interact(Vector3Int.up, m_dynamics, transform.position);
                Interact(Vector3Int.down, m_dynamics, transform.position);

            }
            //when selected a direction - need to check for a passable tile, abort if unpassable. Check all tilemaps, as they all contribute to clamped movement
            if(Input.GetAxis("Horizontal") < 0)
            {
                if(CanMoveThroughTilemap(Vector3Int.left, m_dynamics, transform.position) && CanMoveThroughTilemap(Vector3Int.left, m_stones, transform.position))
                {
                    StartCoroutine(MovePlayer(transform.position, m_dynamics.GetCellCenterWorld(m_dynamics.WorldToCell(transform.position) + Vector3Int.left)));
                    m_animator.SetFloat("HorizontalDirection", -1f);
                    m_animator.SetFloat("VerticalDirection", 0f);
                }
            }

            if (Input.GetAxis("Horizontal") > 0)
            {
                if (CanMoveThroughTilemap(Vector3Int.right, m_dynamics, transform.position) && CanMoveThroughTilemap(Vector3Int.right, m_stones, transform.position))
                {
                    StartCoroutine(MovePlayer(transform.position, m_dynamics.GetCellCenterWorld(m_dynamics.WorldToCell(transform.position) + Vector3Int.right)));
                    m_animator.SetFloat("HorizontalDirection", 1f);
                    m_animator.SetFloat("VerticalDirection", 0f);
                }
            }
            
            if (Input.GetAxis("Vertical") < 0)
            {
                if (CanMoveThroughTilemap(Vector3Int.down, m_dynamics, transform.position) && CanMoveThroughTilemap(Vector3Int.down, m_stones, transform.position))
                {
                    StartCoroutine(MovePlayer(transform.position, m_dynamics.GetCellCenterWorld(m_dynamics.WorldToCell(transform.position) + Vector3Int.down)));
                    m_animator.SetFloat("VerticalDirection", -1f);
                    m_animator.SetFloat("HorizontalDirection", 0f);
                }
            }

            if (Input.GetAxis("Vertical") > 0)
            {
                if (CanMoveThroughTilemap(Vector3Int.up, m_dynamics, transform.position) && CanMoveThroughTilemap(Vector3Int.up, m_stones, transform.position))
                {
                    StartCoroutine(MovePlayer(transform.position, m_dynamics.GetCellCenterWorld(m_dynamics.WorldToCell(transform.position) + Vector3Int.up)));
                    m_animator.SetFloat("VerticalDirection", 1f);
                    m_animator.SetFloat("HorizontalDirection", 0f);
                }
            }
        }
    }
}
