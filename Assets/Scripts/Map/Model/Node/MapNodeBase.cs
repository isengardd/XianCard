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

    public MapNodeBase(MapNodeType nodeType)
    {
        this.nodeType = nodeType;
    }
}
