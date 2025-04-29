-- إضافة عمود Role إلى جدول Users
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Role')
BEGIN
    ALTER TABLE Users ADD Role NVARCHAR(50) DEFAULT 'User';
    PRINT 'تم إضافة عمود Role بنجاح';
END
ELSE
BEGIN
    PRINT 'عمود Role موجود بالفعل';
END

-- تحديث حقل Role للمستخدمين الحاليين إلى "User" إذا كان فارغاً
UPDATE Users SET Role = 'User' WHERE Role IS NULL;
PRINT 'تم تحديث المستخدمين بدون قيمة Role';

-- إنشاء مستخدم Admin إذا لم يكن موجوداً بالفعل
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@yardk.com')
BEGIN
    INSERT INTO Users (Name, Email, Password, Phone, Role, IsActive, CreatedAt)
    VALUES ('Admin', 'admin@yardk.com', 'Admin@123', '0777777777', 'Admin', 1, GETDATE());
    PRINT 'تم إنشاء حساب المسؤول بنجاح';
END
ELSE
BEGIN
    -- تحديث المستخدم الموجود ليصبح Admin
    UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@yardk.com';
    PRINT 'تم تحديث حساب المسؤول الموجود';
END 