using System;

namespace ExtremeRoles.Module.Interface
{
    public interface ISpawnDataManager
    {
        protected static int ComputeSpawnNum(
            OptionHolder.CommonOptionKey minSpawnKey,
            OptionHolder.CommonOptionKey maxSpawnKey)
        {
            var allOption = OptionHolder.AllOption;

            int minSpawnNum = allOption[(int)minSpawnKey].GetValue();
            int maxSpawnNum = allOption[(int)maxSpawnKey].GetValue();

            // 最大値が最小値より小さくならないように
            maxSpawnNum = Math.Clamp(maxSpawnNum, minSpawnNum, int.MaxValue);

            return RandomGenerator.Instance.Next(minSpawnNum, maxSpawnNum + 1);
        }

        protected static int ComputePercentage(IOption self)
            => (int)decimal.Multiply(self.GetValue(), self.ValueCount);
    }
}
