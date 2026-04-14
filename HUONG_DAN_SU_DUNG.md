# HƯỚNG DẪN SỬ DỤNG PHẦN MỀM QUẢN LÝ KHO

Tài liệu này dành cho người dùng cuối, viết theo kiểu "làm theo từng bước". Không cần biết kỹ thuật.

---

## 1. Phần mềm này dùng để làm gì?

Phần mềm giúp công ty:

- Theo dõi **hàng có trong kho** (còn bao nhiêu, thuộc kho nào).
- Ghi nhận mỗi lần **nhập hàng vào** và **xuất hàng ra**.
- In **Giấy đề nghị cấp vật tư** để bộ phận / phòng ban ký duyệt.
- Xem **báo cáo tồn kho** cuối tháng, in ra PDF hoặc Excel.

Mọi thao tác đều bằng chuột, không cần gõ lệnh.

---

## 2. Cách khởi động

1. Mở thư mục cài đặt phần mềm.
2. Nháy đúp vào file **`QuanLyKho.exe`**.
3. Cửa sổ chính sẽ hiện ra với **menu bên trái** và **nội dung bên phải**.

Nếu cửa sổ không mở được, kiểm tra file `quanlykho.db` (cơ sở dữ liệu) có cùng thư mục không.

---

## 3. Làm quen với giao diện

```
┌─────────────────┬────────────────────────────────────────┐
│                 │                                        │
│   MENU TRÁI     │         VÙNG LÀM VIỆC                  │
│                 │                                        │
│   Tổng quan     │   (Tùy menu bạn bấm bên trái,          │
│   Vật tư        │    vùng này sẽ đổi)                    │
│   Nhóm vật tư   │                                        │
│   Đơn vị tính   │                                        │
│   Bộ phận       │                                        │
│   Kho           │                                        │
│   Nhập kho      │                                        │
│   Xuất kho      │                                        │
│   Đề nghị cấp VT│                                        │
│   Tồn kho       │                                        │
│                 │                                        │
└─────────────────┴────────────────────────────────────────┘
```

Bấm vào mục bất kỳ bên trái để chuyển màn hình.

---

## 4. Quy tắc chung cho mọi màn hình

Mọi màn hình trong phần mềm đều có cách dùng giống nhau:

| Nút | Ý nghĩa |
|-----|---------|
| **+ Thêm / Tạo mới** (xanh lá) | Mở form để nhập dữ liệu mới |
| **Sửa** (biểu tượng bút chì) | Chỉnh sửa dòng đang chọn |
| **Xóa** (biểu tượng thùng rác đỏ) | Xóa dòng (có hỏi xác nhận) |
| **Lưu** (đĩa mềm) | Lưu dữ liệu đang nhập |
| **Hủy / Quay lại** | Bỏ dở, không lưu |
| **In** | In phiếu ra PDF rồi mở để gửi máy in |
| **Xuất PDF / Xuất Excel** | Lưu file PDF / Excel vào máy |
| **⟨⟨ ⟨ Trang 1/5 ⟩ ⟩⟩** | Nút chuyển trang khi danh sách dài |

**Lưu ý:** Trường có dấu **(*)** là **bắt buộc nhập**. Nếu để trống, phần mềm sẽ hiện dòng báo lỗi màu đỏ và không cho lưu.

---

## 5. Quy trình khai báo lần đầu (rất quan trọng)

Khi mới dùng lần đầu, bạn **phải khai báo các danh mục gốc theo đúng thứ tự sau**, nếu không tạo phiếu sẽ không có gì để chọn:

### Bước 1: Đơn vị tính

Menu **"Đơn vị tính"** → **+ Thêm** → nhập tên (ví dụ: `Cái`, `Hộp`, `Mét`, `Kg`) → **Lưu**.

Lặp lại cho đến hết các đơn vị công ty đang dùng.

### Bước 2: Nhóm vật tư

Menu **"Nhóm vật tư"** → **+ Thêm** → nhập tên nhóm (ví dụ: `Văn phòng phẩm`, `Điện tử`, `Bảo hộ lao động`) → **Lưu**.

### Bước 3: Bộ phận

Menu **"Bộ phận"** → **+ Thêm** → nhập tên phòng ban (ví dụ: `Phòng Kế toán`, `Phòng Kỹ thuật`) → **Lưu**.

### Bước 4: Kho

Menu **"Kho"** → **+ Thêm** → nhập tên kho (ví dụ: `Kho chính`, `Kho tầng 2`) → **Lưu**.

### Bước 5: Vật tư

Menu **"Vật tư"** → **+ Thêm** → nhập:

- **Mã vật tư**: ví dụ `VT001`
- **Tên vật tư**: ví dụ `Bút bi Thiên Long`
- **Nhóm vật tư**: chọn trong danh sách (đã khai ở Bước 2)
- **Đơn vị tính**: chọn (đã khai ở Bước 1)
- **Ghi chú**: tùy (có thể để trống)

