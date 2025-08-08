# Customer Issues Manager - Enhanced Settings System

## نظرة عامة

تم تطوير نظام إعدادات محسن ومتقدم لإدارة نظام قضايا العملاء، يتضمن الميزات التالية:

## الميزات الجديدة

### 1. حفظ الإعدادات الدائم (Persistent Settings)

#### الميزات:
- **حفظ تلقائي**: جميع الإعدادات تُحفظ تلقائياً في ملف JSON
- **استعادة تلقائية**: تحميل الإعدادات المحفوظة عند بدء التطبيق
- **إعدادات افتراضية**: إعدادات آمنة في حالة عدم وجود ملف إعدادات

#### الإعدادات المدعومة:
- **إعدادات المرفقات**: وضع الإدارة، المجلد المخصص، تنظيم التاريخ
- **إعدادات الواجهة**: الوضع المظلم، الإشعارات، اللغة
- **إعدادات النسخ الاحتياطي**: النسخ التلقائي، عدد النسخ، فترة الاحتفاظ
- **إعدادات قاعدة البيانات**: مسار قاعدة البيانات

### 2. استيراد/تصدير الإعدادات (Import/Export Settings)

#### تصدير الإعدادات:
```csharp
// تصدير الإعدادات إلى ملف JSON
bool success = await settingsService.ExportSettingsAsync("settings.json");
```

#### استيراد الإعدادات:
```csharp
// استيراد الإعدادات من ملف JSON
bool success = await settingsService.ImportSettingsAsync("settings.json");
```

#### الميزات:
- **تنسيق JSON**: إعدادات قابلة للقراءة والتعديل
- **تحقق من صحة البيانات**: التأكد من صحة الإعدادات المستوردة
- **نسخ احتياطية**: إمكانية حفظ واستعادة الإعدادات
- **نقل الإعدادات**: مشاركة الإعدادات بين أجهزة مختلفة

### 3. إشعارات تغيير الإعدادات (Settings Change Notifications)

#### الإشعارات التلقائية:
- **تغيير وضع المرفقات**: إشعار عند تغيير طريقة إدارة المرفقات
- **تغيير المجلد المخصص**: إشعار عند تغيير مسار المرفقات
- **تنظيم التاريخ**: إشعار عند تفعيل/إلغاء تنظيم المرفقات حسب التاريخ

#### أنواع الإشعارات:
- **معلومات**: تغييرات عادية في الإعدادات
- **تحذير**: إعدادات قد تحتاج انتباه
- **خطأ**: أخطاء في الإعدادات
- **نجاح**: تأكيد حفظ الإعدادات

## الواجهات الجديدة

### SettingsWindow المحسنة

#### أقسام الإعدادات:
1. **استيراد/تصدير الإعدادات**
   - زر تصدير الإعدادات
   - زر استيراد الإعدادات
   - عرض حالة العملية

2. **إعدادات قاعدة البيانات**
   - تغيير مسار قاعدة البيانات
   - عرض المسار الحالي

3. **إعدادات المرفقات**
   - وضع إدارة المرفقات (الاحتفاظ الأصلي / نقل إلى مجلد)
   - اختيار مجلد مخصص
   - تنظيم حسب التاريخ

4. **إعدادات النسخ الاحتياطي**
   - النسخ التلقائي
   - عدد النسخ الاحتياطية
   - فترة الاحتفاظ
   - إنشاء واستعادة النسخ

5. **إعدادات الواجهة**
   - الوضع المظلم
   - إظهار الإشعارات

6. **إدارة التخزين**
   - معلومات التخزين
   - تنظيف الملفات

### SettingsTestWindow

#### اختبار الميزات:
- **اختبار التصدير**: تصدير الإعدادات الحالية
- **اختبار الاستيراد**: استيراد إعدادات محفوظة
- **اختبار تغيير الإعدادات**: تغيير إعدادات المرفقات
- **اختبار التحقق**: التحقق من صحة الإعدادات
- **اختبار الإعدادات غير الصحيحة**: محاكاة أخطاء في الإعدادات

## الخدمات الجديدة

### ISettingsService

```csharp
public interface ISettingsService
{
    // إعدادات المرفقات
    string GetAttachmentManagementMode();
    void SetAttachmentManagementMode(string mode);
    string GetCustomAttachmentsFolder();
    void SetCustomAttachmentsFolder(string folderPath);
    bool GetOrganizeByDate();
    void SetOrganizeByDate(bool organizeByDate);
    
    // إعدادات الواجهة
    bool GetDarkMode();
    void SetDarkMode(bool darkMode);
    bool GetShowNotifications();
    void SetShowNotifications(bool showNotifications);
    string GetLanguage();
    void SetLanguage(string language);
    
    // إعدادات النسخ الاحتياطي
    bool GetAutoBackup();
    void SetAutoBackup(bool autoBackup);
    int GetBackupCount();
    void SetBackupCount(int count);
    int GetBackupRetention();
    void SetBackupRetention(int days);
    
    // إعدادات قاعدة البيانات
    string GetDatabasePath();
    void SetDatabasePath(string path);
    
    // استيراد/تصدير
    Task<bool> ExportSettingsAsync(string filePath);
    Task<bool> ImportSettingsAsync(string filePath);
    Task<string> GetSettingsAsJsonAsync();
    Task<bool> LoadSettingsFromJsonAsync(string json);
    
    // التحقق من الصحة
    bool ValidateSettings();
    List<string> GetValidationErrors();
    
    // الأحداث
    event EventHandler<SettingsChangedEventArgs> SettingsChanged;
}
```

