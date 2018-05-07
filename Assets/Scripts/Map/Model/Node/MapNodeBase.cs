using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ͼ�ڵ����ݻ���
/// </summary>
public class MapNodeBase
{
    public bool isPass = false; //�Ƿ�ͨ��
    public MapNodeType nodeType;
    public int nodeIndex { get; private set; }

    public MapNodeBase(MapNodeType nodeType, int nodeIndex)
    {
        this.nodeType = nodeType;
        this.nodeIndex = nodeIndex;
    }
}
