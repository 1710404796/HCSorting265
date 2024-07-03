namespace HBYWCZP2024.Attritubes
{
    public class PowerViewAttribute : Attribute
    {
        public PowerEnum Power { get; }

        public PowerViewAttribute(PowerEnum power)
        {
            Power = power;
        }
    }
}