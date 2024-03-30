using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island
{
    public Island(int xLeft, int yLeft, int xRight, int yRight, int padding)
    {
        this.xLeft = xLeft;
        this.yLeft = yLeft;
        this.xRight = xRight;
        this.yRight = yRight;
        this.padding = padding;
    }

    public void generateLand(ref TileType[,] grid)
    {
        for (int i = xLeft + padding; i < xRight - padding; i++) {
            for (int j = yLeft + padding; j < yRight - padding; j++) {
                grid[i, j] = TileType.Land;
            }
        }
    }

    public int getWidth() { return xRight - xLeft; }
    public int getHeight() { return yRight - yLeft; }

    // Getters and setters
    public int getXLeft() { return xLeft; }
    public void setXLeft(int xLeft) { this.xLeft = xLeft; }
    public int getYLeft() { return yLeft; }
    public void setYLeft(int yLeft) { this.yLeft = yLeft; }
    public int getXRight() { return xRight; }
    public void setXRight(int xRight) { this.xRight = xRight; }
    public int getYRight() { return yRight; }
    public void setYRight(int yRight) { this.yRight = yRight; }
    public ref Island getLeftIsland() { return ref leftIsland; }
    public void setLeftIsland(Island island) { leftIsland = island; }
    public ref Island getRightIsland() { return ref rightIsland; }
    public void setRightIsland(Island island) { rightIsland = island; }
    public bool getIsLeaf() { return isLeaf; }
    public void setIsLeaf() { isLeaf = true; }

    // private:
    int xLeft, yLeft, xRight, yRight;
    int padding;
    Island leftIsland, rightIsland;
    bool isLeaf = false;
}
