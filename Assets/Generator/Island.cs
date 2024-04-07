using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island
{
    private Vector2Int topLeft, downRight;
    private readonly int padding;
    private Island leftIsland, rightIsland;
    private bool isLeaf = false;

    public Island(int xLeft, int yLeft, int xRight, int yRight, int padding)
    {
        topLeft = new Vector2Int(xLeft, yLeft);
        downRight = new Vector2Int(xRight, yRight);
        this.padding = padding;
    }

    public int getWidth() { return downRight.x - topLeft.x; }
    public int getHeight() { return downRight.y - topLeft.y; }
    public Vector2Int getCenter() { return new Vector2Int((topLeft.x + downRight.x) / 2, (topLeft.y + downRight.y) / 2); }

    // Getters and setters
    public ref Vector2Int getTopLeft() { return ref topLeft; }
    public ref Vector2Int getDownRight() { return ref downRight; }
    public int getPadding() { return padding; }
    public ref Island getLeftIsland() { return ref leftIsland; }
    public void setLeftIsland(Island island) { leftIsland = island; }
    public ref Island getRightIsland() { return ref rightIsland; }
    public void setRightIsland(Island island) { rightIsland = island; }
    public bool getIsLeaf() { return isLeaf; }

    public void setIsLeaf()
    {
        isLeaf = true;
    }
}