Bấm **Lưu**.

> **Mẹo:** Không cần khai hết vật tư một lúc. Khi nào có hàng mới thì vào đây thêm sau.

---

## 6. Nhập hàng vào kho (khi mua hàng, nhập bổ sung)

1. Menu trái chọn **"Nhập kho"**.
2. Bấm nút xanh **"+ Tạo phiếu nhập"**.
3. Form hiện ra, điền:
   - **Số phiếu**: **phần mềm tự sinh**, bạn không cần sửa.
   - **Ngày nhập**: mặc định là hôm nay, bấm vào để đổi nếu cần.
   - **Kho nhập (*)**: chọn kho nào sẽ chứa hàng.
   - **Người giao hàng**: tên người mang hàng đến.
   - **Ghi chú**: tùy.
4. Kéo xuống phần **"Chi tiết vật tư nhập"**:
   - Bấm nút **"+ Thêm dòng mới"** ở cuối bảng.
   - Cột **Vật tư**: bấm vào ô, **gõ tên vật tư để tìm**, rồi chọn từ gợi ý.
   - Cột **Số lượng**: nhập số hàng nhập.
   - Cột **Đơn giá**: nhập giá mua 1 đơn vị. Phần mềm tự tính **Thành tiền**.
   - Lặp lại với các vật tư khác (mỗi vật tư một dòng).
   - Muốn bỏ dòng thừa: bấm nút **"Xóa"** cuối dòng đó.
5. Điền thông tin người ký (nếu cần in): Người lập phiếu, Thủ kho, Kế toán trưởng, Giám đốc.
6. Bấm **"Lưu phiếu"** (nút xanh trên cùng bên phải).
7. Sau khi lưu, **tồn kho của các vật tư vừa nhập sẽ tự tăng lên**.

### In phiếu nhập
- Ở danh sách Nhập kho, **chọn dòng phiếu** cần in.
- Bấm **"In"** (PDF sẽ mở sẵn, bấm Ctrl+P để in) hoặc **"Xuất PDF" / "Xuất Excel"** để lưu file.

---

## 7. Xuất hàng ra khỏi kho (cấp phát cho bộ phận)

Tương tự "Nhập kho" nhưng là mang hàng đi:

1. Menu trái chọn **"Xuất kho"**.
2. Bấm **"+ Tạo phiếu xuất"**.
3. Điền:
   - **Số phiếu**: tự sinh.
   - **Ngày xuất**: mặc định hôm nay.
   - **Kho xuất (*)**: chọn kho lấy hàng.
   - **Bộ phận nhận**: chọn phòng ban nhận hàng.
   - **Người nhận**: tên người ra nhận hàng.
4. Phần chi tiết: thêm dòng, chọn vật tư, nhập số lượng.
5. Bấm **"Lưu phiếu"**.
6. Sau khi lưu, **tồn kho các vật tư sẽ tự giảm**.

In phiếu xuất: giống Nhập kho (nút In / Xuất PDF / Xuất Excel).

> **Lưu ý:** Nếu số lượng xuất nhiều hơn số tồn, phần mềm sẽ báo đỏ. Kiểm tra lại tồn kho trước khi xuất.

---

## 8. Tạo Giấy đề nghị cấp vật tư (để in cho CBNV ký)

Dùng khi một phòng ban muốn xin cấp vật tư, cần có giấy in ra để trình ký.

### Cách tạo:

1. Menu trái chọn **"Đề nghị cấp VT"**.
2. Bấm **"+ Tạo đề nghị"**.
3. Điền thông tin ở đầu trang:
   - **Số phiếu**: tự sinh (VD: `DN-20260414-001`), để nguyên.
   - **Ngày**: mặc định hôm nay.
   - **Trạng thái**: để `Nháp` khi mới tạo.
   - **Họ tên (*)**: tên người đề nghị (bắt buộc).
   - **Chức vụ**: tùy.
   - **Bộ phận**: chọn phòng ban.
4. Kéo xuống phần bảng vật tư, bấm **"+ Thêm dòng"**.
   - Cột **Tên vật tư (*)**: **BẮT BUỘC CHỌN MỘT VẬT TƯ** — gõ tên để tìm.
   - Cột **SL đề nghị (*)**: nhập số lượng **> 0**.
   - Cột **SL được duyệt**: để trống khi mới tạo, sau khi duyệt thì điền.
   - Cột **Ghi chú**: tùy.
5. **Mục đích sử dụng**: ghi ngắn gọn dùng để làm gì.
6. Bấm **"Lưu"**.

### Nếu bấm Lưu mà không lưu được:

Thường do một trong 3 lỗi sau (bắt buộc phải sửa mới lưu được):

| Lỗi | Cách sửa |
|-----|----------|
| Chưa điền **Họ tên người đề nghị** | Điền tên vào ô "Họ tên" |
| **Chưa chọn vật tư** ở dòng nào đó | Bấm vào ô "Tên vật tư", gõ tìm, chọn từ danh sách |
| **Số lượng đề nghị = 0** | Nhập số lượng > 0 |

