using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ͼ�ڵ����ݻ���
/// </summary>
public class MapNodeBase
{
    public float posX { get; private set; }    //����
    public float posY { get; private set; }    //����
    public bool isPass = false; //�Ƿ�ͨ��

    public MapNodeBase(float posX, float posY)
    {
        this.posX = posX;
        this.posY = posY;
    }
}
