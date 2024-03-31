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

    public int getWidth() { return xRight - xLeft; }
    public int getHeight() { return yRight - yLeft; }
    public int[] getCenter() { return new int[] { (xLeft + xRight) / 2, (yLeft + yRight) / 2 }; }

    // Getters and setters
    public int getXLeft() { return xLeft; }
    public int getYLeft() { return yLeft; }
    public int getXRight() { return xRight; }
    public int getYRight() { return yRight; }
    public int getPadding() { return padding; }
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
