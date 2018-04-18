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
    public const int NODE_NUM = 50; //ÿ����ͼ5���ڵ� 10�������ͼ
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
        InitMapNode();
        InitBlock();
    }

    //��ʼ����ͼ�ڵ�
    private void InitMapNode()
    {
        List<MapNodeType> lstNodeType = new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY };    //��һ���ڵ�����ͨ��
        lstNodeType.AddRange(GetRandomNodeTypeList(4, new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY, MapNodeType.ADVANTURE }));  //���ɵ�2-5���ڵ�

        //���ɵ�6-50���ڵ� todo ��������
        List<MapNodeType> lstAfterNodeType = GetRandomNodeTypeList(NODE_NUM - 5,
            new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY, MapNodeType.ADVANTURE, MapNodeType.BOX, MapNodeType.ELITE, MapNodeType.SHOP });
        lstNodeType.AddRange(lstAfterNodeType);

        List<MapNodeBase> lstMapNode = new List<MapNodeBase>();
        //todo ���벻ͬ�Ľڵ�����
        for (int i = 0; i < NODE_NUM; i++)
        {
            //todo ����ڵ�����
            MapNodeType nodeType = lstNodeType[i];
            lstMapNode.Add(GetMapNode(nodeType));
            //lstMapNode.Add(new NormalEnemyNode(1/*, 700, BOSS_HEIGHT + i * SINGLE_HEIGHT + UnityEngine.Random.Range(0, SINGLE_HEIGHT)*/));   //todo �������ID
        }
        _lstOfLstMapNode.Add(lstMapNode);
    }

    //��ȡ����ڵ�
    private List<MapNodeType> GetRandomNodeTypeList(int nodeNum, List<MapNodeType> lstAvailableType)
    {
        List<MapNodeType> lstNode = new List<MapNodeType>();
        for (int i = 0; i < nodeNum; i++)
        {
            lstNode.Add(GetRandomNodeType(lstAvailableType));
        }
        return lstNode;
    }

    //���һ������ڵ�����
    private MapNodeType GetRandomNodeType(List<MapNodeType> lstNodeType)
    {
        int index = UnityEngine.Random.Range(0, lstNodeType.Count);
        return lstNodeType[index];
    }

    //��ȡ��ͼ�ڵ�
    private MapNodeBase GetMapNode(MapNodeType nodeType)
    {
        switch (nodeType)
        {
            case MapNodeType.NORMAL_ENEMY:
                return new NormalEnemyNode(1);  //todo �����ͨ��ID
            case MapNodeType.ELITE:
                return new EliteNode(1);  //todo �����Ӣ��ID
            case MapNodeType.ADVANTURE:
                return new AdvantureNode();
            case MapNodeType.SHOP:
                return new ShopNode();
            case MapNodeType.BOX:
                return new BoxNode();
            default:
                Debug.LogError("unhandle map node:" + nodeType);
                break;
        }
        return null;
    }

    //��ʼ����ͼ�б�
    private void InitBlock()
    {
        _lstBlock = new List<Type>
        {
            typeof(MapBlock1),typeof(MapBlock2)
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
