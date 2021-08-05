using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public bool ınputDelay = true;

    private void Update()
    {
        if (ınputDelay)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnMouseDown();
            }
        }
    }

    private void OnMouseDown()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit)
        {
            Block selectedBlock = hit.transform.GetComponent<Block>();
            if (selectedBlock.connectedBlocks.Count > 1)
            {
                BlockManager.Instance.DeleteBlocks(selectedBlock.connectedBlocks);
            }
        }
    }
}