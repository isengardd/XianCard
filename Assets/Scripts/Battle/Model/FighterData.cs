﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterData : ObjectBase
{
    public FighterData(int curHp, int maxHp, int armor) : base(ObjectType.PLAYER)
    {
        base.curHp = curHp;
        base.maxHp = maxHp;
        base.armor = armor;
    }

    internal void ReduceHp(int value)
    {
        curHp -= value;
        if (curHp < 0)
            curHp = 0;
    }

    internal void BattleInit()
    {
        lstBuffInst.Clear();
    }
}