### In giấy đề nghị:

1. Quay lại danh sách "Đề nghị cấp VT".
2. **Bấm chọn dòng đề nghị** vừa tạo (dòng sẽ sáng lên).
3. Bấm nút **"In"** phía trên.
4. File PDF sẽ tự mở — bấm **Ctrl+P** để in ra máy in, ký tay rồi trình duyệt.

### Sau khi duyệt:

- Vào lại đề nghị đó → bấm **"Sửa"**.
- Đổi **Trạng thái** thành `Đã duyệt` hoặc `Đã cấp`.
- Điền **SL được duyệt** ở từng dòng vật tư.
- **Lưu**.
- Sau đó nếu cần cấp thật: vào **"Xuất kho"** tạo phiếu xuất tương ứng.

---

## 9. Xem báo cáo Tồn kho

1. Menu trái chọn **"Tồn kho"**.
2. Ở thanh lọc phía trên, chọn:
   - **Tháng / Năm**: kỳ báo cáo (mặc định tháng hiện tại).
   - **Kho**: chọn kho cụ thể, hoặc bỏ trống để xem tất cả.
   - **Nhóm VT**: lọc theo nhóm nếu muốn.
   - **Tìm kiếm**: gõ mã hoặc tên vật tư.
3. Bảng hiện ra: Tồn đầu kỳ | Nhập trong kỳ | Xuất trong kỳ | Tồn cuối kỳ.
4. Muốn lưu báo cáo: bấm **"Xuất PDF"** hoặc **"In"**.

---

## 10. Màn hình Tổng quan

Menu **"Tổng quan"** hiện 5 ô số liệu nhanh:

- **Tổng số vật tư** trong danh mục.
- **Số kho** đang quản lý.
- **Tháng hiện tại**.
- **Nhập kho tháng này**: số phiếu và tổng tiền.
- **Xuất kho tháng này**: số phiếu và tổng tiền.

Chỉ để xem nhanh, không cần thao tác gì.

---

## 11. Một số câu hỏi thường gặp

**Q: Tôi xóa nhầm một vật tư, có khôi phục được không?**
A: Không. Phần mềm hỏi xác nhận trước khi xóa. Nếu đã bấm "Có", dữ liệu mất vĩnh viễn. Hãy backup file `quanlykho.db` định kỳ.

**Q: Vì sao tôi không chọn được Vật tư trong phiếu nhập / xuất?**
A: Vì bạn chưa khai báo vật tư đó. Vào menu **"Vật tư"** thêm trước.

**Q: Vì sao tôi không chọn được Bộ phận / Kho / Đơn vị tính?**
A: Tương tự — bạn cần khai báo ở các menu danh mục tương ứng trước.

**Q: Phần mềm báo "Số lượng xuất vượt quá tồn kho"?**
A: Vào **"Tồn kho"** kiểm tra lại số thật. Có thể cần tạo phiếu nhập bổ sung trước khi xuất.

**Q: Số phiếu tự sinh bị lỗi / trùng?**
A: Thử đóng phần mềm và mở lại. Số phiếu gồm tiền tố + ngày + số thứ tự, ví dụ `PN-20260414-001`.

**Q: Muốn sao lưu dữ liệu?**
A: Copy file **`quanlykho.db`** (ở thư mục cài đặt) sang USB hoặc ổ khác. Khi cần khôi phục thì chép đè lại.

**Q: Có thể dùng nhiều máy cùng lúc không?**
A: Không. Đây là phần mềm đơn máy. Mỗi máy có một file dữ liệu riêng.

---

## 12. Lưu ý quan trọng

- **Luôn bấm "Lưu"** trước khi đóng form, nếu không dữ liệu sẽ mất.
- **Xóa dữ liệu danh mục gốc** (Kho, Bộ phận, Vật tư) có thể ảnh hưởng phiếu cũ — cân nhắc kỹ.
- **Không sửa tay file `quanlykho.db`** bằng công cụ ngoài, dễ hỏng dữ liệu.
- **Backup định kỳ** file `quanlykho.db` (tuần/tháng) là việc nên làm.

---

## 13. Thứ tự làm việc hàng ngày (gợi ý)

| Khi nào | Làm gì | Ở menu |
|--------|--------|--------|
| Có hàng mới nhập về | Tạo phiếu nhập | Nhập kho |
| Phòng ban xin cấp vật tư | Tạo đề nghị, in ký | Đề nghị cấp VT |
| Sau khi đề nghị được duyệt | Tạo phiếu xuất | Xuất kho |
| Cuối ngày / cuối tuần | Xem báo cáo tồn | Tồn kho |
| Cuối tháng | Xuất báo cáo tồn PDF | Tồn kho → Xuất PDF |

---

*Nếu gặp vấn đề không có trong hướng dẫn này, liên hệ bộ phận kỹ thuật để được hỗ trợ.*
