using Spectre.Console;
using Spectre.Console.Rendering;

namespace Project1_VTCA.UI.Draw
{
    public class ConsoleLayout
    {
        public void Render(IRenderable menuContent, IRenderable viewContent, IRenderable notificationContent)
        {
          
            var menuPanel = new Panel(menuContent)
                .Header("MENU")
                .Border(BoxBorder.Rounded);

            var viewPanel = new Panel(viewContent)
                .Header("VIEW")
                .Expand()
                .Border(BoxBorder.Rounded);

            var notificationPanel = new Panel(notificationContent)
                .Header("NOTIFICATION")
                .Border(BoxBorder.Rounded);
            notificationPanel.Height = 3;

           
            var rightColumnGrid = new Grid()
                .AddColumn(); 
                              

            
            rightColumnGrid
                .AddRow(viewPanel)
                .AddRow(notificationPanel);

          
            var mainGrid = new Grid()
                .AddColumn(new GridColumn().Width(35)) 
                .AddColumn(new GridColumn())           
                .AddRow(menuPanel, rightColumnGrid);   



            
            AnsiConsole.Clear();
            AnsiConsole.Write(mainGrid);
        }
        public async Task RenderFormLayoutAsync(string title, Func<Task> formContentAsync)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]{Markup.Escape(title)}[/]").Centered());
            AnsiConsole.WriteLine();

            
            if (formContentAsync != null)
            {
                await formContentAsync();
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule().Centered());
        }
    }
}