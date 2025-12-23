# HR Management API - Rewards & Activities

API backend cho module **Reward** và **Activity** của hệ thống quản lý nhân sự, được xây dựng bằng .NET 9.

## 📁 Kiến trúc dự án

```
hr-management-dotnet/
├── Data/                          # Database layer
│   ├── AppDbContext.cs            # Entity Framework DbContext
│   └── Migrations/                # FluentMigrator migrations
│
├── Modules/                       # Business modules (Clean Architecture)
│   ├── Reward/                    # Module quản lý điểm thưởng
│   │   ├── Application/           # Use cases, DTOs, Services
│   │   └── Domain/                # Entities, Repositories, Business logic
│   │
│   └── Activity/                  # Module quản lý hoạt động
│       ├── Application/
│       └── Domain/
│
├── Shared/                        # Shared components
│   ├── Config/                    # Configuration classes
│   └── DTOs/                      # Common DTOs (Pagination, Response, etc.)
│
├── Program.cs                     # Application entry point
├── appsettings.json               # Base configuration
├── appsettings.Development.json   # Development configuration
├── Dockerfile                     # Docker build configuration
└── docker-compose.yml             # Docker Compose orchestration
```

### Kiến trúc Module (Clean Architecture)

```
Module/
├── Application/           # Application Layer
│   └── DTOs/              # Data Transfer Objects
│
├── Domain/                # Domain Layer
│   ├── Entities/          # Domain Entities
│   ├── Dao/               # Data Access Object Interfaces
│   ├── Services/          # Business logic (Use Cases)
│   └── Interfaces/        # Service Interfaces
│
└── Infrastructure/        # Infrastructure Layer
    ├── Dao/               # Dao Implementations
    │   ├── Repositories/  # EF Repository Implementation
    │   └── *EFDao.cs      # Dao Implementation using EF Repository
    └── External/          # External service integrations (nếu có)
```

---

## 🚀 Chạy ứng dụng

### Yêu cầu
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (cho Docker)
- PostgreSQL 13+ (nếu chạy local)

### Cách 1: Docker Compose

```bash
# Khởi động API + PostgreSQL
docker compose up -d

# Xem logs
docker compose logs -f

# Dừng services
docker compose down

# Rebuild khi có thay đổi code
docker compose up -d --build
```

**URLs sau khi khởi động:**

| Service      | URL                              |
|--------------|----------------------------------|
| API          | http://localhost:5177            |
| Swagger UI   | http://localhost:5177/swagger    |
| Health Check | http://localhost:5177/health     |
| PostgreSQL   | localhost:5432                   |

**Database credentials:**
- Database: `hr_management_db`
- Username: `postgres`
- Password: `postgres123`

### Cách 2: Chạy Local (Development)

```bash
# 1. Cài đặt dependencies
dotnet restore

# 2. Cấu hình database trong appsettings.Development.json
# Đảm bảo PostgreSQL đang chạy và connection string chính xác

# 3. Chạy ứng dụng
dotnet run
```

**URLs:**
- API: http://localhost:5177
- Swagger: http://localhost:5177/swagger

---

## 🧪 Kiểm thử (Testing)

### Chạy Tests

```bash
# Chạy tất cả tests
dotnet test

# Chạy với verbose output
dotnet test --verbosity normal

# Chạy tests với coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Cấu hình Testing
- File `appsettings.Testing.json` sử dụng **SQLite in-memory** để test nhanh hơn
- Migrations tự động chạy khi khởi động

---

## 🔧 Phát triển (Development)

### Thêm Migration mới

```bash
# Tạo file migration mới trong Data/Migrations/
# Đặt tên theo format: M[YYYYMM]_[XXX]_[Description].cs
```

Ví dụ migration:

```csharp
using FluentMigrator;

namespace HrManagement.Api.Data.Migrations;

[Migration(202512009)]
public class M202512_009_CreateNewTable : Migration
{
    public override void Up()
    {
        Create.Table("new_table")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(255).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("new_table");
    }
}
```

### Thêm Entity mới

1. Tạo Entity trong `Modules/[Module]/Domain/Entities/`
2. Thêm DbSet vào `Data/AppDbContext.cs`
3. Tạo Migration cho table mới
4. Tạo Repository trong `Modules/[Module]/Domain/Repositories/`

### API Endpoint mới

1. Tạo DTO trong `Modules/[Module]/Application/DTOs/`
2. Tạo/cập nhật Service trong `Modules/[Module]/Application/Services/`
3. Tạo Controller trong `Modules/[Module]/Application/Controllers/`

---

## 📋 Các lệnh hữu ích

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run in development mode
dotnet run

# Watch mode (auto-reload)
dotnet watch run

# Clean build artifacts
dotnet clean

# Publish for production
dotnet publish -c Release
```

---

## ☁️ Deploy lên Google Cloud Run

### Environment Variables

Khi deploy lên Cloud Run qua Google Cloud Console, cần cấu hình các biến môi trường sau:

| Variable | Giá trị | Mô tả |
|----------|---------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Môi trường chạy ứng dụng |
| `DatabaseProvider` | `PostgreSQL` | Provider database |
| `ConnectionStrings__DefaultConnection` | `Host=<IP>;Database=hr_management_db;Username=<user>;Password=<pass>` | Connection string đến PostgreSQL |
| `AllowedOrigins__0` | `https://your-frontend.com` | Domain frontend được phép CORS |

> **Lưu ý**: Nếu sử dụng Cloud SQL, có thể dùng Unix socket:
> `Host=/cloudsql/PROJECT_ID:REGION:INSTANCE_NAME;Database=...`

### Cấu hình Cloud Run

| Setting | Giá trị khuyến nghị |
|---------|---------------------|
| **Port** | `5177` |
| **Memory** | `512 MiB` trở lên |
| **CPU** | `1` |
| **Min instances** | `0` (hoặc `1` để tránh cold start) |
| **Max instances** | `10` |

---

## 🔗 API Documentation

Truy cập Swagger UI tại: http://localhost:5177/swagger

Swagger cung cấp:
- Danh sách tất cả API endpoints
- Schema của request/response
- Khả năng test API trực tiếp

---

## 📝 Ghi chú

- **Database Provider**: Hỗ trợ cả PostgreSQL (production) và SQLite (testing)
- **Migrations**: Tự động chạy khi khởi động ứng dụng
- **CORS**: Cho phép tất cả origins trong development
- **Health Check**: Endpoint `/health` để kiểm tra trạng thái database
