namespace Nacos.V2.Naming.Core
{
    using Nacos.V2.Naming.Utils;
    using System;
    using System.Collections.Generic;

    public class Chooser<T, TV>
    {
        private T uniqueKey;

        private Ref<TV> _ref;


        public TV Random()
        {
            List<TV> items = _ref.Items;
            if (items.Count == 0) return default;

            if (items.Count == 1) return items[0];

            return items[new Random().Next(items.Count)];
        }

        public TV RandomWithWeight()
        {
            Ref<TV> @ref = this._ref;
            double random = new Random().NextDouble();

            int index = Array.BinarySearch(@ref.Weights, random);
            if (index < 0)
            {
                index = -index - 1;
            }
            else
            {
                return @ref.Items[index];
            }

            if (index >= 0 && index < @ref.Weights.Length)
            {
                if (random < @ref.Weights[index])
                {
                    return @ref.Items[index];
                }
            }

            /* This should never happen, but it ensures we will return a correct
             * object in case there is some floating point inequality problem
             * wrt the cumulative probabilities. */
            return @ref.Items[@ref.Items.Count - 1];
        }

        public Chooser(T uniqueKey)
            : this(uniqueKey, new List<Pair<TV>>())
        {
        }

        public Chooser(T uniqueKey, List<Pair<TV>> pairs)
        {
            Ref<TV> @ref = new Ref<TV>(pairs);
            @ref.Refresh();
            this.uniqueKey = uniqueKey;
            this._ref = @ref;
        }

        public T GetUniqueKey()
        {
            return uniqueKey;
        }

        public Ref<TV> GetRef()
        {
            return _ref;
        }

        public void Refresh(List<Pair<TV>> itemsWithWeight)
        {
            Ref<TV> newRef = new Ref<TV>(itemsWithWeight);
            newRef.Refresh();
            newRef.Poller = this._ref.Poller.Refresh(newRef.Items);
            this._ref = newRef;
        }

        public override int GetHashCode()
        {
            return uniqueKey.GetHashCode();
        }
    }
}
