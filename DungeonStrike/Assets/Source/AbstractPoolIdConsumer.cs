using UnityEngine;

namespace DungeonStrike
{
    public abstract class AbstractPoolIdConsumer : MonoBehaviour, IPoolIdConsumer
    {
        // Cannot be an auto-generated property because Unity doesn't serialize them.
        // Cannot be 'int?' because Unity doesn't serialize those either.
        [HideInInspector] [SerializeField] private int _poolId;

        public int PoolId
        {
            get
            {
                Preconditions.CheckArgument(_poolId != 0, "You must specify a PoolId.");
                return _poolId;
            }
            set
            {
                Preconditions.CheckArgument(value != 0, "PoolId cannot be 0.");
                _poolId = value;
            }
        }

        public FastPool Pool
        {
            get { return FastPoolManager.GetPool(PoolId, null); }
        }
    }
}