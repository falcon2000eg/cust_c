using CustomerIssuesManager.Core.Data;
using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfApplication = System.Windows.Application;

namespace CustomerIssuesManager;

public partial class App : WpfApplication
{
    private readonly IServiceProvider _serviceProvider;
    public static Employee? CurrentUser { get; private set; }
    public IServiceProvider Services => _serviceProvider;

public App()
{
    // Prevent app from closing when windows are closed unless explicitly called
    this.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
    
    // Add global exception handling
    this.DispatcherUnhandledException += App_DispatcherUnhandledException;
    WpfApplication.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
    
    IServiceCollection services = new ServiceCollection();

    // Register Configuration Service first
    services.AddSingleton<IConfigurationService, ConfigurationService>();
    services.AddSingleton<ILoggingService, LoggingService>();

    // Register DbContext and configure the provider
    services.AddDbContext<AppDbContext>(options => 
        options.UseSqlite("Data Source=CustomerIssues.db"));

    // Register Services
    services.AddScoped<ICaseService, CaseService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IEmployeeService, EmployeeService>();
    services.AddScoped<IBackupService, BackupService>();
    services.AddScoped<IPrintService, PrintService>();

    // Register Settings Service as Singleton for better performance
    services.AddSingleton<ISettingsService, SettingsService>();

    // Register File Manager as Singleton for better performance
    services.AddSingleton<IFileManagerService, FileManagerService>();



    // Register Windows with factories to resolve dependencies
    services.AddTransient<MainWindow>(provider =>
        new MainWindow(
            provider.GetRequiredService<ICaseService>(),
            provider
        ));
    services.AddTransient<LoginWindow>(provider =>
        new LoginWindow(
            provider,
            provider.GetRequiredService<IAuthService>()
        ));
    services.AddTransient<CaseDetailWindow>();
    services.AddTransient<DashboardWindow>();
    services.AddTransient<ManageEmployeesWindow>();
    services.AddTransient<SettingsWindow>(provider =>
        new SettingsWindow(
            provider.GetRequiredService<IBackupService>(),
            provider.GetRequiredService<IFileManagerService>(),
            provider.GetRequiredService<ISettingsService>()
        ));
    services.AddTransient<AdvancedSearchWindow>();
    services.AddTransient<PrintPreviewWindow>();
    services.AddTransient<CorrespondenceDialog>();
    services.AddTransient<SplashScreen>();
    services.AddTransient<DiagnosticWindow>();

    _serviceProvider = services.BuildServiceProvider();
}

private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
{
    try
    {
        // Log the exception
        System.Diagnostics.Debug.WriteLine($"Unhandled Exception: {e.Exception.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack Trace: {e.Exception.StackTrace}");
        
        // Try to log to file if logging service is available
        try
        {
            var loggingService = _serviceProvider.GetRequiredService<ILoggingService>();
            loggingService.LogError("Unhandled exception occurred", e.Exception);
        }
        catch
        {
            // If logging fails, just continue
        }
        
        // Show user-friendly error message
        System.Windows.MessageBox.Show(
            $"حدث خطأ غير متوقع:\n{e.Exception.Message}\n\nسيتم إغلاق التطبيق.",
            "خطأ في النظام",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error);
        
        // Mark as handled to prevent app crash
        e.Handled = true;
        
        // Shutdown the application
        this.Shutdown();
    }
    catch (Exception ex)
    {
        // If error handling itself fails, just show basic message
        System.Windows.MessageBox.Show(
            "حدث خطأ خطير في النظام. سيتم إغلاق التطبيق.",
            "خطأ خطير",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error);
        this.Shutdown();
    }
}

    protected override async void OnStartup(StartupEventArgs e)
    {

        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        try
        {
            base.OnStartup(e);

            var loggingService = _serviceProvider.GetRequiredService<ILoggingService>();
            var configurationService = _serviceProvider.GetRequiredService<IConfigurationService>();

            loggingService.LogInformation("Application starting...");

            var splash = _serviceProvider.GetRequiredService<SplashScreen>();
            splash.Show();

            try
            {
                splash.SetStatus("جاري إنشاء نسخة احتياطية...");
                var backupService = _serviceProvider.GetRequiredService<IBackupService>();
                await backupService.CreateBackupAsync();
                backupService.CleanupOldBackups();
                loggingService.LogInformation("Backup completed successfully");
            }
            catch (Exception ex)
            {
                loggingService.LogError("Failed to create backup", ex);
                // Continue startup even if backup fails
            }

            try
            {
                splash.SetStatus("جاري تهيئة قاعدة البيانات...");
                // Ensure database is created
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await context.Database.EnsureCreatedAsync();
                }
                
                // Database initialization completed
                
                loggingService.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                loggingService.LogError("Failed to initialize database", ex);
                System.Windows.MessageBox.Show(
                    "فشل في تهيئة قاعدة البيانات. يرجى التحقق من الصلاحيات وإعادة تشغيل التطبيق.",
                    "خطأ في قاعدة البيانات",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                Shutdown();
                return;
            }

            splash.SetStatus("جاهز لبدء التشغيل...");
            await Task.Delay(2000); // Keep splash screen for a bit longer

            splash.Close();

            try
            {
                loggingService.LogInformation("Creating LoginWindow...");
                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loggingService.LogInformation("LoginWindow created successfully");
                
                loggingService.LogInformation("Showing LoginWindow...");
                loginWindow.Show();
                loggingService.LogInformation("LoginWindow shown successfully");

                loggingService.LogInformation("Application started successfully");
            }
            catch (Exception ex)
            {
                loggingService.LogCritical($"Failed to create or show LoginWindow: {ex.Message}", ex);
                loggingService.LogCritical($"Stack trace: {ex.StackTrace}", ex);
                
                System.Windows.MessageBox.Show(
                    $"حدث خطأ أثناء إنشاء نافذة تسجيل الدخول:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "خطأ في إنشاء النافذة",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                
                Shutdown();
                return;
            }
        }
        catch (Exception ex)
        {
            try
            {
                var loggingService = _serviceProvider.GetRequiredService<ILoggingService>();
                loggingService.LogCritical("Application failed to start", ex);
            }
            catch
            {
                // If logging fails, at least show the error
                System.Diagnostics.Debug.WriteLine($"Critical error during startup: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            System.Windows.MessageBox.Show(
                $"حدث خطأ أثناء بدء التطبيق:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "خطأ في بدء التطبيق",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            var loggingService = _serviceProvider.GetRequiredService<ILoggingService>();
            var backupService = _serviceProvider.GetRequiredService<IBackupService>();
            
            loggingService.LogInformation("Application shutting down...");
            
            // Perform a final backup on exit
            await backupService.CreateBackupAsync();
            
            loggingService.LogInformation("Application shutdown completed");
        }
        catch (Exception ex)
        {
            // Log but don't prevent shutdown
            System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex.Message}");
        }

        base.OnExit(e);
    }

    public static void SetCurrentUser(Employee employee)
    {
        CurrentUser = employee;
    }
}
