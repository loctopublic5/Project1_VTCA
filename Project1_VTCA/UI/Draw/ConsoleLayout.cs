using Spectre.Console;
using Spectre.Console.Rendering;

namespace Project1_VTCA.UI.Draw
{
    public class ConsoleLayout
    {
        public void Render(IRenderable menuContent, IRenderable viewContent, IRenderable notificationContent)
        {
            // 1. Tạo các khung (Panel) với nội dung tương ứng
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

            // 2. Tạo một Grid phụ (bên phải) để xếp chồng View và Notification
            var rightColumnGrid = new Grid()
                .AddColumn(); // <-- DÒNG SỬA LỖI QUAN TRỌNG LÀ ĐÂY!
                              // Chúng ta phải định nghĩa rằng grid này có MỘT cột.

            // Bây giờ việc thêm các hàng vào grid 1 cột này là hoàn toàn hợp lệ
            rightColumnGrid
                .AddRow(viewPanel)
                .AddRow(notificationPanel);

            // 3. Tạo Grid chính để đặt Menu (trái) và Grid phụ (phải) cạnh nhau
            var mainGrid = new Grid()
                .AddColumn(new GridColumn().Width(35)) // Cột 1
                .AddColumn(new GridColumn())           // Cột 2
                .AddRow(menuPanel, rightColumnGrid);   // Đặt 2 mục vào 2 cột



            // 4. Xóa màn hình cũ và vẽ lại toàn bộ layout mới
            AnsiConsole.Clear();
            AnsiConsole.Write(mainGrid);
        }
        public void RenderFormLayout(string title, Action formContent)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]{title}[/]").Centered());
            AnsiConsole.WriteLine();

            // Gọi Action để hiển thị các câu lệnh nhập liệu
            formContent?.Invoke();
        }
    }
}