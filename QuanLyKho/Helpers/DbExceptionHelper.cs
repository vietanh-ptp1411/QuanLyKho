using Microsoft.EntityFrameworkCore;

namespace QuanLyKho.Helpers;

/// <summary>
/// Chuyển đổi lỗi kỹ thuật của EF Core / SQLite thành thông báo
/// thân thiện bằng tiếng Việt cho người dùng cuối.
/// </summary>
public static class DbExceptionHelper
{
    public static string GetMessage(Exception ex)
    {
        if (ex is DbUpdateException dbEx)
        {
            var inner = dbEx.InnerException?.Message ?? "";

            if (inner.Contains("UNIQUE"))
                return "Dữ liệu bị trùng (mã hoặc số phiếu đã tồn tại). Vui lòng kiểm tra lại.";

            if (inner.Contains("FOREIGN KEY"))
                return "Không thể xóa vì dữ liệu này đang được sử dụng ở nơi khác.";

            if (inner.Contains("NOT NULL"))
                return "Một số trường bắt buộc chưa được điền đủ. Vui lòng kiểm tra lại.";

            return $"Lỗi cơ sở dữ liệu: {inner}";
        }

        if (ex is OperationCanceledException)
            return "Thao tác đã bị hủy.";

        return ex.Message;
    }
}
