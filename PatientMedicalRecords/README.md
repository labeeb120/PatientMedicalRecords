# نظام السجل الطبي للمريض
## Patient Medical Records System

### نظرة عامة
نظام السجل الطبي للمريض هو مشروع تخرج لطالب بكلية الهندسة قسم تقنية المعلومات. يهدف النظام إلى توفير حل متكامل لإدارة السجلات الطبية للمرضى مع إمكانية مشاركة آمنة للبيانات الطبية بين مقدمي الرعاية الصحية.

### المميزات الرئيسية

#### 1. إدارة المستخدمين
- **المرضى**: إنشاء وإدارة الملف الطبي الشخصي
- **الأطباء**: البحث عن المرضى وإضافة التشخيصات والوصفات
- **الصيادلة**: صرف الأدوية وفحص التفاعلات الدوائية
- **الإدمن**: إدارة النظام والموافقة على الطلبات

#### 2. نظام المصادقة والتفويض
- تسجيل الدخول باستخدام الرقم الوطني وكلمة المرور
- نظام JWT للتوكنات الآمنة
- تفويض الأدوار (Role-based Authorization)
- تشفير كلمات المرور باستخدام BCrypt

#### 3. نظام QR Code
- توليد رموز QR آمنة للمشاركة
- صلاحية محدودة (5 دقائق)
- نظام موافقة المريض على الوصول
- تسجيل جميع محاولات الوصول

#### 4. إدارة الملف الطبي
- البيانات الشخصية (الاسم، تاريخ الميلاد، الجنس، فصيلة الدم)
- الأمراض المزمنة والحساسات
- العمليات الجراحية السابقة
- السجلات الطبية والوصفات

#### 5. نظام فحص التفاعلات الدوائية
- قاعدة بيانات شاملة للتفاعلات الدوائية
- فحص تلقائي قبل صرف الأدوية
- تحذيرات واضحة للتفاعلات الخطيرة
- اقتراحات بديلة للأدوية

#### 6. شاشة الطوارئ
- عرض المعلومات الحيوية على شاشة القفل
- QR Code للوصول السريع
- معلومات الحساسات والأمراض المزمنة
- أرقام الطوارئ

### التقنيات المستخدمة

#### Backend
- **ASP.NET Core 8.0** - إطار العمل الرئيسي
- **Entity Framework Core** - ORM لإدارة قاعدة البيانات
- **SQL Server** - قاعدة البيانات
- **JWT Bearer Authentication** - نظام المصادقة
- **BCrypt** - تشفير كلمات المرور
- **QRCoder** - توليد رموز QR

#### Frontend (مستقبلي)
- **Flutter** - تطبيق الهاتف المحمول
- **Material Design** - واجهة المستخدم

### هيكل المشروع

```
PatientMedicalRecords/
├── Controllers/           # Controllers للواجهات البرمجية
│   ├── AuthController.cs
│   ├── PatientController.cs
│   ├── DoctorController.cs
│   ├── PharmacistController.cs
│   ├── AdminController.cs
│   └── QRCodeController.cs
├── Data/                  # طبقة البيانات
│   └── MedicalRecordsDbContext.cs
├── DTOs/                  # نماذج نقل البيانات
│   ├── AuthDTOs.cs
│   ├── PatientDTOs.cs
│   ├── DoctorDTOs.cs
│   ├── PharmacistDTOs.cs
│   └── AdminDTOs.cs
├── Models/                # نماذج البيانات
│   └── AllModels.cs
├── Services/              # الخدمات
│   ├── AuthService.cs
│   ├── QRCodeService.cs
│   └── DrugInteractionService.cs
├── Program.cs             # نقطة البداية
├── appsettings.json       # إعدادات التطبيق
└── README.md             # هذا الملف
```

### قاعدة البيانات

#### الجداول الرئيسية
- **Users** - المستخدمون الأساسيون
- **Patients** - بيانات المرضى
- **Doctors** - بيانات الأطباء
- **Pharmacists** - بيانات الصيادلة
- **MedicalRecords** - السجلات الطبية
- **Prescriptions** - الوصفات الطبية
- **PrescriptionItems** - عناصر الوصفات
- **Allergies** - الحساسات
- **ChronicDiseases** - الأمراض المزمنة
- **Surgeries** - العمليات الجراحية
- **AccessTokens** - رموز الوصول
- **AuditLogs** - سجل التدقيق

### كيفية التشغيل

#### المتطلبات
- Visual Studio 2022 أو أحدث
- .NET 8.0 SDK
- SQL Server 2019 أو أحدث
- SQL Server Management Studio (اختياري)

#### خطوات التشغيل

1. **استنساخ المشروع**
   ```bash
   git clone [repository-url]
   cd PatientMedicalRecords
   ```

2. **تثبيت الحزم**
   ```bash
   dotnet restore
   ```

3. **تحديث سلسلة الاتصال**
   - افتح `appsettings.json`
   - حدث `ConnectionStrings.DefaultConnection` ليشير إلى قاعدة البيانات الخاصة بك

4. **إنشاء قاعدة البيانات**
   ```bash
   dotnet ef database update
   ```

5. **تشغيل المشروع**
   ```bash
   dotnet run
   ```

6. **الوصول إلى Swagger UI**
   - افتح المتصفح واذهب إلى `https://localhost:7000`
   - ستجد واجهة Swagger UI مع جميع الـ APIs

### استخدام النظام

