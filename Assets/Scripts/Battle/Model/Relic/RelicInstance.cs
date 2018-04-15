using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 法宝实例
/// </summary>
public class RelicHuZangHuFu : RelicBase
{
    public RelicHuZangHuFu() : base(RelicId.HUZANG_HUFU)
    {
    }

    public override void OnPutIntoRelicList()
    {
        Message.AddListener(MsgType.SELF_BOUT_END, OnCharBoutEnd);
        return;
    }

    public override void OnDisable()
    {
        Message.RemoveListener(MsgType.SELF_BOUT_END, OnCharBoutEnd);
        return;
    }

    public override void OnCharBoutEnd(object obj)
    {
        return;
    }
}

public class RelicWuHuoQiQinShan : RelicBase
{
    public RelicWuHuoQiQinShan() : base(RelicId.WUHUOQIQIN_SHAN)
    {
    }
}

public class RelicWuJinHu : RelicBase
{
    public RelicWuJinHu() : base(RelicId.WUJIN_HU)
    {
    }
}

public class RelicXueJingShi : RelicBase
{
    public RelicXueJingShi() : base(RelicId.XUEJING_SHI)
    {
    }
}