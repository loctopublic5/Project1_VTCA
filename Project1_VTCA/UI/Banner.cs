using Spectre.Console;

namespace Project1_VTCA.Utils
{
    public static class Banner
    {
        public static void Show()
        {
            AnsiConsole.Write(
                new FigletText("SNEAKER SHOP")
                    .Centered()
                    .Color(Color.Orange1));

            AnsiConsole.Write(new Rule().Centered());
        }
    }
}