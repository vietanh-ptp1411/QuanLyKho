namespace QuanLyKho;

public class AppSettings
{
    public CompanySettings Company { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
}

public class CompanySettings
{
    public string Name { get; set; } = "CÔNG TY CỔ PHẦN TẬP ĐOÀN SÔNG HỒNG THỦ ĐÔ";
    public string ShortName { get; set; } = "SÔNG HỒNG THỦ ĐÔ";
    public string Address { get; set; } = "Số 199 Lam Kinh - Thành phố Việt Trì - Phú Thọ";
    public string Location { get; set; } = "Tam Nông";
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = "Data Source=quanlykho.db";
}
