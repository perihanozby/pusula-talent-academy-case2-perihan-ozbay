# Student Automation System

Pusula Talent Academy 2025 Full Stack Case Study kapsamında geliştirilen, öğrenci, öğretmen ve ders süreçlerini rol bazlı olarak yöneten full-stack öğrenci otomasyon sistemidir.

Backend, ASP.NET Core Web API ve PostgreSQL ile; kullanıcı arayüzü ise Blazor WebAssembly ile geliştirilmiştir. Uygulama JWT tabanlı kimlik doğrulama, rol bazlı yetkilendirme, Entity Framework Core migration yapısı ve Swagger/OpenAPI dokümantasyonu içerir.

## Öne Çıkan Özellikler

- JWT tabanlı kullanıcı girişi
- Admin, Teacher ve Student rolleri
- Rol bazlı endpoint yetkilendirmesi
- Öğrenci ve öğretmen yönetimi
- Ders oluşturma ve listeleme
- Öğrenciyi derse kaydetme
- Not girişi ve görüntüleme
- Devamsızlık kaydı ve takibi
- Öğretmen yorumları
- PostgreSQL üzerinde ilişkisel veri modeli
- Entity Framework Core migration desteği
- Swagger/OpenAPI ile API dokümantasyonu
- Blazor WebAssembly kullanıcı arayüzü

## Kullanılan Teknolojiler

### Backend

- .NET 9
- C#
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- PostgreSQL / Npgsql
- JWT Bearer Authentication
- BCrypt parola hashleme
- Swagger / OpenAPI

### Frontend

- Blazor WebAssembly
- Razor Components
- HTML5 ve CSS3
- Bootstrap
- HttpClient tabanlı REST API iletişimi

### Geliştirme Araçları

- Visual Studio 2022
- Git ve GitHub
- PostgreSQL 17
- Postman
- .NET User Secrets

## Proje Yapısı

```text
StudentAutomation.sln
├── StudentAutomation.Api
│   ├── Domain
│   ├── Features
│   │   ├── Auth
│   │   ├── Attendance
│   │   ├── Comments
│   │   ├── Courses
│   │   ├── Enrollments
│   │   ├── Grades
│   │   ├── Students
│   │   └── Teachers
│   ├── Infrastructure
│   └── Migrations
└── StudentAutomation.Web
    ├── Layout
    ├── Pages
    ├── Services
    └── wwwroot
```

## Veri Modeli

Sistem aşağıdaki temel varlıklardan oluşur:

- User
- Student
- Teacher
- Course
- Enrollment
- Grade
- Attendance
- Comment

Kullanıcı e-postaları benzersizdir. Öğrenci-ders kayıtlarında tekrarları önlemek amacıyla birleşik benzersiz indeks kullanılmaktadır.

## Gereksinimler

- .NET 9 SDK
- PostgreSQL 17 veya uyumlu bir PostgreSQL sürümü
- Git
- Visual Studio 2022, Visual Studio Code veya Rider

## Kurulum

### 1. Depoyu klonlayın

```bash
git clone https://github.com/perihanozby/pusula-talent-academy-case2-perihan-ozbay.git
cd pusula-talent-academy-case2-perihan-ozbay
```

### 2. PostgreSQL veritabanını oluşturun

PostgreSQL üzerinde aşağıdaki isimle boş bir veritabanı oluşturun:

```text
student_automation
```

### 3. Yerel geliştirme sırlarını tanımlayın

Bağlantı bilgileri, JWT anahtarı ve yerel admin parolası kaynak kodda tutulmaz. Değerleri `StudentAutomation.Api` projesinin .NET User Secrets alanına kaydedin:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "YOUR_POSTGRESQL_CONNECTION_STRING" --project StudentAutomation.Api
dotnet user-secrets set "Jwt:Key" "YOUR_RANDOM_JWT_KEY" --project StudentAutomation.Api
dotnet user-secrets set "SeedAdmin:Email" "YOUR_LOCAL_ADMIN_EMAIL" --project StudentAutomation.Api
dotnet user-secrets set "SeedAdmin:Password" "YOUR_STRONG_LOCAL_ADMIN_PASSWORD" --project StudentAutomation.Api
```

> Gerçek parola, bağlantı dizesi veya JWT anahtarını Git'e eklemeyin.

### 4. Paketleri geri yükleyin

```bash
dotnet restore StudentAutomation.sln
```

### 5. Migration'ları uygulayın

```bash
dotnet ef database update --project StudentAutomation.Api
```

### 6. API'yi çalıştırın

```bash
dotnet run --project StudentAutomation.Api --urls http://localhost:5001
```

Swagger arayüzü:

```text
http://localhost:5001/swagger
```

### 7. Blazor WebAssembly arayüzünü çalıştırın

Yeni bir terminal açın:

```bash
dotnet run --project StudentAutomation.Web --urls http://localhost:5232
```

Uygulama adresi:

```text
http://localhost:5232
```

## API Modülleri

| Modül | Temel işlemler |
| --- | --- |
| Auth | Kullanıcı kaydı ve JWT ile giriş |
| Students | Öğrenci listeleme, oluşturma, güncelleme ve profil görüntüleme |
| Teachers | Öğretmen listeleme, oluşturma, güncelleme ve silme |
| Courses | Ders oluşturma, listeleme ve durum güncelleme |
| Enrollments | Öğrenciyi derse kaydetme, kaydı silme ve ders öğrencilerini listeleme |
| Grades | Not oluşturma ve rol bazlı not görüntüleme |
| Attendance | Devamsızlık işaretleme ve görüntüleme |
| Comments | Öğretmen yorumu oluşturma ve öğrenci yorumlarını görüntüleme |

## Güvenlik Yaklaşımı

- Parolalar BCrypt ile hashlenir.
- API erişimi JWT Bearer tokenlarıyla korunur.
- Endpointler Admin, Teacher ve Student rollerine göre sınırlandırılır.
- Geliştirme sırları .NET User Secrets içinde tutulur.
- Bağlantı bilgileri ve JWT imzalama anahtarı repoya eklenmez.
- Kullanıcı e-postaları veri tabanı seviyesinde benzersizdir.

## Proje Durumu ve Yol Haritası

Temel kullanıcı, ders, not, devamsızlık ve kayıt akışları uygulanmıştır. Planlanan iyileştirmeler:

- Otomatik birim ve entegrasyon testleri
- Merkezi hata yönetimi ve standart API hata modeli
- FluentValidation tabanlı kapsamlı istek doğrulama
- Sayfalama, filtreleme ve sıralama
- Docker Compose ile API ve PostgreSQL kurulumu
- GitHub Actions tabanlı CI iş akışı
- Arayüz kullanılabilirliği ve erişilebilirlik iyileştirmeleri

## Geliştirici

**Perihan Özbay**
Yazılım Mühendisi / Backend Developer

- [LinkedIn](https://www.linkedin.com/in/perihanozbay)
- [GitHub](https://github.com/perihanozby)
