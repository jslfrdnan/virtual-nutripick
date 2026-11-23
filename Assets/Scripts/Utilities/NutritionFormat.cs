namespace NutriPick.Utilities
{
    public static class NutritionFormat
    {
        public static string Grams(float value)
        {
            return value.ToString("0.#").Replace('.', ',') + " g";
        }

        public static string Mg(float value)
        {
            return value.ToString("0").Replace('.', ',') + " mg";
        }

        public static string Kcal(int value)
        {
            return value + " kkal";
        }

        public static string Score(int value)
        {
            return value.ToString();
        }
    }
}
