namespace Lighsing.KingsExecutioner
{
    /// <summary>
    /// 表示回合不同阶段的枚举类。
    /// </summary>
    public enum CombatRoundStage
    {
        BeforeBegin = 0,
        AfterBegin,
        BeforeEnd,
        AfterEnd,
        OnCardPlay = 100,
        OnCardAttack,
        OnCardDefend,
        OnCardDefeated,/*
        OnTargetPlay = 200,
        OnTargetAttack,
        OnTargetDefend,
        OnTargetDefeated,*/
    }
}
