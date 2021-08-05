using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


public class BlockManager : Singleton<BlockManager>
{
    [Header("Icon Prefab Gameobject")] [SerializeField]
    private List<Transform> blocksPrefab = new List<Transform>();

    [Header("Map Settings")] [Range(2, 10)]
    public int width;

    [Range(2, 12)] public int height;

    [Header("Color Settings")] [Range(1, 6)]
    public int colorRange;

    [Tooltip(" <= A -- default icon")] public int iconA;
    [Tooltip("A< <=B -- iconA")] public int iconB;

    [Tooltip("B< <=C -- iconB  more than C -- iconC")]
    public int iconC;

    private float blockWidth = 2.25f;
    private float blockHeight = 2.25f;
    public Block[,] grid;

    public bool isShuffle;

    public static event Action OnImageChange;
    public Block blockListShare;
    public static event Action OnListShare;


    private void Start()
    {
        grid = new Block[width, height];
        BlockGridGenerate();
    }

    private void BlockGridGenerate()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 ınstantiatePos = new Vector3(blockWidth * i, blockHeight * j);
                Transform generatedBlock = transform;
                NewBlockGenerate(ınstantiatePos, generatedBlock, i, j);
            }
        }

        BlockMatchController();
    }

    public void BlockMatchController()
    {
        //Checks if neighbors match.
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!grid[i, j].isChecked)
                {
                    grid[i, j].isChecked = true;
                    if (i != 0)
                    {
                        CheckArround(i, j, -1, 0);
                    }

                    if (i != width - 1)
                    {
                        CheckArround(i, j, 1, 0);
                    }

                    if (j != 0)
                    {
                        CheckArround(i, j, 0, -1);
                    }

                    if (j != height - 1)
                    {
                        CheckArround(i, j, 0, 1);
                    }
                }
            }
        }

        // Linked boxes share lists between them.
        ListShare();
        //Checks whether mixing is necessary.
        ShuffleController();
    }

    public void ListShare()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!grid[i, j].isAdded)
                {
                    for (int k = 0; k < grid[i, j].connectedBlocks.Count; k++)
                    {
                        for (int l = 0; l < grid[i, j].connectedBlocks.Count; l++)
                        {
                            if (!grid[i, j].connectedBlocks[k].connectedBlocks.Contains(grid[i, j].connectedBlocks[l]))
                            {
                                grid[i, j].connectedBlocks[k].connectedBlocks.Add(grid[i, j].connectedBlocks[l]);
                                grid[i, j].connectedBlocks[k].isAdded = true;
                            }
                        }
                    }
                }
            }
        }

        //block ımage ınvoke
        OnImageChange?.Invoke();
        //Resets bools that prevent unnecessary loops
        ResetControl();
    }

    private void ShuffleController()
    {
        int shuffleCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j].connectedBlocks.Count > 0)
                {
                    shuffleCount++;
                    break;
                }
            }
        }

        if (shuffleCount == 0)
        {
            isShuffle = true;
            StartCoroutine(BlocksShuffle());
        }
    }

    private IEnumerator BlocksShuffle()
    {
        InputManager.Instance.ınputDelay = false;
        yield return new WaitForSeconds(0.5f);

        List<Block> shuffleList = new List<Block>();
        int random;
        Vector3 moveTrans;

        //All matrix member added list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                shuffleList.Add(grid[i, j]);
                grid[i, j] = null;
            }
        }

        // Generates random matrix from list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                random = Random.Range(0, shuffleList.Count);
                grid[i, j] = shuffleList[random];
                grid[i, j].x = i;
                grid[i, j].y = j;

                moveTrans = new Vector3(i * blockWidth, j * blockHeight);
                shuffleList[random].transform.DOMove(moveTrans, 0.5f);

                BlockLayer(grid[i, j].transform, j);

                shuffleList.Remove(shuffleList[random]);
            }
        }

        BlockMatchController();
        isShuffle = false;
        InputManager.Instance.ınputDelay = true;
    }

    public void DeleteBlocks(List<Block> deletedBlocks)
    {
        for (int i = 0; i < deletedBlocks.Count; i++)
        {
            int delY = deletedBlocks[i].y;
            int delX = deletedBlocks[i].x;
            grid[deletedBlocks[i].x, deletedBlocks[i].y] = null;
            for (int j = delY + 1; j < height; j++)
            {
                if (grid[delX, j] != null)
                {
                    grid[delX, j].dropCount++;
                }
            }

            Destroy(deletedBlocks[i].gameObject);
        }

        InputManager.Instance.ınputDelay = false;
        //After the explosion, the top blocks fall down
        StartCoroutine(FallBlocks());
    }

    private IEnumerator FallBlocks()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j] != null)
                {
                    if (grid[i, j].dropCount > 0)
                    {
                        Transform trans = grid[i, j].transform;
                        trans.DOMoveY(trans.position.y - (grid[i, j].dropCount * blockHeight), 0.5f);
                        int fallCount = grid[i, j].dropCount;
                        grid[i, j].dropCount = 0;
                        Block block = grid[i, j];
                        block.y = j - fallCount;
                        BlockLayer(block.transform, block.y);

                        grid[i, j] = null;
                        grid[i, j - fallCount] = block;
                    }
                }
            }
        }

        // Generated new boxes in the spaces.
        StartCoroutine(FallNewBlocks());
    }

    private IEnumerator FallNewBlocks()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j] == null)
                {
                    float dropHeight = height * blockHeight;
                    Vector3 ınstantiatePos = new Vector3(blockWidth * i, dropHeight + j * blockHeight);
                    Transform generatedBlock = transform;

                    NewBlockGenerate(ınstantiatePos, generatedBlock, i, j);
                }
            }
        }

        // Clear matched blocks list.
        ClearConnectedList();
        BlockMatchController();
        yield return new WaitForSeconds(0.5f);
        InputManager.Instance.ınputDelay = true;
    }


    private void ResetControl()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j].isAdded = false;
                grid[i, j].isChecked = false;
            }
        }
    }

    private void ClearConnectedList()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j].connectedBlocks.Clear();
            }
        }
    }

    private void CheckArround(int x, int y, int plusX, int plusY)
    {
        if (!grid[x + plusX, y + plusY].isChecked)
        {
            if (grid[x, y].blockColor == grid[x + plusX, y + plusY].blockColor)
            {
                if (!grid[x, y].connectedBlocks.Contains(grid[x + plusX, y + plusY]))
                {
                    if (!grid[x, y].connectedBlocks.Contains(grid[x, y]))
                    {
                        grid[x, y].connectedBlocks.Add(grid[x, y]);
                    }

                    grid[x, y].connectedBlocks.Add(grid[x + plusX, y + plusY]);
                    grid[x + plusX, y + plusY].isChecked = true;
                    grid[x + plusX, y + plusY].CheckRecursive(x + plusX, y + plusY, grid[x, y]);
                }
            }
        }
    }

    private void NewBlockGenerate(Vector3 ınstantiatePos, Transform generatedBlock, int x, int y)
    {
        int randomColor = Random.Range(0, colorRange);
        Vector3 endPos = new Vector3(blockWidth * x, blockHeight * y);

        generatedBlock = Instantiate(blocksPrefab[randomColor], ınstantiatePos, quaternion.identity);
        generatedBlock.name = "Block" + x + y;

        generatedBlock.DOMoveY(endPos.y, 0.5f).SetEase(Ease.OutBounce);

        grid[x, y] = generatedBlock.GetComponent<Block>();
        grid[x, y].x = x;
        grid[x, y].y = y;

        BlockLayer(generatedBlock, y);
    }


    private void BlockLayer(Transform generatedBlock, int layerOrder)
    {
        SpriteRenderer[] graphics = generatedBlock.GetComponentsInChildren<SpriteRenderer>();
        for (int k = 0; k < graphics.Length; k++)
        {
            graphics[k].sortingOrder = layerOrder;
        }
    }
}