namespace HBYWCZP2024.Models.ECS
{
    public interface IECSContent
    {
        void Parse(string content);
        string GetContent();
    }
}