#### 1. تسجيل الدخول
```http
POST /api/auth/login
Content-Type: application/json

{
  "nationalId": "1234567890",
  "password": "password123",
  "role": 1
}
```

#### 2. إنشاء حساب جديد
```http
POST /api/auth/register
Content-Type: application/json

{
  "nationalId": "1234567890",
  "password": "password123",
  "confirmPassword": "password123",
  "role": 1
}
```

#### 3. توليد QR Code
```http
POST /api/patient/generate-qr
Authorization: Bearer [token]
```

#### 4. البحث عن مريض
```http
POST /api/doctor/search-patient
Authorization: Bearer [token]
Content-Type: application/json

{
  "nationalId": "1234567890"
}
```

### الأمان

#### 1. تشفير البيانات
- كلمات المرور مشفرة باستخدام BCrypt
- البيانات الحساسة محمية في قاعدة البيانات
- استخدام HTTPS لجميع الاتصالات

#### 2. التحكم في الوصول
- نظام JWT للتوكنات الآمنة
- تفويض الأدوار على مستوى الـ API
- تسجيل جميع العمليات في سجل التدقيق

#### 3. حماية البيانات الطبية
- رموز QR محدودة الصلاحية (5 دقائق)
- نظام موافقة المريض على الوصول
- تشفير البيانات الحساسة

### نقاط القوة

1. **الأمان العالي**: نظام مصادقة متقدم وحماية شاملة للبيانات
2. **سهولة الاستخدام**: واجهة بسيطة وواضحة
3. **المرونة**: قابل للتطوير والتوسع
4. **التوافق**: يعمل على جميع المنصات
5. **الموثوقية**: نظام احتياطي وسجل تدقيق شامل

### نقاط الضعف والتحسينات المستقبلية

#### نقاط الضعف الحالية
1. **عدم وجود تطبيق الهاتف المحمول**: النظام يعمل فقط كـ API
2. **عدم وجود نظام إشعارات متقدم**: الإشعارات محدودة
3. **عدم وجود نظام نسخ احتياطي تلقائي**: النسخ الاحتياطي يدوي
4. **عدم وجود نظام تحليل البيانات**: لا توجد تقارير متقدمة

#### التحسينات المستقبلية
1. **تطبيق Flutter**: تطبيق هاتف محمول كامل
2. **نظام إشعارات متقدم**: إشعارات فورية وpush notifications
3. **تحليل البيانات**: تقارير وإحصائيات متقدمة
4. **التكامل مع الأنظمة الأخرى**: ربط مع المستشفيات والصيدليات
5. **الذكاء الاصطناعي**: تحليل البيانات الطبية وتقديم التوصيات

### الاختبار

#### اختبار Postman
تم إنشاء مجموعة Postman شاملة لاختبار جميع الـ APIs:

1. **استيراد المجموعة**
   - افتح Postman
   - استورد ملف `PatientMedicalRecords.postman_collection.json`

2. **إعداد البيئة**
   - أنشئ بيئة جديدة
   - أضف متغير `baseUrl` = `https://localhost:7000`

3. **تشغيل الاختبارات**
   - ابدأ بتسجيل الدخول
   - استخدم التوكن في باقي الطلبات

#### اختبار الوحدة
```bash
dotnet test
```

### المساهمة

1. Fork المشروع
2. أنشئ فرع للميزة الجديدة (`git checkout -b feature/AmazingFeature`)
3. Commit التغييرات (`git commit -m 'Add some AmazingFeature'`)
4. Push إلى الفرع (`git push origin feature/AmazingFeature`)
5. افتح Pull Request

### الترخيص

هذا المشروع مرخص تحت رخصة MIT - انظر ملف [LICENSE](LICENSE) للتفاصيل.

### جهات الاتصال

- **المطور**: [اسم المطور]
- **البريد الإلكتروني**: [email@example.com]
- **الجامعة**: [اسم الجامعة]
- **القسم**: تقنية المعلومات

### شكر وتقدير

- شكر خاص لجميع المساهمين في المشروع
- شكر لـ Microsoft على توفير ASP.NET Core
- شكر لجميع المطورين الذين ساهموا في المكتبات المستخدمة

---

**ملاحظة**: هذا المشروع هو مشروع تخرج تعليمي وليس للاستخدام التجاري بدون إذن صريح.

CREATE TABLE Ingredients (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    NormalizedName NVARCHAR(200) NOT NULL
);

CREATE TABLE Medications (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(250) NOT NULL,
    NormalizedName NVARCHAR(250) NOT NULL
);

CREATE TABLE MedicationIngredients (
    MedicationId INT NOT NULL,
    IngredientId INT NOT NULL,
    PRIMARY KEY (MedicationId, IngredientId),
    FOREIGN KEY (MedicationId) REFERENCES Medications(Id),
    FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id)
);

CREATE TABLE IngredientInteractions (
    Id INT IDENTITY PRIMARY KEY,
    IngredientAId INT NOT NULL,
    IngredientBId INT NOT NULL,
    Severity NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    Recommendation NVARCHAR(MAX),
    CONSTRAINT UQ_IngredientPair UNIQUE (IngredientAId, IngredientBId),
    FOREIGN KEY (IngredientAId) REFERENCES Ingredients(Id),
    FOREIGN KEY (IngredientBId) REFERENCES Ingredients(Id)
);

CREATE INDEX IX_Ingredients_NormalizedName ON Ingredients(NormalizedName);
CREATE INDEX IX_Medications_NormalizedName ON Medications(NormalizedName);
