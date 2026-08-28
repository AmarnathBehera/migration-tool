using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Helpers
{
    public static class EnumerableHelper
    {
        public static async Task<TOutput[]> TransformGroupedElementsMaintainingOrderAsync<TInput, TKey, TOutput>(IEnumerable<TInput> inputList, Func<TInput, TKey> keySelector, Func<TKey, IEnumerable<TInput>, Task<IEnumerable<TOutput>>> asyncBatchModifier)
        {
            var evaluatedList = inputList.ToList();

            var groupedItems = evaluatedList
                .Select((value, index) => (Index: index, Value: value))
                .GroupBy(x => keySelector(x.Value));

            var outputs = new TOutput[evaluatedList.Count];

            foreach (var group in groupedItems)
            {
                var key = group.Key;
                var batchItems = group.Select(x => x.Value);
                var modifiedBatch = (await asyncBatchModifier(key, batchItems)).ToList();

                for (var i = 0; i < modifiedBatch.Count; i++)
                {
                    var indexedItem = group.ElementAt(i);
                    outputs[indexedItem.Index] = modifiedBatch[i];
                }
            }

            return outputs;
        }
    }

}
