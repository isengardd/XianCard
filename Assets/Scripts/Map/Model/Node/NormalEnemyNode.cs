using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ͨ����ڵ�
/// </summary>
public class NormalEnemyNode : MapNodeBase
{
    public uint tplId;   //����ID todo �ĳ��б���߸��������ع���֧�ֶ�������ʼ��

    public NormalEnemyNode(uint tplId) : base(MapNodeType.NORMAL_ENEMY)
    {
        this.tplId = tplId;
    }
}
