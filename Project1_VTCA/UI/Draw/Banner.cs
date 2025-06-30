using Spectre.Console;

namespace Project1_VTCA.UI.Draw
{
    public static class Banner
    {
        public static void Show()
        {
            // Sử dụng FigletText với font "3-D" để tạo hiệu ứng nổi khối
            var figlet = new FigletText("SNEAKER SHOP")
                .Centered()
                .Color(Color.Orange1); // Giữ màu cam đặc trưng

            AnsiConsole.Write(figlet);

            // Thêm một đường kẻ với màu sắc khác để tạo điểm nhấn
            AnsiConsole.Write(new Rule().Centered().RuleStyle("deepskyblue2"));
        }
    }
}