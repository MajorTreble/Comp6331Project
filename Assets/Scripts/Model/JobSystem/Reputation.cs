
namespace Model
{
    [System.Serializable]
    public class Reputation
    {
        public RepType type;
        public int value;

        public Reputation(RepType _type, int _value)
        {
            type = _type;
            value = _value;
        }
        public void ChangeValue(int _value)
        {
            value += _value;
        }
    }
}
