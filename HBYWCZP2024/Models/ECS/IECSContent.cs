namespace HBYWCZP2024.Models.ECS
{
    public interface IECSContent
    {
        string Type { get; }
        void Parse(string content);
        string GetContent();
    }
}