using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ͨ����ڵ�
/// </summary>
public class NormalEnemyNode : MapNodeBase
{
    public uint tplId;   //����ID todo �ĳ��б���߸��������ع���֧�ֶ�������ʼ��

    public NormalEnemyNode(int nodeIndex,uint tplId) : base(MapNodeType.NORMAL_ENEMY,nodeIndex)
    {
        this.tplId = tplId;
    }
}
