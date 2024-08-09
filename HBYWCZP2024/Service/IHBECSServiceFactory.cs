namespace HBYWCZP2024.Service
{
    public interface IHBECSServiceFactory
    {
        HBECSService Create(string name);
    }
}