namespace HCSortingCZP.Attritubes
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