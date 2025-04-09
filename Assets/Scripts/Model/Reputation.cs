
namespace Model
{

    [System.Serializable]
    public class Reputation
    {
        public Faction fac;
        //public RepType type;
        public int value;

        public Reputation(Faction _fac, int _value)
        {
            fac = _fac;
            value = _value;
        }
        public void ChangeValue(int _value)
        {
            value += _value;
        }
    }

}
