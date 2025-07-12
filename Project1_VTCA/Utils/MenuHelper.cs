using Project1_VTCA.UI.Draw;
using Spectre.Console;
using System;
using System.Collections.Generic;

namespace Project1_VTCA.Utils
{
    public static class MenuHelper
    {
        
        public static string ShowHorizontalMenu(string title, List<string> options)
        {
            int selectedIndex = 0;

            while (true)
            {
                AnsiConsole.Clear();
                Banner.Show();
                AnsiConsole.Write(new Rule($"[bold yellow]{title}[/]").Centered());
                AnsiConsole.WriteLine();

                var grid = new Grid().Centered();
                grid.AddColumn(new GridColumn().NoWrap().PadRight(4));

               
                var panels = new List<Panel>();
                for (int i = 0; i < options.Count; i++)
                {
                    var panel = new Panel(new Markup($"[bold]{Markup.Escape(options[i])}[/]"))
                        .Border(BoxBorder.Rounded);

                    
                    if (i == selectedIndex)
                    {
                        panel.BorderStyle = new Style(Color.Orange1);
                        panel.Header = new PanelHeader(">").SetStyle(new Style(Color.Orange1));
                    }

                    panels.Add(panel);
                }

               
                grid.AddRow(new Columns(panels));
                AnsiConsole.Write(grid);

                AnsiConsole.MarkupLine("\n[dim](Dùng phím Trái/Phải để di chuyển, Enter để chọn)[/]");

              
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        selectedIndex = (selectedIndex == 0) ? options.Count - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.RightArrow:
                        selectedIndex = (selectedIndex == options.Count - 1) ? 0 : selectedIndex + 1;
                        break;
                    case ConsoleKey.Enter:
                        return options[selectedIndex];
                }
            }
        }
    }
}