### SettingsService

#### الميزات الرئيسية:
- **حفظ دائم**: حفظ الإعدادات في ملف JSON
- **تحميل تلقائي**: تحميل الإعدادات عند بدء التطبيق
- **تحقق من الصحة**: التحقق من صحة الإعدادات
- **إشعارات التغيير**: إشعارات عند تغيير الإعدادات
- **إعدادات افتراضية**: إعدادات آمنة في حالة الخطأ

### SettingsManager

#### إدارة الواجهة:
- **تحميل الإعدادات**: تحميل الإعدادات إلى واجهة المستخدم
- **حفظ الإعدادات**: حفظ الإعدادات من واجهة المستخدم
- **استيراد/تصدير**: إدارة عمليات الاستيراد والتصدير
- **التحقق من الصحة**: التحقق من صحة الإعدادات قبل الحفظ

## أمثلة الاستخدام

### تصدير الإعدادات
```csharp
var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
bool success = await settingsService.ExportSettingsAsync("my_settings.json");
```

### استيراد الإعدادات
```csharp
var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
bool success = await settingsService.ImportSettingsAsync("my_settings.json");
```

### تغيير إعدادات المرفقات
```csharp
settingsService.SetAttachmentManagementMode("MoveToFolder");
settingsService.SetCustomAttachmentsFolder(@"C:\Attachments");
settingsService.SetOrganizeByDate(true);
```

### التحقق من صحة الإعدادات
```csharp
if (settingsService.ValidateSettings())
{
    // الإعدادات صحيحة
}
else
{
    var errors = settingsService.GetValidationErrors();
    // معالجة الأخطاء
}
```

## ملفات الإعدادات

### تنسيق JSON
```json
{
  "AttachmentManagementMode": "KeepOriginal",
  "CustomAttachmentsFolder": "C:\\CustomAttachments",
  "OrganizeByDate": true,
  "DarkMode": false,
  "ShowNotifications": true,
  "Language": "ar",
  "AutoBackup": true,
  "BackupCount": 10,
  "BackupRetention": 30,
  "DatabasePath": "C:\\CustomerIssues.db"
}
```

### موقع الملف
- **Windows**: `%AppData%\CustomerIssuesManager\settings.json`
- **Linux/macOS**: `~/.config/CustomerIssuesManager/settings.json`

## التحقق من الصحة

### قواعد التحقق:
- **وضع إدارة المرفقات**: مطلوب ولا يمكن أن يكون فارغاً
- **مجلد مخصص**: مطلوب عند اختيار "نقل إلى مجلد"
- **عدد النسخ الاحتياطية**: بين 1 و 100
- **فترة الاحتفاظ**: بين 1 و 365 يوم
- **مسار قاعدة البيانات**: مطلوب ولا يمكن أن يكون فارغاً

## الإشعارات

### أنواع الإشعارات:
- **معلومات**: تغييرات عادية في الإعدادات
- **نجاح**: تأكيد عمليات الحفظ والتصدير
- **تحذير**: إعدادات قد تحتاج انتباه
- **خطأ**: أخطاء في الإعدادات أو العمليات

### رسائل الإشعارات:
- "تم تغيير وضع إدارة المرفقات من 'KeepOriginal' إلى 'MoveToFolder'"
- "تم تفعيل تنظيم المرفقات حسب التاريخ"
- "تم تصدير الإعدادات بنجاح"
- "تم استيراد الإعدادات بنجاح"

## التطوير المستقبلي

### الميزات المقترحة:
1. **مزامنة الإعدادات**: مزامنة الإعدادات عبر السحابة
2. **قوالب الإعدادات**: قوالب جاهزة للإعدادات المختلفة
3. **تاريخ الإعدادات**: تتبع تغييرات الإعدادات
4. **إعدادات متقدمة**: إعدادات إضافية للمطورين
5. **تصدير/استيراد جزئي**: استيراد/تصدير إعدادات محددة فقط

## الدعم والمساعدة

### استكشاف الأخطاء:
1. **ملف الإعدادات مفقود**: سيتم إنشاء إعدادات افتراضية
2. **ملف الإعدادات تالف**: سيتم استخدام الإعدادات الافتراضية
3. **أخطاء في التحقق**: عرض قائمة بالأخطاء للمستخدم
4. **فشل في الحفظ**: عرض رسالة خطأ مع اقتراحات الحل

### الاتصال:
- **المطور**: فريق تطوير نظام قضايا العملاء
- **الدعم**: support@customerissues.com
- **التوثيق**: docs.customerissues.com 