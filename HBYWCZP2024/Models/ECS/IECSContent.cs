namespace HBYWCZP2024.Models.ECS
{
    public interface IECSContentRes : IECSContent
    {
        string ErrorCode { get; }
    }
    public interface IECSContent
    {
        string Type { get; }
        void Parse(string content);
        string GetContent();
    }
}