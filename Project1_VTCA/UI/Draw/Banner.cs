using Spectre.Console;

namespace Project1_VTCA.UI.Draw
{
    public static class Banner
    {
        public static void Show()
        {
            
            var figlet = new FigletText("SNEAKER SHOP")
                .Centered()
                .Color(Color.Orange1); 

            AnsiConsole.Write(figlet);

           
            AnsiConsole.Write(new Rule().Centered().RuleStyle("deepskyblue2"));
        }
    }
}