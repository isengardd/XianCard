using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ͼ�ڵ���ʾ����ӿ�
/// </summary>
public interface IMapNode
{
    void SetNode(MapNodeBase mapNode);
    MapNodeBase GetNode();
}
