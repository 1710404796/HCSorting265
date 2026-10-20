namespace HCSortingCZP.Service
{
    public interface IHBECSServiceFactory
    {
        HBECSService Create(string name);
    }
}