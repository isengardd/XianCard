using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���˽ڵ�
/// </summary>
public class EnemyNode : MapNodeBase
{
    public uint tplId;   //����ID todo �ĳ��б���߸��������ع���֧�ֶ�������ʼ��

    public EnemyNode(uint tplId, float posX, float posY) : base(posX, posY)
    {
        this.tplId = tplId;
    }
}
