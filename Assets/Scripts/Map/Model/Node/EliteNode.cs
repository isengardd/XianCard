using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ӣ�ڵ�
/// </summary>
public class EliteNode : MapNodeBase
{
    public uint tplId;   //����ID todo �ĳ��б���߸��������ع���֧�ֶ�������ʼ��

    public EliteNode(int nodeIndex, uint tplId) : base(MapNodeType.ELITE, nodeIndex)
    {
        this.tplId = tplId;
    }
}
