using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public ColorData blockColor;
    public List<Block> connectedBlocks = new List<Block>();
    public bool isChecked;
    public bool isAdded;
    public int x;
    public int y;
    public int dropCount;
    public bool shuffled;

    private SpriteRenderer[] renderers = new SpriteRenderer[4];

    
    public void CheckRecursive(int x, int y, Block mainBlock)
    {
        BlockManager blockGen = BlockManager.Instance;
        if (x != 0)
        {
            if (!blockGen.grid[x - 1, y].isChecked)
            {
                if (blockGen.grid[x - 1, y].blockColor == blockGen.grid[x, y].blockColor)
                {
                    if (!mainBlock.connectedBlocks.Contains(blockGen.grid[x - 1, y]))
                    {
                        mainBlock.connectedBlocks.Add(blockGen.grid[x - 1, y]);
                        blockGen.grid[x - 1, y].isChecked = true;
                        blockGen.grid[x - 1, y].CheckRecursive(x - 1, y, mainBlock);
                    }
                }
            }
        }

        if (x != blockGen.width - 1)
        {
            if (!blockGen.grid[x + 1, y].isChecked)
            {
                if (blockGen.grid[x + 1, y].blockColor == blockGen.grid[x, y].blockColor)
                {
                    if (!mainBlock.connectedBlocks.Contains(blockGen.grid[x + 1, y]))
                    {
                        mainBlock.connectedBlocks.Add(blockGen.grid[x + 1, y]);
                        blockGen.grid[x + 1, y].isChecked = true;
                        blockGen.grid[x + 1, y].CheckRecursive(x + 1, y, mainBlock);
                    }
                }
            }
        }

        if (y != 0)
        {
            if (!blockGen.grid[x, y - 1].isChecked)
            {
                if (blockGen.grid[x, y - 1].blockColor == blockGen.grid[x, y].blockColor)
                {
                    if (!mainBlock.connectedBlocks.Contains(blockGen.grid[x, y - 1]))
                    {
                        mainBlock.connectedBlocks.Add(blockGen.grid[x, y - 1]);
                        blockGen.grid[x, y - 1].isChecked = true;
                        blockGen.grid[x, y - 1].CheckRecursive(x, y - 1, mainBlock);
                    }
                }
            }
        }

        if (y != blockGen.height - 1)
        {
            if (!blockGen.grid[x, y + 1].isChecked)
            {
                if (blockGen.grid[x, y + 1].blockColor == blockGen.grid[x, y].blockColor)
                {
                    if (!mainBlock.connectedBlocks.Contains(blockGen.grid[x, y + 1]))
                    {
                        mainBlock.connectedBlocks.Add(blockGen.grid[x, y + 1]);
                        blockGen.grid[x, y + 1].isChecked = true;
                        blockGen.grid[x, y + 1].CheckRecursive(x, y + 1, mainBlock);
                    }
                }
            }
        }
    }


    public void ChangeImage()
    {
        
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();
        }

        int sort = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                sort = renderers[i].sortingOrder;
            }
        }

        if (connectedBlocks.Count <= BlockManager.Instance.iconA)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
        }

        if (connectedBlocks.Count > BlockManager.Instance.iconA && connectedBlocks.Count <= BlockManager.Instance.iconB)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
        }

        if (connectedBlocks.Count > BlockManager.Instance.iconB && connectedBlocks.Count <= BlockManager.Instance.iconC)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(false);
        }

        if (connectedBlocks.Count > BlockManager.Instance.iconC)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(true);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                renderers[i].sortingOrder = sort;
            }
        }
    }

    public void BlockListShare()
    {
        for (int i = 1; i < connectedBlocks.Count; i++)
        {
            for (int j = 0; j < connectedBlocks.Count; j++)
            {
                if (!connectedBlocks[i].connectedBlocks.Contains(connectedBlocks[j]))
                {
                    connectedBlocks[i].connectedBlocks.Add(connectedBlocks[j]);
                }
            }
        }
    }


    private void OnEnable()
    {
        BlockManager.OnImageChange += ChangeImage;

    }

    private void OnDisable()
    {
        BlockManager.OnImageChange -= ChangeImage;
    }

    public enum ColorData
    {
        blue,
        red,
        green,
        purple,
        yellow,
        pink
    }
}