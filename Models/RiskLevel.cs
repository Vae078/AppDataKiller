namespace AppDataCleaner.Models
{
    public enum RiskLevel
    {
        Safe,       // 明确的缓存文件，安全删除
        Caution,    // 可能包含配置，谨慎处理
        Danger      // 高风险，不建议删除
    }
}
