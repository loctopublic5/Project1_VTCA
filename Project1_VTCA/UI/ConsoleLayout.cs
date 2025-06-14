using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Project1_VTCA.UI
{
    public class ConsoleLayout
    {
        public void Render(IRenderable menuContent, IRenderable viewContent, IRenderable notificationContent)
        {
            var menuPanel = new Panel(menuContent).Header("MENU").Border(BoxBorder.Rounded);
            var viewPanel = new Panel(viewContent).Header("VIEW").Expand().Border(BoxBorder.Rounded);
            var notificationPanel = new Panel(notificationContent).Header("NOTIFICATION").Border(BoxBorder.Rounded);

            var mainGrid = new Grid()
                .AddColumn(new GridColumn().Width(35))
                .AddColumn(new GridColumn());

            var rightColumnGrid = new Grid()
                .AddRow(viewPanel)
                .AddRow(notificationPanel);

            mainGrid.AddRow(menuPanel, rightColumnGrid);

            AnsiConsole.Clear();
            AnsiConsole.Write(mainGrid);
        }
    }
}
