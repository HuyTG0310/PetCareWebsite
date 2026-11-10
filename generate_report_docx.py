# -*- coding: utf-8 -*-
import os
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import nsdecls, qn

def create_document():
    doc = docx.Document()
    
    # Page setup - Margins: Normal (1 inch = 72 pt = 1440 dxa)
    sections = doc.sections
    for section in sections:
        section.top_margin = Inches(0.8)
        section.bottom_margin = Inches(0.8)
        section.left_margin = Inches(0.9)
        section.right_margin = Inches(0.9)
        
    # Helpers for Styling
    NAVY = RGBColor(30, 58, 138)       # #1E3A8A
    SLATE = RGBColor(71, 85, 105)      # #475569
    DARK = RGBColor(30, 41, 59)        # #1E293B
    BLUE_ACCENT = RGBColor(2, 132, 199) # #0284C7
    
    def style_heading_1(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(14)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.name = 'Arial'
        run.font.size = Pt(15)
        run.font.color.rgb = NAVY
        return p

    def style_heading_2(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(11)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.name = 'Arial'
        run.font.size = Pt(12.5)
        run.font.color.rgb = BLUE_ACCENT
        return p

    def style_heading_3(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(8)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.name = 'Arial'
        run.font.size = Pt(11)
        run.font.color.rgb = DARK
        return p

    def add_p(text, bold_prefix=None, italic_prefix=None):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.line_spacing = 1.18
        if bold_prefix:
            r_bold = p.add_run(bold_prefix)
            r_bold.bold = True
            r_bold.font.name = 'Segoe UI'
            r_bold.font.size = Pt(10)
            r_bold.font.color.rgb = DARK
        if italic_prefix:
            r_it = p.add_run(italic_prefix)
            r_it.italic = True
            r_it.font.name = 'Segoe UI'
            r_it.font.size = Pt(10)
            r_it.font.color.rgb = SLATE
        r_text = p.add_run(text)
        r_text.font.name = 'Segoe UI'
        r_text.font.size = Pt(10)
        r_text.font.color.rgb = DARK
        return p

    def set_cell_background(cell, fill_hex):
        tcPr = cell._tc.get_or_add_tcPr()
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_hex}"/>')
        tcPr.append(shd)

    def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
        tcPr = cell._tc.get_or_add_tcPr()
        tcMar = parse_xml(f'''
            <w:tcMar {nsdecls("w")}>
                <w:top w:w="{top}" w:type="dxa"/>
                <w:bottom w:w="{bottom}" w:type="dxa"/>
                <w:left w:w="{left}" w:type="dxa"/>
                <w:right w:w="{right}" w:type="dxa"/>
            </w:tcMar>
        ''')
        tcPr.append(tcMar)

    def set_table_borders(table, color="CBD5E1", sz="4", val="single"):
        tblPr = table._tbl.tblPr
        borders = parse_xml(f'''
            <w:tblBorders {nsdecls("w")}>
                <w:top w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:bottom w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:insideH w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:insideV w:val="none"/>
                <w:left w:val="none"/>
                <w:right w:val="none"/>
            </w:tblBorders>
        ''')
        tblPr.append(borders)

    def add_code_block(code_text):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        tbl.autofit = False
        cell = tbl.cell(0, 0)
        cell.width = Inches(6.7)
        set_cell_background(cell, "F8FAFC")
        set_cell_margins(cell, top=120, bottom=120, left=180, right=180)
        
        tcPr = cell._tc.get_or_add_tcPr()
        border = parse_xml(f'''
            <w:tcBorders {nsdecls("w")}>
                <w:left w:val="single" w:sz="18" w:space="0" w:color="2563EB"/>
                <w:top w:val="single" w:sz="4" w:space="0" w:color="E2E8F0"/>
                <w:right w:val="single" w:sz="4" w:space="0" w:color="E2E8F0"/>
                <w:bottom w:val="single" w:sz="4" w:space="0" w:color="E2E8F0"/>
            </w:tcBorders>
        ''')
        tcPr.append(border)
        
        p = cell.paragraphs[0]
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.line_spacing = 1.15
        run = p.add_run(code_text.strip())
        run.font.name = 'Consolas'
        run.font.size = Pt(9.0)
        run.font.color.rgb = RGBColor(30, 41, 59)
        doc.add_paragraph().paragraph_format.space_after = Pt(3)

    def add_callout(text, title="LƯU Ý QUAN TRỌNG", box_type="info"):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        tbl.autofit = False
        cell = tbl.cell(0, 0)
        cell.width = Inches(6.7)
        
        bg_color = "EFF6FF" if box_type == "info" else "FEF3C7"
        border_color = "3B82F6" if box_type == "info" else "F59E0B"
        title_color = RGBColor(30, 64, 175) if box_type == "info" else RGBColor(180, 83, 9)
        
        set_cell_background(cell, bg_color)
        set_cell_margins(cell, top=130, bottom=130, left=180, right=180)
        
        tcPr = cell._tc.get_or_add_tcPr()
        border = parse_xml(f'''
            <w:tcBorders {nsdecls("w")}>
                <w:left w:val="single" w:sz="24" w:space="0" w:color="{border_color}"/>
                <w:top w:val="none"/>
                <w:right w:val="none"/>
                <w:bottom w:val="none"/>
            </w:tcBorders>
        ''')
        tcPr.append(border)
        
        p = cell.paragraphs[0]
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(2)
        r_title = p.add_run(f"📌 {title}: ")
        r_title.bold = True
        r_title.font.name = 'Segoe UI'
        r_title.font.size = Pt(10)
        r_title.font.color.rgb = title_color
        
        r_text = p.add_run(text)
        r_text.font.name = 'Segoe UI'
        r_text.font.size = Pt(9.5)
        r_text.font.color.rgb = RGBColor(31, 41, 55)
        doc.add_paragraph().paragraph_format.space_after = Pt(3)

    # ==========================
    # 0. HEADER & TITLE BLOCK
    # ==========================
    title_tbl = doc.add_table(rows=1, cols=1)
    title_tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    t_cell = title_tbl.cell(0, 0)
    t_cell.width = Inches(6.7)
    set_cell_background(t_cell, "1E3A8A") # Navy
    set_cell_margins(t_cell, top=240, bottom=240, left=250, right=250)
    
    tp1 = t_cell.paragraphs[0]
    tp1.alignment = WD_ALIGN_PARAGRAPH.CENTER
    tp1.paragraph_format.space_after = Pt(4)
    tr1 = tp1.add_run("BÁO CÁO GIẢI THÍCH CHI TIẾT SOURCE CODE")
    tr1.bold = True
    tr1.font.name = 'Arial'
    tr1.font.size = Pt(16)
    tr1.font.color.rgb = RGBColor(255, 255, 255)
    
    tp2 = t_cell.add_paragraph()
    tp2.alignment = WD_ALIGN_PARAGRAPH.CENTER
    tp2.paragraph_format.space_after = Pt(0)
    tr2 = tp2.add_run("MODULE: AUTHENTICATION & MANAGE REVIEW (PETCARE BOOKING SYSTEM)")
    tr2.bold = True
    tr2.font.name = 'Arial'
    tr2.font.size = Pt(11.5)
    tr2.font.color.rgb = RGBColor(224, 231, 255)
    
    doc.add_paragraph().paragraph_format.space_after = Pt(6)
    
    # Metadata Table
    meta_tbl = doc.add_table(rows=4, cols=2)
    meta_tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    meta_tbl.autofit = False
    col_widths = [Inches(2.2), Inches(4.5)]
    set_table_borders(meta_tbl, color="E2E8F0")
    
    meta_data = [
        ("Họ và tên sinh viên phụ trách:", "Đoàn Minh Duy"),
        ("Mã số sinh viên (Roll Number):", "CE191481"),
        ("Môn học & Đề tài:", "SWD392 - Software Architecture and Design | PetCare Booking Website"),
        ("Kiến trúc công nghệ:", ".NET 8 Web API, Clean Architecture, CQRS, MediatR, EF Core 8, BCrypt, JWT")
    ]
    for row_idx, (k, v) in enumerate(meta_data):
        for col_idx in range(2):
            cell = meta_tbl.cell(row_idx, col_idx)
            cell.width = col_widths[col_idx]
            set_cell_margins(cell, top=80, bottom=80, left=120, right=120)
            if row_idx % 2 == 1:
                set_cell_background(cell, "F8FAFC")
            p = cell.paragraphs[0]
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            if col_idx == 0:
                r = p.add_run(k)
                r.bold = True
                r.font.name = 'Segoe UI'
                r.font.size = Pt(9.5)
                r.font.color.rgb = SLATE
            else:
                r = p.add_run(v)
                r.bold = (row_idx < 2)
                r.font.name = 'Segoe UI'
                r.font.size = Pt(9.5)
                r.font.color.rgb = DARK
    
    doc.add_paragraph().paragraph_format.space_after = Pt(10)

    # ==========================
    # PHẦN 1: KIẾN TRÚC TỔNG QUAN HỆ THỐNG
    # ==========================
    style_heading_1("1. KIẾN TRÚC TỔNG QUAN CỦA DỰ ÁN (CLEAN ARCHITECTURE & CQRS)")
    add_p("Hệ thống PetCareBooking được xây dựng dựa trên mẫu kiến trúc Clean Architecture (Onion Architecture) phân tách thành 5 tầng độc lập, kết hợp triệt để với mô hình CQRS (Command Query Responsibility Segregation) và thư viện MediatR:")
    
    # Bullet points
    add_p("Đóng vai trò là cổng giao tiếp tiếp nhận Request HTTP từ Frontend/Swagger/Postman, chuyển giao trực tiếp cho MediatR và trả về HTTP Response chuẩn (ApiResponse). Controller ở đây tuân thủ nguyên lý Thin Controller (không chứa nghiệp vụ, chỉ ủy quyền cho MediatR).", bold_prefix="1. PetCareBooking.API: ")
    add_p("Trọng tâm của hệ thống, chứa toàn bộ nghiệp vụ (Use Cases). Được chia thành các Features độc lập theo mô hình CQRS (Commands cho ghi/thay đổi dữ liệu, Queries cho đọc dữ liệu), kèm theo DTOs, Interface dịch vụ và FluentValidation.", bold_prefix="2. PetCareBooking.Application: ")
    add_p("Chứa các thực thể cốt lõi (Entities như Customer, Staff, Review, Booking...), Enums trạng thái (BookingStatus, BookingItemStatus), không phụ thuộc vào bất kỳ framework bên ngoài nào.", bold_prefix="3. PetCareBooking.Domain: ")
    add_p("Cung cấp các dịch vụ bên thứ 3 và tiện ích hệ thống như tạo JWT Token (JwtTokenGenerator), gửi email OTP (EmailService), lấy user đăng nhập hiện tại (CurrentUserService).", bold_prefix="4. PetCareBooking.Infrastructure: ")
    add_p("Quản trị cơ sở dữ liệu Entity Framework Core (ApplicationDbContext), thực thi GenericRepository và UnitOfWork.", bold_prefix="5. PetCareBooking.Persistence: ")

    add_callout(
        "Mô hình CQRS kết hợp MediatR giúp code của Đoàn Minh Duy cực kỳ dễ bảo trì và mở rộng: Mỗi chức năng (Login, Register, AddReview, UpdateReview...) là một cặp Command/Query và Handler riêng biệt. Khi cần sửa lỗi hoặc mở rộng logic của Login, ta chỉ cần mở đúng thư mục Features/Auth/Commands/Login mà không sợ làm ảnh hưởng đến các chức năng khác.",
        title="Ý NGHĨA KIẾN TRÚC ĐỐI VỚI ĐỒ ÁN SWD392",
        box_type="info"
    )

    # ==========================
    # PHẦN 2: MODULE AUTHENTICATION
    # ==========================
    style_heading_1("2. MODULE AUTHENTICATION (XÁC THỰC NGƯỜI DÙNG)")
    add_p("Module Authentication do sinh viên Đoàn Minh Duy (CE191481) phụ trách bao gồm 4 chức năng chính được phân công trong bảng tiến độ: Register, Login, Logout, Forget Password. Ngoài ra hệ thống còn mở rộng hỗ trợ Verify OTP, Resend OTP, Reset Password và Staff Login.")

    style_heading_2("2.1. Bảng tổng hợp các Endpoint API của Module Authentication")
    
    auth_table = doc.add_table(rows=7, cols=4)
    auth_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    auth_table.autofit = False
    auth_col_widths = [Inches(1.2), Inches(2.2), Inches(1.3), Inches(2.0)]
    set_table_borders(auth_table)
    
    auth_api_data = [
        ("Chức năng", "Endpoint (URL)", "Phương thức", "Quyền / Xác thực"),
        ("Register", "/api/Auth/register", "POST", "Public (Guest)"),
        ("Login (Customer)", "/api/Auth/login", "POST", "Public (Guest)"),
        ("Staff Login", "/api/Auth/staff/login", "POST", "Public (Staff/Admin)"),
        ("Logout", "/api/Auth/logout", "POST", "Yêu cầu JWT Bearer Token"),
        ("Forgot Password", "/api/Auth/forgot-password", "POST", "Public (Guest)"),
        ("Reset Password", "/api/Auth/reset-password", "POST", "Public (Kèm OTP hợp lệ)")
    ]
    for r_i, row in enumerate(auth_api_data):
        for c_i, val in enumerate(row):
            cell = auth_table.cell(r_i, c_i)
            cell.width = auth_col_widths[c_i]
            set_cell_margins(cell, top=70, bottom=70, left=100, right=100)
            p = cell.paragraphs[0]
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            if r_i == 0:
                set_cell_background(cell, "1E3A8A")
                run = p.add_run(val)
                run.bold = True
                run.font.name = 'Segoe UI'
                run.font.size = Pt(9.5)
                run.font.color.rgb = RGBColor(255, 255, 255)
            else:
                if r_i % 2 == 1:
                    set_cell_background(cell, "F8FAFC")
                run = p.add_run(val)
                run.font.name = 'Consolas' if c_i in [1, 2] else 'Segoe UI'
                run.font.size = Pt(9.0)
                run.font.color.rgb = DARK

    doc.add_paragraph().paragraph_format.space_after = Pt(8)

    # 2.2. HƯỚNG DẪN CHI TIẾT CHỨC NĂNG LOGIN
    style_heading_2("2.2. HƯỚNG DẪN CHI TIẾT: MUỐN LOGIN THÌ VÀO CODE CHỖ NÀO, GỌI TỚI ĐÂU, TỪNG DÒNG CÓ NGHĨA GÌ?")
    add_p("Đây là câu trả lời chi tiết và trực quan nhất cho câu hỏi thực tế của bạn khi làm bài hoặc trình bày với giảng viên hướng dẫn:")

    style_heading_3("Bước 1: Vị trí Controller tiếp nhận Request Login")
    add_p("Muốn xem điểm khởi đầu tiếp nhận yêu cầu Login của khách hàng, bạn mở file sau:", bold_prefix="👉 File: ")
    add_p("PetCareBooking.API/Controllers/AuthController.cs (Dòng 65 - 70)", italic_prefix="Đường dẫn: ")
    
    code_ctrl_login = """[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginCommand command)
{
    var response = await _mediator.Send(command);
    return StatusCode(response.StatusCode, response);
}"""
    add_code_block(code_ctrl_login)
    
    add_p("Khai báo đây là API nhận giao thức POST với đường dẫn URL là /api/Auth/login.", bold_prefix="• Dòng 1 [HttpPost(\"login\")]: ")
    add_p("Nhận dữ liệu dạng JSON từ Client truyền lên trong Body và tự động gán vào đối tượng LoginCommand (gồm Email và Password).", bold_prefix="• Dòng 2 public async Task<IActionResult> Login([FromBody] LoginCommand command): ")
    add_p("ĐÂY LÀ DÒNG GỌI QUAN TRỌNG NHẤT! _mediator là trung gian điều hướng. Dòng này gửi command qua hệ thống MediatR. MediatR sẽ quét tìm Handler nào đăng ký nhận LoginCommand (chính là LoginCommandHandler) và gọi hàm Handle() của nó.", bold_prefix="• Dòng 4 var response = await _mediator.Send(command): ")
    add_p("Lấy StatusCode kết quả (200 OK nếu thành công, 400 BadRequest nếu sai mật khẩu) trả về cho Client cùng với dữ liệu JSON ApiResponse.", bold_prefix="• Dòng 5 return StatusCode(response.StatusCode, response): ")

    style_heading_3("Bước 2: Đối tượng mang dữ liệu (Command) và Kiểm tra dữ liệu (Validator)")
    add_p("PetCareBooking.Application/Features/Auth/Commands/Login/LoginCommand.cs", bold_prefix="👉 File Command: ")
    code_login_cmd = """public class LoginCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}"""
    add_code_block(code_login_cmd)
    add_p("Class này kế thừa IRequest<ApiResponse<LoginResponseDto>>, định nghĩa rõ ràng: 'Khi thực thi Command này, kết quả nhận về bắt buộc phải là một đối tượng ApiResponse bọc lấy LoginResponseDto (chứa Token và thông tin user)'.")

    add_p("PetCareBooking.Application/Features/Auth/Commands/Login/LoginCommandValidator.cs", bold_prefix="👉 File Validator: ")
    code_login_val = """public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}"""
    add_code_block(code_login_val)
    add_p("Trước khi bước vào xử lý nghiệp vụ, FluentValidation sẽ tự động chặn các request lỗi: Email bị bỏ trống hoặc sai cú pháp email, Password bị bỏ trống. Giúp bảo vệ hệ thống không bị crash hoặc query thừa thãi vào Database.")

    style_heading_3("Bước 3: Nơi xử lý nghiệp vụ kiểm tra tài khoản (LoginCommandHandler)")
    add_p("Sau khi gọi _mediator.Send(command) ở Controller, MediatR sẽ nhảy thẳng vào file này để xử lý:", bold_prefix="👉 File: ")
    add_p("PetCareBooking.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs (Dòng 21 - 73)", italic_prefix="Đường dẫn: ")

    code_login_handler = """public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
{
    // DÒNG 1: Truy vấn tài khoản trong Database theo Email người dùng gửi lên
    var customer = await _repository.GetQueryable()
        .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

    // DÒNG 2: Kiểm tra nếu không tìm thấy email trong hệ thống
    if (customer == null)
    {
        return new ApiResponse<LoginResponseDto>
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Invalid email or password."
        };
    }

    // DÒNG 3: So khớp mật khẩu nhập vào với mật khẩu đã mã hóa lưu trong Database
    var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash);
    if (!isPasswordValid)
    {
        return new ApiResponse<LoginResponseDto>
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Invalid email or password."
        };
    }

    // DÒNG 4: Kiểm tra trạng thái tài khoản đã kích hoạt OTP hay chưa
    if (!customer.IsActive)
    {
        return new ApiResponse<LoginResponseDto>
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Account is not activated. Please verify OTP code before logging in."
        };
    }

    // DÒNG 5: Sinh mã JWT Token chứa thông tin và quyền hạn người dùng
    var token = _jwtTokenGenerator.GenerateToken(customer);

    // DÒNG 6: Đóng gói kết quả thành công và trả về cho Controller
    return new ApiResponse<LoginResponseDto>
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
        Message = "Login successful!",
        Result = new LoginResponseDto
        {
            Token = token,
            CustomerId = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber
        }
    };
}"""
    add_code_block(code_login_handler)

    add_p("GIẢI THÍCH CHI TIẾT TỪNG ĐOẠN CODE TRONG HÀM HANDLE():", bold_prefix="📖 ")
    add_p("Sử dụng Entity Framework Core bất đồng bộ để tìm bản ghi Khách hàng (Customer) khớp với Email. Nếu người dùng nhập sai email, customer sẽ là null.", bold_prefix="1. _repository.GetQueryable().FirstOrDefaultAsync(...): ")
    add_p("Nếu không tìm thấy email, trả về lỗi Status 400. Chú ý thông báo lỗi ghi chung là 'Invalid email or password' (không nói rõ sai email hay sai mật khẩu) nhằm đảm bảo tiêu chuẩn an ninh thông tin OWASP, ngăn chặn kẻ tấn công dò quét email người dùng.", bold_prefix="2. if (customer == null): ")
    add_p("Trong database, mật khẩu KHÔNG ĐƯỢC LƯU DẠNG CHỮ RÕ (plaintext) mà được băm (hash) bằng thuật toán BCrypt kèm muối (salt). Hàm BCrypt.Verify sẽ lấy mật khẩu người dùng vừa gõ và băm thử, so sánh với PasswordHash trong DB. Trả về false nếu sai mật khẩu.", bold_prefix="3. BCrypt.Net.BCrypt.Verify(...): ")
    add_p("Dù email và mật khẩu đúng, nhưng nếu tài khoản chưa hoàn tất bước nhập mã OTP kích hoạt (IsActive == false) thì hệ thống từ chối đăng nhập và yêu cầu người dùng xác thực OTP trước.", bold_prefix="4. if (!customer.IsActive): ")
    add_p("Gọi sang tầng Infrastructure để đóng gói thông tin (Claims) thành một chuỗi token mã hóa JSON Web Token.", bold_prefix="5. _jwtTokenGenerator.GenerateToken(customer): ")

    style_heading_3("Bước 4: Nơi khởi tạo JWT Token")
    add_p("PetCareBooking.Infrastructure/Services/JwtTokenGenerator.cs (Dòng 20 - 31 & 57 - 75)", bold_prefix="👉 File: ")
    code_jwt = """public string GenerateToken(Customer customer)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()), // Định danh người dùng
        new Claim(JwtRegisteredClaimNames.Email, customer.Email),       // Email người dùng
        new Claim(ClaimTypes.Name, customer.FullName),                  // Tên người dùng
        new Claim(ClaimTypes.Role, "Customer")                          // Vai trò là Customer
    };

    return BuildToken(claims);
}"""
    add_code_block(code_jwt)
    add_p("Hàm BuildToken() sẽ sử dụng SecretKey định nghĩa trong appsettings.json, thuật toán HmacSha256 để ký chữ ký điện tử và gán thời gian hết hạn (Expires, mặc định 120 phút). Token này trả về cho Client lưu vào LocalStorage/Cookie để dùng cho các request tiếp theo.")

    # 2.3. LÀM THỬ VÍ DỤ LOGIN MẪU
    style_heading_2("2.3. THỰC HÀNH LÀM THỬ CÁI LOGIN MẪU (DEMO REQUEST & RESPONSE)")
    add_p("Dưới đây là một kịch bản Login thực tế từ lúc gửi request từ Postman/Swagger đến lúc nhận kết quả:")

    add_p("1. Thông tin Request HTTP Client gửi lên:", bold_prefix="🔹 ")
    add_code_block("""POST /api/Auth/login HTTP/1.1
Host: localhost:7025
Content-Type: application/json

{
  "email": "customer.test@gmail.com",
  "password": "Password123@"
}""")

    add_p("2. Quá trình xử lý nội bộ của Server:", bold_prefix="🔹 ")
    add_p("• Bước 1: AuthController nhận request, tạo đối tượng LoginCommand chứa email và pass.", bold_prefix="  ")
    add_p("• Bước 2: LoginCommandValidator kiểm tra: email hợp lệ, mật khẩu không rỗng -> Hợp lệ.", bold_prefix="  ")
    add_p("• Bước 3: LoginCommandHandler tìm trong bảng Customers: thấy user có email customer.test@gmail.com.", bold_prefix="  ")
    add_p("• Bước 4: BCrypt.Verify(\"Password123@\", \"$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi\") -> Khớp (True).", bold_prefix="  ")
    add_p("• Bước 5: Kiểm tra customer.IsActive == true -> Tài khoản đã kích hoạt.", bold_prefix="  ")
    add_p("• Bước 6: JwtTokenGenerator sinh mã Token có Role = \"Customer\".", bold_prefix="  ")

    add_p("3. Dữ liệu JSON Server phản hồi về Client (Response 200 OK):", bold_prefix="🔹 ")
    add_code_block("""HTTP/1.1 200 OK
Content-Type: application/json

{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Login successful!",
  "result": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkZjgzYzcyNS1hYWE4LTRiYzgtYjFlZC1kOTc1YWY4MWJhMDIiLCJlbWFpbCI6ImN1c3RvbWVyLnRlc3RAZ21haWwuY29tIiwibmFtZSI6Ik5ndXnDs24gVsSDbiBBIiwicm9sZSI6IkN1c3RvbWVyIiwiZXhwIjoxNzk0MjIzMjAwfQ.SignatureStringExample...",
    "customerId": "df83c725-aaa8-4bc8-b1ed-d975af81ba02",
    "fullName": "Nguyễn Văn A",
    "email": "customer.test@gmail.com",
    "phoneNumber": "0912345678"
  }
}""")

    add_callout(
        "Sau khi nhận được chuỗi 'token' ở trên, ở các API cần bảo vệ (như Đặt lịch, Thêm đánh giá, Đổi mật khẩu), Frontend chỉ cần đính kèm Header: 'Authorization: Bearer <chuỗi_token>'. Server ASP.NET Core sẽ tự động giải mã token, xác định được ngay ai đang đăng nhập (CustomerId) mà người dùng không cần phải truyền lại thông tin cá nhân.",
        title="CÁCH CLIENT SỬ DỤNG TOKEN SAU KHI LOGIN",
        box_type="info"
    )

    # 2.4. CÁC CHỨC NĂNG CÒN LẠI CỦA AUTHENTICATION
    style_heading_2("2.4. Giải thích code các chức năng còn lại của Module Authentication")
    
    add_p("1. Register (Đăng ký tài khoản):", bold_prefix="👉 ")
    add_p("File RegisterCommandHandler.cs: Kiểm tra xem Email hoặc Số điện thoại đã được đăng ký và kích hoạt chưa. Nếu chưa đăng ký, tạo đối tượng Customer với IsActive = false, băm mật khẩu bằng BCrypt, sinh mã OTP ngẫu nhiên 6 số (Random.Shared.Next(100000, 999999)), lưu OtpExpiry = DateTime.UtcNow.AddMinutes(5) và gọi _emailService.SendOtpEmailAsync() gửi OTP về email người dùng.")

    add_p("2. Verify OTP & Resend OTP:", bold_prefix="👉 ")
    add_p("File VerifyOtpCommandHandler.cs: Nhận Email và OTP từ người dùng. So khớp mã OTP trong DB và kiểm tra còn hạn (OtpExpiry > DateTime.UtcNow) hay không. Nếu hợp lệ, cập nhật customer.IsActive = true, xóa mã OTP và lưu vào DB.")

    add_p("3. Logout (Đăng xuất):", bold_prefix="👉 ")
    add_p("File LogoutCommandHandler.cs và AuthController.cs (Endpoint có attribute [Authorize]): Vì hệ thống sử dụng Stateless JWT, Logout ở backend trả về StatusCode 200 OK để Client xóa Token khỏi bộ nhớ (LocalStorage/Cookie).")

    add_p("4. Forget Password & Reset Password:", bold_prefix="👉 ")
    add_p("File ForgotPasswordCommandHandler.cs: Tiếp nhận Email cần lấy lại mật khẩu. Nếu tài khoản tồn tại và đang hoạt động, hệ thống sinh mã OTP 6 số mới có hạn 5 phút và gửi email. Sau đó, tại ResetPasswordCommandHandler.cs, người dùng gửi mã OTP kèm Mật khẩu mới; hệ thống xác thực OTP và cập nhật PasswordHash mới.")

    add_p("5. Staff & Admin Login:", bold_prefix="👉 ")
    add_p("File StaffLoginCommandHandler.cs: Dành riêng cho Nhân viên và Quản trị viên (/api/Auth/staff/login). Khác với Customer, Handler này truy vấn bảng Staffs và bảng Roles, RolePermissions để nạp toàn bộ danh sách Quyền hạn (Permissions) vào JWT Token.")

    # ==========================
    # PHẦN 3: MODULE MANAGE REVIEW
    # ==========================
    style_heading_1("3. MODULE MANAGE REVIEW (QUẢN LÝ ĐÁNH GIÁ DỊCH VỤ)")
    add_p("Module Manage Review do sinh viên Đoàn Minh Duy (CE191481) phụ trách bao gồm 6 chức năng được thể hiện trong phân công: View review list, View review detail, Add review, Edit review, Delete review, Search review.")

    style_heading_2("3.1. Bảng tổng hợp các Endpoint API của Module Manage Review")
    
    review_table = doc.add_table(rows=6, cols=4)
    review_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    review_table.autofit = False
    rev_col_widths = [Inches(1.5), Inches(2.1), Inches(1.2), Inches(1.9)]
    set_table_borders(review_table)
    
    review_api_data = [
        ("Chức năng", "Endpoint (URL)", "Method", "Phân quyền (Authorize)"),
        ("View list & Search", "/api/Reviews", "GET", "Public (Guest/Customer/Staff)"),
        ("View detail", "/api/Reviews/{id}", "GET", "Public (Guest/Customer/Staff)"),
        ("Add review", "/api/Reviews", "POST", "[Authorize(Roles = \"Customer\")]"),
        ("Edit review", "/api/Reviews/{id}", "PUT", "[Authorize] (Chủ review / Admin)"),
        ("Delete review", "/api/Reviews/{id}", "DELETE", "[Authorize] (Chủ review / Admin)")
    ]
    for r_i, row in enumerate(review_api_data):
        for c_i, val in enumerate(row):
            cell = review_table.cell(r_i, c_i)
            cell.width = rev_col_widths[c_i]
            set_cell_margins(cell, top=70, bottom=70, left=100, right=100)
            p = cell.paragraphs[0]
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            if r_i == 0:
                set_cell_background(cell, "1E3A8A")
                run = p.add_run(val)
                run.bold = True
                run.font.name = 'Segoe UI'
                run.font.size = Pt(9.5)
                run.font.color.rgb = RGBColor(255, 255, 255)
            else:
                if r_i % 2 == 1:
                    set_cell_background(cell, "F8FAFC")
                run = p.add_run(val)
                run.font.name = 'Consolas' if c_i in [1, 2] else 'Segoe UI'
                run.font.size = Pt(9.0)
                run.font.color.rgb = DARK

    doc.add_paragraph().paragraph_format.space_after = Pt(8)

    style_heading_2("3.2. Chi tiết và Giải thích code từng chức năng của Module Review")

    # UC-8.1 & 8.6
    style_heading_3("1. View Review List & Search Review (Xem danh sách & Tìm kiếm đánh giá)")
    add_p("PetCareBooking.API/Controllers/ReviewsController.cs (Hàm GetReviews)", bold_prefix="👉 Controller: ")
    add_p("PetCareBooking.Application/Features/Reviews/Queries/GetReviewsWithPagination/GetReviewsWithPaginationQueryHandler.cs", bold_prefix="👉 Handler: ")
    
    code_review_list = """var query = _repository.GetQueryable().AsNoTracking();

// Tìm kiếm/Lọc theo Dịch vụ (ServiceId) nếu có
if (request.ServiceId.HasValue)
{
    query = query.Where(r => r.ServiceId == request.ServiceId.Value);
}

// Tìm kiếm/Lọc theo số sao Rating (1 đến 5 sao) nếu có
if (request.Rating.HasValue)
{
    query = query.Where(r => r.Rating == request.Rating.Value);
}

// Tính tổng số lượng bản ghi thỏa mãn điều kiện
int totalCount = await query.CountAsync(cancellationToken);

// Thực hiện phân trang và sắp xếp giảm dần theo ngày tạo mới nhất
var items = await query
    .OrderByDescending(r => r.CreatedAt)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(r => new ReviewListResponseDto
    {
        Id = r.Id,
        ServiceId = r.ServiceId,
        ServiceName = r.Service.Name,
        CustomerId = r.CustomerId,
        CustomerName = r.Customer.FullName,
        CustomerAvatarUrl = r.Customer.AvatarUrl,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    })
    .ToListAsync(cancellationToken);"""
    add_code_block(code_review_list)
    
    add_p("Ý NGHĨA KỸ THUẬT:", bold_prefix="💡 ")
    add_p("Chức năng này đảm nhiệm đồng thời cả 'View review list' và 'Search review'. Khách hàng có thể tìm kiếm các đánh giá của một dịch vụ cụ thể hoặc lọc những đánh giá 5 sao / 1 sao. Sử dụng AsNoTracking() giúp tăng tốc độ truy vấn đọc dữ liệu, phân trang Skip/Take tối ưu hiệu năng không lo tràn bộ nhớ khi có hàng ngàn review.")

    # UC-8.2
    style_heading_3("2. View Review Detail (Xem chi tiết một đánh giá)")
    add_p("PetCareBooking.Application/Features/Reviews/Queries/GetReviewById/GetReviewByIdQueryHandler.cs", bold_prefix="👉 Handler: ")
    add_p("Truy vấn theo Id duy nhất (Guid) của Review, nạp thông tin dịch vụ (Service.Name) và thông tin khách hàng (Customer.FullName, Customer.Email), trả về DTO chi tiết ReviewDetailResponseDto.")

    # UC-8.3
    style_heading_3("3. Add Review (Thêm đánh giá dịch vụ mới)")
    add_p("PetCareBooking.Application/Features/Reviews/Commands/CreateReview/CreateReviewCommandHandler.cs", bold_prefix="👉 Handler: ")
    add_p("ĐÂY LÀ CHỨC NĂNG CÓ NGHIỆP VỤ PHỨC TẠP NHẤT TRONG MODULE! Hệ thống cài đặt các ràng buộc nghiệp vụ vô cùng chặt chẽ để chống review ảo và gian lận:")
    
    code_add_review = """// 1. Kiểm tra đăng nhập và lấy CustomerId từ Token
var customerId = _currentUserService.UserId;

// 2. Kiểm tra xem BookingItem có thuộc về đúng khách hàng này không
var bookingItem = await _bookingItemRepository.GetQueryable()
    .Include(bi => bi.Booking)
    .FirstOrDefaultAsync(bi => bi.Id == request.BookingItemId && bi.Booking.CustomerId == customerId.Value, cancellationToken);

if (bookingItem == null)
    return BadRequest("Booking item not found or does not belong to you.");

// 3. RÀNG BUỘC NGHIỆP VỤ 1: Đơn đặt lịch tổng phải ở trạng thái Completed (đã thanh toán & hoàn tất tại quầy)
if (bookingItem.Booking.Status != BookingStatus.Completed)
    return BadRequest("You can only review services after the booking is completed (checked out).");

// 4. RÀNG BUỘC NGHIỆP VỤ 2: Dịch vụ cụ thể này phải hoàn thành
if (bookingItem.Status != BookingItemStatus.Completed)
    return BadRequest("You can only review a completed service.");

// 5. RÀNG BUỘC NGHIỆP VỤ 3: Mỗi dịch vụ trong đơn chỉ được đánh giá 1 lần duy nhất
var isReviewed = await _reviewRepository.GetQueryable()
    .AnyAsync(r => r.BookingItemId == request.BookingItemId, cancellationToken);
if (isReviewed)
    return BadRequest("This booking item has already been reviewed.");

// 6. Tạo thực thể Review mới và lưu Database
var review = new Review
{
    BookingItemId = bookingItem.Id,
    ServiceId = bookingItem.ServiceId,
    CustomerId = customerId.Value,
    Rating = request.Rating,
    Comment = request.Comment,
    CreatedAt = DateTime.UtcNow
};
await _reviewRepository.AddAsync(review);
await _unitOfWork.SaveChangesAsync(cancellationToken);"""
    add_code_block(code_add_review)

    # UC-8.4
    style_heading_3("4. Edit Review (Chỉnh sửa đánh giá)")
    add_p("PetCareBooking.Application/Features/Reviews/Commands/UpdateReview/UpdateReviewCommandHandler.cs", bold_prefix="👉 Handler: ")
    add_p("Chức năng chỉnh sửa đánh giá có 2 điều kiện bảo mật quan trọng:", bold_prefix="📌 ")
    add_p("Chỉ chủ bài viết (CustomerId trùng với người đăng nhập) HOẶC Admin/Staff mới có quyền sửa.", bold_prefix="• Phân quyền sở hữu: ")
    add_p("Khách hàng chỉ được phép chỉnh sửa đánh giá TỐI ĐA 1 LẦN. Trong code kiểm tra: if (!isAdminOrStaff && review.UpdatedAt != null) -> Báo lỗi. Nếu khách hàng muốn sửa tiếp thì phải xóa đánh giá và gửi lại từ đầu. Quy định này ngăn chặn tình trạng khách hàng liên tục thay đổi đánh giá thất thường.", bold_prefix="• Quy định sửa 1 lần: ")

    # UC-8.5
    style_heading_3("5. Delete Review (Xóa đánh giá)")
    add_p("PetCareBooking.Application/Features/Reviews/Commands/DeleteReview/DeleteReviewCommandHandler.cs", bold_prefix="👉 Handler: ")
    add_p("Xóa đánh giá khỏi cơ sở dữ liệu. Chỉ chính chủ sở hữu review (Customer) hoặc Admin/Staff (kiểm duyệt nội dung xúc phạm/spam) mới có quyền thực hiện xóa bài.")

    # ==========================
    # PHẦN 4: BẢNG TỔNG HỢP CÂU HỎI BẢO VỆ ĐỒ ÁN SWD392
    # ==========================
    style_heading_1("4. BẢNG CÂU HỎI THƯỜNG GẶP KHI BẢO VỆ VÀ PHẢN BIỆN ĐỒ ÁN SWD392")
    add_p("Khi giảng viên phản biện đồ án hỏi về 2 module Authentication và Manage Review, sinh viên Đoàn Minh Duy có thể tự tin trả lời theo các gợi ý chuẩn chuyên môn sau:")

    qa_list = [
        ("Câu 1: Vì sao trong AuthController và ReviewsController nhóm không viết logic xử lý mà lại dùng MediatR?",
         "Trả lời: Nhóm áp dụng nguyên lý Thin Controller và Clean Architecture kết hợp CQRS. Controller chỉ đóng vai trò nhận HTTP request và trả kết quả. Việc dùng MediatR (Mediator Pattern) giúp tách rời Controller khỏi tầng nghiệp vụ, mỗi chức năng là một Handler độc lập, tuân thủ nguyên lý Single Responsibility Principle (SRP) và Open/Closed Principle trong SOLID."),
        
        ("Câu 2: Tại sao mật khẩu tài khoản người dùng lại dùng thư viện BCrypt thay vì mã hóa MD5 hay SHA256?",
         "Trả lời: MD5 và SHA256 là các hàm băm nhanh, rất dễ bị giải mã bằng Rainbow Table hoặc tấn công vét cạn (brute-force) bằng GPU. Trong khi đó, BCrypt là thuật toán băm chậm (slow-hashing algorithm), tự động sinh muối (salt) ngẫu nhiên và hỗ trợ Work Factor (chi phí tính toán). Kẻ tấn công không thể đảo ngược mật khẩu ngay cả khi lộ database."),
        
        ("Câu 3: Làm thế nào server biết được ai đang gửi request Thêm/Sửa/Xóa review mà không cần truyền CustomerId từ Client?",
         "Trả lời: Hệ thống sử dụng JWT Bearer Token. Khi đăng nhập thành công, CustomerId được mã hóa vào trong Claim 'sub' của Token. Tầng API sử dụng ICurrentUserService đọc thông tin từ HttpContext.User. Nhờ vậy, Client không thể giả mạo CustomerId của người khác."),
        
        ("Câu 4: Điều kiện gì để một khách hàng có thể viết đánh giá cho dịch vụ PetCare?",
         "Trả lời: Trong CreateReviewCommandHandler đã kiểm tra 4 điều kiện nghiêm ngặt: 1) Khách hàng đã đăng nhập; 2) Đơn đặt lịch tổng thể đã thanh toán và hoàn tất (Status = Completed); 3) Dịch vụ con cụ thể đã hoàn thành (BookingItemStatus = Completed); 4) Dịch vụ này chưa từng được đánh giá trước đó.")
    ]

    for q, a in qa_list:
        add_p(q, bold_prefix="❓ ")
        add_p(a, bold_prefix="💡 ")
        doc.add_paragraph().paragraph_format.space_after = Pt(4)

    # Save document
    output_path = r"d:\KI 7\SWD392\PetCareWebsite\Giai_Thich_Code_Module_Auth_Va_Review_DoanMinhDuy.docx"
    doc.save(output_path)
    print(f"Document successfully created at: {output_path}")

if __name__ == "__main__":
    create_document()
