-- Create ProductImages Table
CREATE TABLE ProductImages (
    ID int IDENTITY(1,1) PRIMARY KEY,
    ProductID int NOT NULL,
    ImageUrl nvarchar(max) NOT NULL,
    IsMainImage bit NOT NULL DEFAULT 0,
    DisplayOrder int NOT NULL DEFAULT 0,
    CreatedAt datetime DEFAULT GETDATE(),
    CONSTRAINT FK__ProductImages__ProductID__123456 FOREIGN KEY (ProductID)
        REFERENCES Products(ID) ON DELETE CASCADE
);

-- Create Index
CREATE INDEX IX_ProductImages_ProductID ON ProductImages(ProductID);

-- Verify table creation
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductImages'; 