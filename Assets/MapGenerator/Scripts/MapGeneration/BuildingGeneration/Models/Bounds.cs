using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct buildingBounds
{
    public int minRow;
    public int maxRow;
    public int minCol;
    public int maxCol;

    public buildingBounds(int minRow, int minCol, int maxRow, int maxCol)
    {
        this.minRow = minRow;
        this.minCol = minCol;
        this.maxRow = maxRow;
        this.maxCol = maxCol;
    }
}
