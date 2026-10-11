-- ====================================================================================================
-- SCRIPT TẠO 4 ROLES VÀ TOÀN BỘ PERMISSIONS CHO HỆ THỐNG PETCARE BOOKING
-- Bao gồm: Admin, Receptionist, Boarding Staff, Grooming Staff
-- Đảm bảo Idempotent (Chạy nhiều lần không trùng lặp / không lỗi khóa chính)
-- ====================================================================================================

USE [PetCareBooking];
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    PRINT '>>> 1. KHỞI TẠO HOẶC CẬP NHẬT 4 ROLES...';

    -- Định nghĩa biến GUID cố định cho 4 vai trò
    DECLARE @AdminRoleId UNIQUEIDENTIFIER = 'A0000000-0000-0000-0000-000000000001';
    DECLARE @ReceptionistRoleId UNIQUEIDENTIFIER = 'A0000000-0000-0000-0000-000000000002';
    DECLARE @BoardingStaffRoleId UNIQUEIDENTIFIER = 'A0000000-0000-0000-0000-000000000003';
    DECLARE @GroomingStaffRoleId UNIQUEIDENTIFIER = 'A0000000-0000-0000-0000-000000000004';

    -- Role 1: Admin
    IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Admin')
    BEGIN
        INSERT INTO [Roles] ([Id], [RoleName], [Description], [CreatedAt], [UpdatedAt])
        VALUES (@AdminRoleId, N'Admin', N'Quản trị viên toàn hệ thống, toàn quyền quản lý', SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Role: Admin';
    END
    ELSE
    BEGIN
        SELECT @AdminRoleId = [Id] FROM [Roles] WHERE [RoleName] = N'Admin';
        PRINT '  * Da ton tai Role: Admin';
    END

    -- Role 2: Receptionist
    IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Receptionist')
    BEGIN
        INSERT INTO [Roles] ([Id], [RoleName], [Description], [CreatedAt], [UpdatedAt])
        VALUES (@ReceptionistRoleId, N'Receptionist', N'Nhân viên lễ tân: tiếp đón khách, tạo đơn tại quầy, check-in, xếp lịch', SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Role: Receptionist';
    END
    ELSE
    BEGIN
        SELECT @ReceptionistRoleId = [Id] FROM [Roles] WHERE [RoleName] = N'Receptionist';
        PRINT '  * Da ton tai Role: Receptionist';
    END

    -- Role 3: Boarding Staff
    IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Boarding Staff')
    BEGIN
        INSERT INTO [Roles] ([Id], [RoleName], [Description], [CreatedAt], [UpdatedAt])
        VALUES (@BoardingStaffRoleId, N'Boarding Staff', N'Nhân viên khách sạn: quản lý buồng phòng, ghi nhật ký chăm sóc lưu trú', SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Role: Boarding Staff';
    END
    ELSE
    BEGIN
        SELECT @BoardingStaffRoleId = [Id] FROM [Roles] WHERE [RoleName] = N'Boarding Staff';
        PRINT '  * Da ton tai Role: Boarding Staff';
    END

    -- Role 4: Grooming Staff
    IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [RoleName] = N'Grooming Staff')
    BEGIN
        INSERT INTO [Roles] ([Id], [RoleName], [Description], [CreatedAt], [UpdatedAt])
        VALUES (@GroomingStaffRoleId, N'Grooming Staff', N'Nhân viên spa làm đẹp: thực hiện ca tắm tỉa, tải ảnh kết quả làm đẹp', SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Role: Grooming Staff';
    END
    ELSE
    BEGIN
        SELECT @GroomingStaffRoleId = [Id] FROM [Roles] WHERE [RoleName] = N'Grooming Staff';
        PRINT '  * Da ton tai Role: Grooming Staff';
    END


    PRINT '>>> 2. KHỞI TẠO BẢNG QUYỀN HẠN (PERMISSIONS)...';

    -- Bảng tạm chứa danh sách chuẩn 24 permissions của hệ thống
    DECLARE @TempPermissions TABLE (
        [PermissionCode] NVARCHAR(100) PRIMARY KEY,
        [ModuleName] NVARCHAR(50),
        [PermissionName] NVARCHAR(150)
    );

    INSERT INTO @TempPermissions ([PermissionCode], [ModuleName], [PermissionName]) VALUES
    -- 1. Dashboard
    (N'DASHBOARD_VIEW', N'Dashboard', N'Xem bảng thống kê và báo cáo tổng quan'),

    -- 2. Role & Staff
    (N'ROLE_VIEW', N'Role & Permission', N'Xem danh sách vai trò và quyền hạn'),
    (N'ROLE_MANAGE', N'Role & Permission', N'Cấu hình phân quyền cho vai trò'),
    (N'STAFF_VIEW', N'Staff', N'Xem danh sách nhân viên'),
    (N'STAFF_MANAGE', N'Staff', N'Thêm, sửa, khóa tài khoản nhân viên và gán vai trò'),

    -- 3. Customer
    (N'CUSTOMER_VIEW', N'Customer', N'Xem danh sách và thông tin khách hàng'),
    (N'CUSTOMER_MANAGE', N'Customer', N'Thêm mới và chỉnh sửa thông tin khách hàng'),

    -- 4. Pet
    (N'PET_VIEW', N'Pet', N'Xem danh sách và hồ sơ thú cưng, cân nặng, sức khỏe'),
    (N'PET_MANAGE', N'Pet', N'Thêm mới, cập nhật hồ sơ và sức khỏe thú cưng'),

    -- 5. Service
    (N'SERVICE_VIEW', N'Service', N'Xem danh mục dịch vụ và bảng giá'),
    (N'SERVICE_MANAGE', N'Service', N'Thêm mới, chỉnh sửa dịch vụ và cấu hình khung giá theo cân nặng'),

    -- 6. Room
    (N'ROOM_VIEW', N'Room', N'Xem danh sách phòng và loại phòng'),
    (N'ROOM_CHECK_AVAILABILITY', N'Room', N'Kiểm tra lịch phòng trống theo ngày/tháng'),
    (N'ROOM_MANAGE', N'Room', N'Quản lý phòng (Thêm, sửa, đổi trạng thái bảo trì/sẵn sàng)'),

    -- 7. Booking
    (N'BOOKING_VIEW', N'Booking', N'Xem danh sách và chi tiết các đơn đặt lịch'),
    (N'BOOKING_CREATE', N'Booking', N'Tạo đơn đặt lịch mới tại quầy'),
    (N'BOOKING_CHECKIN', N'Booking', N'Thực hiện check-in đơn khi khách mang thú cưng đến'),
    (N'BOOKING_CANCEL', N'Booking', N'Hủy đơn đặt lịch'),
    (N'BOOKING_UPDATE_STATUS', N'Booking', N'Cập nhật tiến độ và hoàn tất đơn/dịch vụ'),
    (N'BOOKING_ASSIGN_STAFF', N'Booking', N'Phân công nhân viên thực hiện ca làm việc'),

    -- 8. Grooming
    (N'GROOMING_TASK_VIEW', N'Grooming', N'Xem danh sách ca làm việc Grooming được phân công'),
    (N'GROOMING_TASK_UPDATE', N'Grooming', N'Cập nhật tiến độ ca làm đẹp, ghi chú, tải ảnh trước/sau'),

    -- 9. Care Record
    (N'CARE_RECORD_VIEW', N'Care Record', N'Xem nhật ký chăm sóc thú cưng lưu trú'),
    (N'CARE_RECORD_CREATE', N'Care Record', N'Tạo nhật ký chăm sóc hàng ngày (Ăn uống, vệ sinh, sức khỏe)'),
    (N'CARE_RECORD_MANAGE', N'Care Record', N'Chỉnh sửa và xóa nhật ký chăm sóc'),

    -- 10. Voucher
    (N'VOUCHER_VIEW', N'Voucher', N'Xem danh sách mã giảm giá'),
    (N'VOUCHER_MANAGE', N'Voucher', N'Tạo mới và quản lý mã giảm giá');

    -- Thêm các Permission chưa có vào database
    INSERT INTO [Permissions] ([Id], [PermissionCode], [ModuleName], [PermissionName])
    SELECT NEWID(), t.[PermissionCode], t.[ModuleName], t.[PermissionName]
    FROM @TempPermissions t
    WHERE NOT EXISTS (
        SELECT 1 FROM [Permissions] p WHERE p.[PermissionCode] = t.[PermissionCode]
    );

    -- Cập nhật tên/module nếu đã tồn tại để đồng bộ
    UPDATE p
    SET p.[ModuleName] = t.[ModuleName],
        p.[PermissionName] = t.[PermissionName]
    FROM [Permissions] p
    INNER JOIN @TempPermissions t ON p.[PermissionCode] = t.[PermissionCode];

    PRINT '  + Da dong bo xong 24 Permissions.';


    PRINT '>>> 3. PHÂN QUYỀN CHO 4 VAI TRÒ (ROLE_PERMISSIONS)...';

    -- Xóa các liên kết quyền cũ của 4 role này để gán lại chuẩn xác
    DELETE FROM [RolePermissions] 
    WHERE [RoleId] IN (@AdminRoleId, @ReceptionistRoleId, @BoardingStaffRoleId, @GroomingStaffRoleId);

    -- -------------------------------------------------------------
    -- A. ROLE: ADMIN (Toàn quyền 24/24 permissions)
    -- -------------------------------------------------------------
    INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
    SELECT @AdminRoleId, p.[Id]
    FROM [Permissions] p;
    PRINT '  + Admin: Da gan toan bo quyền (100%).';

    -- -------------------------------------------------------------
    -- B. ROLE: RECEPTIONIST (Lễ tân tiếp đón & xử lý đơn tại quầy)
    -- -------------------------------------------------------------
    INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
    SELECT @ReceptionistRoleId, p.[Id]
    FROM [Permissions] p
    WHERE p.[PermissionCode] IN (
        N'DASHBOARD_VIEW',
        N'CUSTOMER_VIEW', N'CUSTOMER_MANAGE',
        N'PET_VIEW', N'PET_MANAGE',
        N'SERVICE_VIEW',
        N'ROOM_VIEW', N'ROOM_CHECK_AVAILABILITY',
        N'BOOKING_VIEW', N'BOOKING_CREATE', N'BOOKING_CHECKIN', N'BOOKING_CANCEL', N'BOOKING_UPDATE_STATUS', N'BOOKING_ASSIGN_STAFF',
        N'CARE_RECORD_VIEW',
        N'VOUCHER_VIEW'
    );
    PRINT '  + Receptionist: Da gan quyen Tiep don, Khach hang, Pet, Check-in, Dat lich tai quay.';

    -- -------------------------------------------------------------
    -- C. ROLE: BOARDING STAFF (Nhân viên khách sạn / lưu trú)
    -- -------------------------------------------------------------
    INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
    SELECT @BoardingStaffRoleId, p.[Id]
    FROM [Permissions] p
    WHERE p.[PermissionCode] IN (
        N'PET_VIEW',
        N'ROOM_VIEW', N'ROOM_CHECK_AVAILABILITY', N'ROOM_MANAGE',
        N'BOOKING_VIEW', N'BOOKING_UPDATE_STATUS',
        N'CARE_RECORD_VIEW', N'CARE_RECORD_CREATE', N'CARE_RECORD_MANAGE'
    );
    PRINT '  + Boarding Staff: Da gan quyen Phong luu tru, Nhat ky cham soc, Cap nhat don luu tru.';

    -- -------------------------------------------------------------
    -- D. ROLE: GROOMING STAFF (Nhân viên làm đẹp / spa)
    -- -------------------------------------------------------------
    INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
    SELECT @GroomingStaffRoleId, p.[Id]
    FROM [Permissions] p
    WHERE p.[PermissionCode] IN (
        N'PET_VIEW',
        N'GROOMING_TASK_VIEW', N'GROOMING_TASK_UPDATE',
        N'BOOKING_UPDATE_STATUS'
    );
    PRINT '  + Grooming Staff: Da gan quyen Ca lam dep, Hoan tat dich vu, Tai anh ket qua.';


    PRINT '>>> 4. KHỞI TẠO 4 TÀI KHOẢN NHÂN SỰ MẪU (STAFFS & STAFF_ROLES)...';

    -- Chuỗi Hash chuẩn cho mật khẩu: Admin@123
    DECLARE @PasswordHash NVARCHAR(MAX) = N'$2a$11$V9oHOV6yZenoWGzpZNZS2.M4rhu/Gultx35SO7LKt8oemSs/ez5Zm';

    -- Định nghĩa GUID cố định cho 4 nhân sự mẫu
    DECLARE @AdminStaffId UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000001';
    DECLARE @ReceptionistStaffId UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000002';
    DECLARE @BoardingStaffId UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000003';
    DECLARE @GroomingStaffId UNIQUEIDENTIFIER = 'B0000000-0000-0000-0000-000000000004';

    -- Staff 1: Admin
    IF NOT EXISTS (SELECT 1 FROM [Staffs] WHERE [Email] = 'admin@petcare.com')
    BEGIN
        INSERT INTO [Staffs] ([Id], [Email], [PasswordHash], [FullName], [PhoneNumber], [YearsOfExperience], [Status], [CreatedAt], [UpdatedAt])
        VALUES (@AdminStaffId, 'admin@petcare.com', @PasswordHash, N'Quản Trị Viên', '0901000001', 5, 0, SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Staff: admin@petcare.com';
    END
    ELSE
    BEGIN
        SELECT @AdminStaffId = [Id] FROM [Staffs] WHERE [Email] = 'admin@petcare.com';
        UPDATE [Staffs] SET [PasswordHash] = @PasswordHash, [Status] = 0 WHERE [Id] = @AdminStaffId;
        PRINT '  * Da cap nhat Staff: admin@petcare.com (Password & Status = Active)';
    END

    -- Staff 2: Receptionist
    IF NOT EXISTS (SELECT 1 FROM [Staffs] WHERE [Email] = 'receptionist@petcare.com')
    BEGIN
        INSERT INTO [Staffs] ([Id], [Email], [PasswordHash], [FullName], [PhoneNumber], [YearsOfExperience], [Status], [CreatedAt], [UpdatedAt])
        VALUES (@ReceptionistStaffId, 'receptionist@petcare.com', @PasswordHash, N'Lễ Tân Hương Mai', '0901000002', 2, 0, SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Staff: receptionist@petcare.com';
    END
    ELSE
    BEGIN
        SELECT @ReceptionistStaffId = [Id] FROM [Staffs] WHERE [Email] = 'receptionist@petcare.com';
        UPDATE [Staffs] SET [PasswordHash] = @PasswordHash, [Status] = 0 WHERE [Id] = @ReceptionistStaffId;
        PRINT '  * Da cap nhat Staff: receptionist@petcare.com (Password & Status = Active)';
    END

    -- Staff 3: Boarding Staff
    IF NOT EXISTS (SELECT 1 FROM [Staffs] WHERE [Email] = 'boarding@petcare.com')
    BEGIN
        INSERT INTO [Staffs] ([Id], [Email], [PasswordHash], [FullName], [PhoneNumber], [YearsOfExperience], [Status], [CreatedAt], [UpdatedAt])
        VALUES (@BoardingStaffId, 'boarding@petcare.com', @PasswordHash, N'Nhân Viên Trực Phòng Tuấn', '0901000003', 3, 0, SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Staff: boarding@petcare.com';
    END
    ELSE
    BEGIN
        SELECT @BoardingStaffId = [Id] FROM [Staffs] WHERE [Email] = 'boarding@petcare.com';
        UPDATE [Staffs] SET [PasswordHash] = @PasswordHash, [Status] = 0 WHERE [Id] = @BoardingStaffId;
        PRINT '  * Da cap nhat Staff: boarding@petcare.com (Password & Status = Active)';
    END

    -- Staff 4: Grooming Staff
    IF NOT EXISTS (SELECT 1 FROM [Staffs] WHERE [Email] = 'grooming@petcare.com')
    BEGIN
        INSERT INTO [Staffs] ([Id], [Email], [PasswordHash], [FullName], [PhoneNumber], [YearsOfExperience], [Status], [CreatedAt], [UpdatedAt])
        VALUES (@GroomingStaffId, 'grooming@petcare.com', @PasswordHash, N'Thợ Cắt Tỉa Hoàng Anh', '0901000004', 4, 0, SYSUTCDATETIME(), NULL);
        PRINT '  + Da tao Staff: grooming@petcare.com';
    END
    ELSE
    BEGIN
        SELECT @GroomingStaffId = [Id] FROM [Staffs] WHERE [Email] = 'grooming@petcare.com';
        UPDATE [Staffs] SET [PasswordHash] = @PasswordHash, [Status] = 0 WHERE [Id] = @GroomingStaffId;
        PRINT '  * Da cap nhat Staff: grooming@petcare.com (Password & Status = Active)';
    END

    -- Gán Staff vào Roles tương ứng
    DELETE FROM [StaffRoles] WHERE [StaffId] IN (@AdminStaffId, @ReceptionistStaffId, @BoardingStaffId, @GroomingStaffId);

    INSERT INTO [StaffRoles] ([StaffId], [RoleId]) VALUES
    (@AdminStaffId, @AdminRoleId),
    (@ReceptionistStaffId, @ReceptionistRoleId),
    (@BoardingStaffId, @BoardingStaffRoleId),
    (@GroomingStaffId, @GroomingStaffRoleId);
    PRINT '  + Da gan xong StaffRoles cho 4 nhan su.';

    COMMIT TRANSACTION;
    PRINT '>>> HOÀN TẤT SEED ROLE, PERMISSION & STAFF THÀNH CÔNG!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
GO

-- ====================================================================================================
-- KIỂM TRA LẠI KẾT QUẢ VỪA TẠO
-- ====================================================================================================
SELECT r.[RoleName], COUNT(rp.[PermissionId]) AS [TotalAssignedPermissions]
FROM [Roles] r
LEFT JOIN [RolePermissions] rp ON r.[Id] = rp.[RoleId]
GROUP BY r.[RoleName]
ORDER BY [TotalAssignedPermissions] DESC;

SELECT s.[Email], s.[FullName], r.[RoleName], 
       CASE s.[Status] WHEN 0 THEN N'Active' ELSE N'Inactive' END AS [StatusText]
FROM [Staffs] s
JOIN [StaffRoles] sr ON s.[Id] = sr.[StaffId]
JOIN [Roles] r ON sr.[RoleId] = r.[Id];
GO
