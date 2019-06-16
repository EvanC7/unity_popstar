using UnityEngine;
using System.Collections;

public class StarInfo
{
    public Index _index;
    public int _colorType;

    public StarInfo(Index index, int colorType)
    {
        _index = index;
        _colorType = colorType;
    }

    public StarInfo()
    {
        _index = new Index(-1, -1);
        _colorType = -1;
    }
}
