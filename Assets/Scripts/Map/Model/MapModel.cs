using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    private const int BLOCK_NUM = 10; //10�������ͼ
    private const int STEP_PER_BLOCK = 5; //ÿ�ŵ�ͼ�������ߵĲ���
    private const int SINGLE_HEIGHT = 120; //��������ռ�ݵĸ߶�
    private const int BOSS_HEIGHT = 500; //BOSSռ�ݵĸ߶�

    public MapNodeBase enterNode { get; private set; }  //��ǰ����Ľڵ�

    //private List<List<MapNodeBase>> _lstOfLstMapNode = new List<List<MapNodeBase>>();
    private int _currentLayerIndex = 0; //��ǰ��ͼ��ε�����

    private List<Type> _lstBlockType;
    private List<GComponent> _lstBlockCom;
    public List<List<int>> lstOfLstBlockRoad = new List<List<int>>();   //��ͼ���·���б�
    private Dictionary<string, MapNodeBase> _dicMapNode = new Dictionary<string, MapNodeBase>();
    private int _leftBoxNum;    //ʣ������ɽڵ������
    private int _leftShopNum;
    private int _leftElitNum;
    private int _lastElitStep;  //���һ�����ɽڵ��λ��
    private int _lastShopStep;
    private int _lastBoxStep;

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Init()
    {
        InitBlock();
        //InitMapNode();
    }

    //��ʼ����ͼ�б�
    private void InitBlock()
    {
        //init model
        _lstBlockType = new List<Type>
        {
            typeof(MapBlock1),typeof(MapBlock2)
        };
        _leftBoxNum = 6;
        _leftShopNum = 6;
        _leftElitNum = 12;
        lstOfLstBlockRoad.Clear();

        //���ɾ�������
        _lstBlockCom = new List<GComponent>();
        for (int i = 0; i < BLOCK_NUM; i++)
        {
            //����block���
            int index = UnityEngine.Random.Range(0, _lstBlockType.Count);
            Type typeBlock = _lstBlockType[index];
            var method = typeBlock.GetMethod("CreateInstance", BindingFlags.Public | BindingFlags.Static);
            GComponent mapBlock = method.Invoke(null, null) as GComponent;
            _lstBlockCom.Add(mapBlock);

            //���ɿ����ߵĽڵ������
            string data = mapBlock.packageItem.componentData.GetAttribute("customData");
            string[] lstRoad = data.Split('\r');
            foreach (string strRoad in lstRoad)
            {
                string[] lstPointIndex = strRoad.Split(',');
                int roadCount = lstPointIndex.Length;
                List<int> lstBlockRoad = new List<int>();
                for (int blockStep = 0; blockStep < roadCount; blockStep++)
                {
                    string strPointIndex = lstPointIndex[blockStep];
                    string nodeKey = GetNodeKey(i, strPointIndex);
                    if (_dicMapNode.ContainsKey(nodeKey))
                        continue;
                    int step = i * STEP_PER_BLOCK + blockStep + 1;
                    int nodeIndex;
                    int.TryParse(strPointIndex, out nodeIndex);
                    lstBlockRoad.Add(nodeIndex);
                    MapNodeBase nodeBase = GetRandomNode(step, nodeIndex);
                    _dicMapNode.Add(nodeKey, nodeBase);
                }
                lstOfLstBlockRoad.Add(lstBlockRoad);
            }
        }
    }

    /// <summary>
    /// ��ȡ��ǰ���ͼ�Ľڵ�����
    /// </summary>
    /// <param name="pointIndex"></param>
    /// <returns></returns>
    public MapNodeBase GetCurrentLayerMapNodeData(string pointIndex)
    {
        string nodeKey = GetNodeKey(_currentLayerIndex, pointIndex);
        if (_dicMapNode.ContainsKey(nodeKey))
            return _dicMapNode[nodeKey];
        return null;
    }

    private string GetNodeKey(int layerIndex, string pointIndex)
    {
        return string.Format("{0}_{1}", layerIndex, pointIndex);
    }

    //���ݲ���������ɽڵ�
    private MapNodeBase GetRandomNode(int step, int nodeIndex)
    {
        if (step == 1)  //��һ���ڵ�����ͨ��
            return GetMapNode(MapNodeType.NORMAL_ENEMY, nodeIndex);

        List<MapNodeType> lstNodeType;
        if (step <= 5)  //ǰ5���ڵ�ֻ������ͨ�ֻ�������
        {
            lstNodeType = new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY, MapNodeType.ADVANTURE };
        }
        else
        {
            lstNodeType = new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY, MapNodeType.ADVANTURE };
            TryAddNodeType(lstNodeType, MapNodeType.BOX, step);
            TryAddNodeType(lstNodeType, MapNodeType.ELITE, step);
            TryAddNodeType(lstNodeType, MapNodeType.SHOP, step);
        }

        MapNodeType nodeType = GetRandomNodeType(lstNodeType);
        return GetMapNode(nodeType, nodeIndex);
    }

    //���Լ���ڵ�����
    private void TryAddNodeType(List<MapNodeType> lstNodeType, MapNodeType nodeType, int step)
    {
        switch (nodeType)
        {
            case MapNodeType.NONE:
            case MapNodeType.NORMAL_ENEMY:
            case MapNodeType.ADVANTURE:
                //do nothing
                break;
            case MapNodeType.ELITE:
                if (_leftElitNum <= 0 && step - _lastElitStep >= 4)
                    return;
                --_leftElitNum;
                _lastElitStep = step;
                lstNodeType.Add(MapNodeType.ELITE);
                break;
            case MapNodeType.SHOP:
                if (_leftShopNum <= 0 && step - _lastShopStep >= 7)
                    return;
                --_leftShopNum;
                _lastShopStep = step;
                lstNodeType.Add(MapNodeType.SHOP);
                break;
            case MapNodeType.BOX:
                if (_leftBoxNum <= 0 && step - _lastBoxStep >= 7)
                    return;
                --_leftBoxNum;
                _lastBoxStep = step;
                lstNodeType.Add(MapNodeType.BOX);
                break;
            default:
                Debug.LogError("unhandle add node type:" + nodeType);
                break;
        }
    }

    /// <summary>
    /// ��ȡ��ͼ�����
    /// </summary>
    /// <returns></returns>
    public GComponent GetCurrentBlockCom()
    {
        return _lstBlockCom[_currentLayerIndex];
    }

    //��ʼ����ͼ�ڵ�
    //private void InitMapNode()
    //{
    //    List<MapNodeType> lstNodeType = new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY };    //��һ���ڵ�����ͨ��
    //    lstNodeType.AddRange(GetRandomNodeTypeList(4, new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY, MapNodeType.ADVANTURE }));  //���ɵ�2-5���ڵ�

    //    //���ɵ�6-50���ڵ� todo ��������
    //    List<MapNodeType> lstAfterNodeType = GetRandomNodeTypeList(NODE_NUM - 5,
    //        new List<MapNodeType>() { MapNodeType.NORMAL_ENEMY, MapNodeType.ADVANTURE, MapNodeType.BOX, MapNodeType.ELITE, MapNodeType.SHOP });
    //    lstNodeType.AddRange(lstAfterNodeType);

    //    List<MapNodeBase> lstMapNode = new List<MapNodeBase>();
    //    //todo ���벻ͬ�Ľڵ�����
    //    for (int i = 0; i < NODE_NUM; i++)
    //    {
    //        //todo ����ڵ�����
    //        MapNodeType nodeType = lstNodeType[i];
    //        lstMapNode.Add(GetMapNode(nodeType));
    //        //lstMapNode.Add(new NormalEnemyNode(1/*, 700, BOSS_HEIGHT + i * SINGLE_HEIGHT + UnityEngine.Random.Range(0, SINGLE_HEIGHT)*/));   //todo �������ID
    //    }
    //    _lstOfLstMapNode.Add(lstMapNode);
    //}

    //��ȡ����ڵ�
    //private List<MapNodeType> GetRandomNodeTypeList(int nodeNum, List<MapNodeType> lstAvailableType)
    //{
    //    List<MapNodeType> lstNode = new List<MapNodeType>();
    //    for (int i = 0; i < nodeNum; i++)
    //    {
    //        lstNode.Add(GetRandomNodeType(lstAvailableType));
    //    }
    //    return lstNode;
    //}

    //���һ������ڵ�����
    private MapNodeType GetRandomNodeType(List<MapNodeType> lstNodeType)
    {
        int index = UnityEngine.Random.Range(0, lstNodeType.Count);
        return lstNodeType[index];
    }

    //��ȡ��ͼ�ڵ�
    private MapNodeBase GetMapNode(MapNodeType nodeType, int nodeIndex)
    {
        switch (nodeType)
        {
            case MapNodeType.NORMAL_ENEMY:
                return new NormalEnemyNode(nodeIndex, 1);  //todo �����ͨ��ID
            case MapNodeType.ELITE:
                return new EliteNode(nodeIndex, 1);  //todo �����Ӣ��ID
            case MapNodeType.ADVANTURE:
                return new AdvantureNode(nodeIndex);
            case MapNodeType.SHOP:
                return new ShopNode(nodeIndex);
            case MapNodeType.BOX:
                return new BoxNode(nodeIndex);
            default:
                Debug.LogError("unhandle map node:" + nodeType);
                break;
        }
        return null;
    }

    /// <summary>
    /// ���ý���Ľڵ�
    /// </summary>
    /// <param name="enterNode"></param>
    internal void SetEnterNode(MapNodeBase enterNode)
    {
        this.enterNode = enterNode;
        SendEvent(MapEvent.ENTER_NODE_UPDATE);
    }
}
