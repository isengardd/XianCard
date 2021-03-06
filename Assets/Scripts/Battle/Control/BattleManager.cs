﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : IDisposable
{
    //model
    private BattleModel _battleModel;

    //control
    private Dictionary<int, IEnemy> _dicEnemyAction = new Dictionary<int, IEnemy>();    //敌人行动管理

    public BattleManager()
    {
        _battleModel = BattleModel.Inst;
        InitControl();
    }

    public void Dispose()
    {
        ReleaseControl();
    }

    private void InitControl()
    {
        _battleModel.AddListener(BattleEvent.ENEMY_INIT, OnEnemyInit);
        _battleModel.AddListener(BattleEvent.BOUT_UPDATE, OnBoutUpdate);
        _battleModel.AddListener(BattleEvent.ENEMY_DEAD, OnEnemyDead);

        Message.AddListener(MsgType.ATTACK_HIT, OnAttackHit);
    }

    private void ReleaseControl()
    {
        if (_dicEnemyAction != null)
        {
            _dicEnemyAction.Clear();
            _dicEnemyAction = null;
        }

        _battleModel.RemoveListener(BattleEvent.ENEMY_INIT, OnEnemyInit);
        _battleModel.RemoveListener(BattleEvent.BOUT_UPDATE, OnBoutUpdate);
        _battleModel.RemoveListener(BattleEvent.ENEMY_DEAD, OnEnemyDead);

        Message.RemoveListener(MsgType.ATTACK_HIT, OnAttackHit);
    }

    //自己抽牌
    public void SelfDrawCard(int drawNum, bool bRoundStarDraw)
    {
        if (_battleModel.selfData.HasBuff(BuffType.CAN_NOT_DRAW_CARD))
            return;
        Core.Inst.StartCoroutine(DrawCard(drawNum, bRoundStarDraw));
    }

    //抽多张牌
    private IEnumerator DrawCard(int drawNum, bool bRoundStartDraw)
    {
        for (int i = 0; i < drawNum; i++)
        {
            if (!BattleTool.IsDeckHasCard()) //如果没卡了
            {
                if  (!BattleTool.IsUsedHasCard())
                {
                    yield break;
                }

                float shuffleTime = BattleTool.ShuffleDeckFromUsed();
                yield return new WaitForSeconds(shuffleTime);
            }
            _battleModel.DrawOneCard(bRoundStartDraw);
            yield return new WaitForSeconds(0.2f);
        }
    }

    //抽牌直到抽到一张非攻击牌
    public void SelfDrawCardUntilNotAttack()
    {
        if (_battleModel.selfData.HasBuff(BuffType.CAN_NOT_DRAW_CARD))
            return;

        Core.Inst.StartCoroutine(DrawCardUntilNotAttack());
    }

    private IEnumerator DrawCardUntilNotAttack()
    {
        CardInstance drawCardInst = null;
        while(true)
        {
            if (!BattleTool.IsDeckHasCard()) //如果没卡了
            {
                if (!BattleTool.IsUsedHasCard())
                {
                    yield break;
                }

                float shuffleTime = BattleTool.ShuffleDeckFromUsed();
                yield return new WaitForSeconds(shuffleTime);
            }

            if (BattleTool.IsHandCardFull())// 如果手牌满了
            {
                yield break;
            }

            drawCardInst = _battleModel.DrawOneCard(false);
            if (null == drawCardInst || drawCardInst.cardType != CardType.ATTACK)
            {
                yield break;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnEnemyInit(object obj)
    {
        Dictionary<int, EnemyInstance> enemys = _battleModel.GetEnemys();
        foreach (var kv in enemys)
        {
            _dicEnemyAction.Add(kv.Key, EnemyActionReg.GetAction(kv.Value.template.nId));
        }
    }

    private void OnBoutUpdate(object obj)
    {
        switch (_battleModel.bout)
        {
            case Bout.SELF:
                SelfBoutStartHandle();
                break;
            case Bout.ENEMY:
                Core.Inst.StartCoroutine(SelfBoutEndHandle());
                Core.Inst.StartCoroutine(EnemyBoutHandle());
                break;
            default:
                Debug.LogError("unhandle bout:" + _battleModel.bout);
                break;
        }
    }

    private void SelfBoutStartHandle()
    {
        SelfDrawCard(5, true);
        _battleModel.RecoverCost();
        if (!_battleModel.selfData.HasBuff(BuffType.KEEP_ARMOR))
            _battleModel.UpdateArmor(_battleModel.selfData, 0);
        UpdateEnemyAction();
        _battleModel.InitRoundStatistics();

        SettleBuffOnBoutStart(_battleModel.selfData);
    }

    //自身回合结束处理
    private IEnumerator SelfBoutEndHandle()
    {
        FighterData boutEndFighterData = (FighterData)_battleModel.selfData.Clone();
        Message.Send(MsgType.SELF_BOUT_END, boutEndFighterData);

        //虚无卡牌，在回合结束时消耗
        var handListCopy = new List<CardInstance>(_battleModel.GetHandList());
        foreach (var handCard in handListCopy)
        {
            CardTemplate cardTpl = CardTemplateData.GetData(handCard.tplId);
            if (cardTpl == null)
                continue;

            if (cardTpl.bEthereal)
            {
                _battleModel.ExhaustHandCard(handCard);
            }
        }

        //回合结束buff结算
        SettleBuffOnBoutEnd(_battleModel.selfData);

        //手牌飞入弃牌堆
        handListCopy = new List<CardInstance>(_battleModel.GetHandList());
        foreach (var handCard in handListCopy)
        {
            _battleModel.MoveHandCardToUsed(handCard);
        }
        yield return new WaitForSeconds(AnimationTime.HAND_TO_USED);
    }

    private void SettleBuffOnBoutStart(ObjectBase targetObj)
    {
        List<BuffInst> lstBuffInst = new List<BuffInst>(targetObj.lstBuffInst);
        foreach (var buffInst in lstBuffInst)
        {
            BuffTemplate template = BuffTemplateData.GetData(buffInst.tplId);
            if (template == null)
                continue;

            if (template.nType == BuffType.GET_BUFF_ROUND_START)
            {
                _battleModel.AddBuff(targetObj, targetObj, (uint)template.iEffectB, buffInst.effectVal);
            }

            if (buffInst.leftBout != -1)
            {
                //Debug.LogError("DecBuffLeftBout" + buffInst.leftBout);
                if (template.nTrigger == BuffTriggerType.BOUT_START)
                {
                    _battleModel.DecBuffLeftBout(targetObj, buffInst, 1);
                }
            }
        }
    }

    private void SettleBuffOnBoutEnd(ObjectBase targetObj)
    {
        List<BuffInst> lstBuffInst = new List<BuffInst>(targetObj.lstBuffInst);
        foreach (var buffInst in lstBuffInst)
        {
            BuffTemplate template = BuffTemplateData.GetData(buffInst.tplId);
            if (template == null)
                continue;
            switch (template.nType)
            {
                case BuffType.ADD_ARMOR_ROUND_END:
                case BuffType.MULTI_ARMOR:
                    _battleModel.AddArmor(targetObj, buffInst.effectVal);
                    break;
                default:
                    //Debug.LogError("unhandle bout end buff:" + template.nType);
                    break;
            }
        }

        foreach (var buffInst in lstBuffInst)
        {
            BuffTemplate template = BuffTemplateData.GetData(buffInst.tplId);
            if (template == null)
                continue;

            if (buffInst.selfAddDebuffThisBout)
            {
                buffInst.selfAddDebuffThisBout = false;
                //Debug.LogError("buffId：" + buffInst.tplId + " selfAddDebuffThisBout");
            }
            else if (buffInst.leftBout != -1)
            {
                //Debug.LogError("DecBuffLeftBout" + buffInst.leftBout);
                if (template.nTrigger != BuffTriggerType.BOUT_START)
                {
                    _battleModel.DecBuffLeftBout(targetObj, buffInst, 1);
                }
            }
        }
    }

    //更新敌人下回合行为
    private void UpdateEnemyAction()
    {
        foreach (var kv in _dicEnemyAction)
        {
            _battleModel.SetAction(kv.Key, kv.Value.GetNextBoutAction());
        }
    }

    private void OnEnemyDead(object obj)
    {
        int instId = (int)obj;
        _dicEnemyAction.Remove(instId);

        CheckIsBattleEnd();
    }

    //检查战斗是否结束
    private void CheckIsBattleEnd()
    {
        if (BattleTool.IsAllEnemyDead())
            Message.Send(MsgType.BATTLE_WIN);
    }

    //敌方回合处理
    private IEnumerator EnemyBoutHandle()
    {
        //回合标题展示时间
        yield return new WaitForSeconds(AnimationTime.BOUT_DISPLAY_TIME);

        //todo 结算buff效果

        //清空护甲
        foreach (var kv in _battleModel.GetEnemys())
        {
            if (!kv.Value.HasBuff(BuffType.KEEP_ARMOR))
                _battleModel.UpdateArmor(kv.Value, 0);
        }

        //按顺序执行回合动作
        foreach (var kv in _battleModel.GetEnemys())
        {
            BoutAction boutAction = kv.Value.boutAction;
            if (boutAction == null)
                continue;
            float actionTime = HandleBoutAction(kv.Value, boutAction);
            yield return new WaitForSeconds(actionTime);
        }

        //回合结束结算BUFF
        foreach (var kv in _battleModel.GetEnemys())
        {
            SettleBuffOnBoutEnd(kv.Value);
        }

        //都执行完了统一等待
        yield return new WaitForSeconds(AnimationTime.BOUT_END_TIME);

        _battleModel.SetBout(Bout.SELF);
    }

    //处理某个敌人的行动
    private float HandleBoutAction(EnemyInstance enemyInst, BoutAction boutAction)
    {
        switch (boutAction.enemyAction)
        {
            case EnemyAction.ATTACK:
                bool isBlock = _battleModel.selfData.armor >= boutAction.iValue;
                Message.Send(MsgType.DO_ATTACK, new AttackStruct(enemyInst, boutAction, isBlock));
                return AnimationTime.ATTACK_TIME;
            default:
                Debug.LogError("unhandle enemy action:" + boutAction.enemyAction);
                return 0.5f;    //容错用时间
        }
    }

    private void OnAttackHit(object obj)
    {
        //结算对自身的伤害
        AttackStruct attackStruct = obj as AttackStruct;
        attackStruct.boutAction.iValue = BattleTool.AdjustAttackVal(attackStruct.casterInst, _battleModel.selfData, attackStruct.boutAction.iValue);

        int orignArmor = _battleModel.selfData.armor;
        int leftArmor = orignArmor - attackStruct.boutAction.iValue;
        int reduceArmor = 0;
        int reeduceHp = 0;
        if (leftArmor < 0)
        {
            _battleModel.UpdateArmor(_battleModel.selfData, 0);
            _battleModel.ReduceSelfHp(-leftArmor);
            reduceArmor = orignArmor;
            reeduceHp = -leftArmor;
        }
        else  //如果护甲有剩余
        {
            _battleModel.UpdateArmor(_battleModel.selfData, leftArmor);
            reduceArmor = attackStruct.boutAction.iValue;
        }
        SoundTool.inst.PlaySoundEffect(ResPath.SFX_SPEAR);  //todo 根据模板表和是否格挡来播放音效
        Message.Send(MsgType.SHOW_HIT_EFFECT, attackStruct);

        OnObjectHitted(attackStruct.casterInst, _battleModel.selfData, reeduceHp, reduceArmor);
    }

    //结算受击效果
    private void OnObjectHitted(ObjectBase caster, ObjectBase target, int reduceHp, int reduceArmor)
    {
        List<BuffInst> lstBuffInst = new List<BuffInst>(target.lstBuffInst);
        foreach (var buffInst in lstBuffInst)
        {
            BuffTemplate template = BuffTemplateData.GetData(buffInst.tplId);
            if (template == null)
                continue;
            switch (template.nType)
            {
                case BuffType.ARMOR_REFLECT:
                    if (0 == reduceArmor)
                        continue;

                    int reflectValue = (reduceArmor * buffInst.effectVal) / 100;
                    _battleModel.ReduceEnemyHp(caster.instId, reflectValue);
                    break;
                case BuffType.MULTI_ARMOR:
                    if (reduceHp > 0)
                    {
                        //受伤少一层护甲
                        _battleModel.DecBuffEffectVal(target, buffInst, 1);
                    }
                    break;
                default:
                    //Debug.LogError("unhandle on hit buff type:" + template.nType);
                    break;
            }
        }
    }

    //使用技能卡
    internal void UseSkillCard(CardInstance cardInstance, CardTemplate template, int targetInstId = 0)
    {
        _battleModel.effectStat = new EffectStatistics();

        Debug.Log("card used:" + cardInstance.tplId);
        int iCost = template.iCost;
        if (-1 == iCost)
        {
            iCost = _battleModel.curCost;
        }

        _battleModel.ReduceCost(iCost);
        _battleModel.effectStat.consumeCost += (uint)iCost;

        HandleCardEffect(cardInstance, template.nEffectId, targetInstId);

        _battleModel.roundStat.lstUsedCard.Add(cardInstance.Clone());
        _battleModel.battleStat.useCardCount += 1;

        //处理卡牌去向
        if (template.bExhaust)
        {
            //消耗
            _battleModel.ExhaustHandCard(cardInstance);
        }
        else
        {
            switch (template.nType)
            {
                case CardType.ATTACK:
                case CardType.SKILL:
                    _battleModel.MoveHandCardToUsed(cardInstance);
                    break;
                case CardType.FORMATION:
                    _battleModel.ConsumeHandCard(cardInstance);
                    break;
                default:
                    Debug.LogError("unhandle card type:" + template.nType);
                    break;
            }
        }
    }

    //处理卡牌效果
    private void HandleCardEffect(CardInstance cardInstance, uint effectId, int targetInstId = 0)
    {
        CardEffectTemplate effectTemplate = CardEffectTemplateData.GetData(effectId);
        if (effectTemplate == null)
            return;

        CardEffectBase cardEffect = CardEffectFactory.GetCardEffect(effectTemplate.nType);
        if (cardEffect == null)
            return;

        if (cardEffect.CanTriggerCardEffect(effectTemplate))
        {
            cardEffect.DoEffect(this, cardInstance, effectTemplate, targetInstId);
        }

        if (effectTemplate.nLinkId != 0)
            HandleCardEffect(cardInstance, effectTemplate.nLinkId, targetInstId);
    }

    public IEnumerator DamageEnemyCoroutine(int iDmgCount, int targetInstId, CardEffectTemplate effectTemplate)
    {
        int iEffectVal = effectTemplate.iEffectValue;
        if (effectTemplate.nType == CardEffectType.ARMOR_DAMAGE)
        {
            iEffectVal = _battleModel.selfData.armor; //当前护甲值
        }

        bool bIgnoreArmor = (effectTemplate.nType == CardEffectType.ONE_DAMAGE_IGNORE_ARMOR);
        for (int i = 0; i < iDmgCount; ++i)
        {
            DamageEnemy(targetInstId, iEffectVal, bIgnoreArmor);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator DamageAllEnemyCoroutine(int iDmgCount, CardEffectTemplate effectTemplate)
    {
        int iEffectVal = effectTemplate.iEffectValue;
        if (effectTemplate.nType == CardEffectType.ARMOR_DAMAGE)
        {
            iEffectVal = _battleModel.selfData.armor; //当前护甲值
        }

        bool bIgnoreArmor = (effectTemplate.nType == CardEffectType.ONE_DAMAGE_IGNORE_ARMOR);
        for (int i = 0; i < iDmgCount; ++i)
        {
            foreach (KeyValuePair<int, EnemyInstance> pair in _battleModel.GetEnemys())
            {
                DamageEnemy(pair.Value.instId, iEffectVal, bIgnoreArmor);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    //检测能否触发卡牌效果
    internal bool CanTriggerCardEffect(uint effectId, int targetInstId)
    {
        CardEffectTemplate effectTemplate = CardEffectTemplateData.GetData(effectId);
        if (effectTemplate == null)
            return false;

        switch (effectTemplate.iEffectTrigType)
        {
            case CardEffectTrigType.NONE:
                break;
            case CardEffectTrigType.GIVE_NOT_BLOCK_DAMAGE:
                if (_battleModel.effectStat.damageLife > 0)
                    return true;
                return false;
            case CardEffectTrigType.SELF_HAS_BUFF_TYPE:
                return _battleModel.selfData.HasBuff(effectTemplate.iTrigVal);
            default:
                Debug.LogError("unhandle card EffectTrigType:" + effectTemplate.iEffectTrigType);
                break;
        }

        return true;
    }

    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    /// <param name="instId"></param>
    /// <param name="iEffectValue"></param>
    internal void DamageEnemy(int instId, int iEffectValue, bool ignoreArmor)
    {
        EnemyInstance enemyInstance = _battleModel.GetEnemy(instId);
        if (enemyInstance == null)
            return;

        iEffectValue = BattleTool.AdjustAttackVal(_battleModel.selfData, enemyInstance, iEffectValue);

        if (false == ignoreArmor)
        {
            if (enemyInstance.armor >= iEffectValue)
            {
                _battleModel.ReduceEnemyArmor(instId, iEffectValue);

                _battleModel.effectStat.damageArmor += (uint)iEffectValue;
                _battleModel.roundStat.damageArmor += (uint)iEffectValue;
            }
            else
            {
                int iReduceHp = iEffectValue - enemyInstance.armor;
                int iReduceArmor = enemyInstance.armor;
                _battleModel.effectStat.damageArmor += (uint)iReduceArmor;
                _battleModel.roundStat.damageArmor += (uint)iReduceArmor;
                _battleModel.ReduceEnemyArmor(instId, enemyInstance.armor);

                _battleModel.ReduceEnemyHp(instId, iReduceHp);
                OnObjectHitted(_battleModel.selfData, enemyInstance, iReduceHp, iReduceArmor);
            }
        }
        else
        {
            _battleModel.ReduceEnemyHp(instId, iEffectValue);
            OnObjectHitted(_battleModel.selfData, enemyInstance, iEffectValue, 0);
        }
        SoundTool.inst.PlaySoundEffect(ResPath.SFX_SPEAR);  //todo 根据模板表 以及是否格挡 播放不同的音效
    }
}