using System;
using System.Collections;
using System.Collections.Generic;
using UI.Map;
using UnityEngine;

/// <summary>
/// ���ͼ���ݲ�
/// </summary>
public class MapModel : ModelBase
{
    #region
    private readonly static MapModel _inst = new MapModel();
    static MapModel() { }
    public static MapModel Inst { get { return _inst; } }
    #endregion

    //define
    public const int NODE_NUM = 16; //ÿ���ͼ����BOSS����Ľڵ�����
    private const int SINGLE_HEIGHT = 120; //��������ռ�ݵĸ߶�
    private const int BOSS_HEIGHT = 500; //BOSSռ�ݵĸ߶�

    public MapNodeBase enterNode { get; private set; }  //��ǰ����Ľڵ�

    private List<List<MapNodeBase>> _lstOfLstMapNode = new List<List<MapNodeBase>>();
    private int _currentLayerIndex = 0; //��ǰ��ͼ��ε�����

    private List<Type> _lstBlock;

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Init()
    {
        //todo ����ְҵ��ʼ��
        List<MapNodeBase> lstMapNode = new List<MapNodeBase>();
        //todo ���벻ͬ�Ľڵ�����
        for (int i = 0; i < NODE_NUM; i++)
        {
            lstMapNode.Add(new EnemyNode(1, 700, BOSS_HEIGHT + i * SINGLE_HEIGHT + UnityEngine.Random.Range(0, SINGLE_HEIGHT)));   //todo �������ID ���x����
        }
        _lstOfLstMapNode.Add(lstMapNode);

        InitBlock();
    }

    private void InitBlock()
    {
        _lstBlock = new List<Type>
        {
            typeof(MapBlock1)
        };
    }

    public List<Type> GetBlocks()
    {
        return _lstBlock;
    }

    /// <summary>
    /// ��ȡ��ǰ��εĵ�ͼ�ڵ��б�
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public List<MapNodeBase> GetCurrentLayerMapNodes()
    {
        return _lstOfLstMapNode[_currentLayerIndex];
    }

    /// <summary>
    /// ���ý���Ľڵ�
    /// </summary>
    /// <param name="enterNode"></param>
    internal void SetEnterNode(MapNodeBase enterNode)
    {
        this.enterNode = enterNode;
    }
}
