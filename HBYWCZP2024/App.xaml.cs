global using LanguageExt;
global using static LanguageExt.FSharp;
global using static LanguageExt.Prelude;
using HBYWCZP2024.Data;
using HBYWCZP2024.Jobs;
using HBYWCZP2024.Mediator.Behaviors;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.PLC;
using HBYWCZP2024.PLC.Models;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using HBYWCZP2024.View;
using HBYWCZP2024.View.CZPallet;
using HBYWCZP2024.View.User;
using HBYWCZP2024.ViewModel;
using HBYWCZP2024.ViewModel.CZPallet;
using LinxUniverse.Auth;
using LinxUniverse.DI;
using LinxUniverse.OPCUA.Extenisons;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;


namespace HBYWCZP2024
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IHost Host
        {
            get;
        }

        public static T GetService<T>()
            where T : class
        {

            System.Diagnostics.Debug.WriteLine("----------------------------");
            System.Diagnostics.Debug.WriteLine(typeof(T).ToString());
            //var a = typeof(T);
            if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }

        public App()
        {
            string MName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
            string PName = System.IO.Path.GetFileNameWithoutExtension(MName);
            System.Diagnostics.Process[] myProcess = System.Diagnostics.Process.GetProcessesByName(PName);

            if (myProcess.Length > 1)
            {
                MessageBox.Show("本程序一次只能运行一个实例！", "提示");
                Application.Current.Shutdown();
                return;
            }
            //DB10B dB10B  = new();
            //DB10 dB10  = new();
            //var cc = dB10B.GetType().GetProperties();
            //var cc1 = dB10.GetType().GetProperties();
            MainStorage.Init();
            //DB.Database.Migrate();
            Host = Microsoft.Extensions.Hosting.Host.
                     CreateDefaultBuilder().
                     UseContentRoot(AppContext.BaseDirectory).
                     ConfigureServices((context, services) =>
                     {
                         //services.AddSingleton<ServoManagerViewModel>();
                         //services.AddTransient<ServoManagerView>();

                         services.AddHttpClient();
                         services.AddQuartz(config =>
                         {
                             JobKey jobKey = JobKey.Create(nameof(DailyJobs));
                             config.AddJob<DailyJobs>(jobKey).AddTrigger(trigger =>
                             {
                                 trigger.ForJob(jobKey).WithCronSchedule("10 0 0 * * ?");
                             });
                             JobKey jobKey1 = JobKey.Create(nameof(SaveJobs));

                             config.AddJob<SaveJobs>(jobKey1).AddTrigger(trigger =>
                             {
                                 trigger.ForJob(jobKey1).WithCronSchedule("0 */5 * * * ?");
                             });

                             JobKey jobKey2 = JobKey.Create(nameof(EquipStatusReportJobs));
                             config.AddJob<EquipStatusReportJobs>(jobKey2).AddTrigger(trigger =>
                             {
                                 trigger.ForJob(jobKey2).WithCronSchedule("0 * * * * ?");
                             });

                             //JobKey jobKey2 = JobKey.Create(nameof(StatusJobs));
                             //config.AddJob<StatusJobs>(jobKey2).AddTrigger(trigger =>
                             //{
                             //    trigger.ForJob(jobKey2).WithCronSchedule("0 * * * * ?");
                             //});

                         });
                         services.AddQuartzHostedService(config =>
                         {
                             config.WaitForJobsToComplete = true;
                         });

                         services.AddSingleton<LinxAuthenticationStateProvider, StandLinxAuthenticationStateProvider<LinxUser>>();

                         services.AddAuth<LinxUser>();

                         services.AddSingleton<MainViewModel>();
                         services.AddTransient<MainWindow>();


                         services.AddSingleton<SettingViewModel>();
                         services.AddTransient<SettingView>();

                         services.AddSingleton<UserManagerViewModel>();
                         services.AddTransient<UserManagerView>();

                         services.AddTransient<WCSLogViewModel>();
                         services.AddTransient<WCSLogView>();

                         services.AddTransient<YWStageView>();


                         services.AddTransient<WarningLogView>();
                         services.AddSingleton<WarningLogViewModel>();

                         services.AddSingleton<UserManagerViewModel>();
                         services.AddTransient<UserManagerView>();

                         services.AddTransient<LogDashBoardView>();


                         services.AddSingleton<RichTextBox>();
                         services.AddSingleton<CacheService>();

                         services.AddTransient<PalletChaiLogView>();
                         services.AddTransient<PalletChaiLogViewModel>();

                         services.AddTransient<PalletLogView>();
                         services.AddTransient<PalletLogViewModel>();
                         services.AddSingleton<CacheService>();

                         services.AddTransient<NgBeltCellLogView>();
                         services.AddTransient<NgBeltCellLogViewModel>();

                         services.AddTransient<PowerSettingWindows>();

                         services.AddTransient<StationSettingView>();
                         services.AddTransient<PalletRuleEditView>();


                         services.AddSingleton<IHBECSServiceFactory, HBECSServiceFactory>();
                         services.AddOPCPLCContext<HBYWPLCContext>();
                         services.AddOPCPLCTaskFactory<HBYWPLCContext>();
                         //services.AddPLCTaskFactory();

                         //services.AddDbContext< LinxUserDBContext, LinxContext >(option =>
                         //{
                         //    option.UseSqlServer(context.Configuration.GetConnectionString("LinxContextConnection"));
                         //}, ServiceLifetime.Singleton, ServiceLifetime.Singleton);

                         services.AddDbContext<LinxUserDBContext<LinxUser>, LinxContext>(option =>
                         {
                             option.UseSqlServer(context.Configuration.GetConnectionString("LinxContextConnection"));
                         }, ServiceLifetime.Transient, ServiceLifetime.Transient);

                         services.AddMediatR(cfg =>
                         {
                             cfg.RegisterServicesFromAssemblyContaining<App>();

                         });

                         services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                         //services.AddSwaggerGen(
                         //);
                         //services.AddPLCTaskFactory();

                     }).ConfigureLogging(logging =>
                     {
                         logging.ClearProviders();
                         logging.AddSerilog(dispose: true);
                         //logging.AddConsole();
                         //logging.AddDebug();
                     })
                     //.ConfigureWebHostDefaults(webHostBuilder =>
                     //{

                     //    webHostBuilder.UseStartup<Startup>();
                     //    webHostBuilder.UseUrls($"http://*:{MainViewModel.Saves.Port}");
                     //    webHostBuilder.ConfigureKestrel((context, options) =>
                     //    {
                     //        // Handle requests up to 50 MB
                     //        options.Limits.MaxRequestBodySize = 16_000_000_000_000_000;
                     //        options.Limits.KeepAliveTimeout = TimeSpan.FromHours(10);
                     //    });

                     //})
                     .ConfigureServices(services =>
                     {
                         services.Configure<FormOptions>(options =>
                         {
                             // Set the limit to 256 MB
                             options.MultipartBodyLengthLimit = 2684354560;
                         });
                     }).
                     Build();
            InitContext();
            var dd = MainStorage.Saves.BatteryLevelList.Any(s => s.Contains("Xn1y"));
            var data = CZPUtils.GetLevelFromMes("WGNG");

            //var a = new S105B() { BatteryBarcodes = Enumerable.Range(0, 24).Select(s => ($"Test{s}", "AA")).ToList(),
            //    BatteryCount = 24, TrayNumber = "213123", TrayPosition = 1, TrayType = TrayType.Engineering };

             var a = new S105B() { BatteryBarcodes = Enumerable.Range(0, 24).Select(s => ($"Test{s}", "AA")).ToList(),
                BatteryCount = 24, TrayNumber = "213123", TrayPosition = 1, TrayType = TrayType.空托盘 };

            var b = MessageHelper.GetSocketMessage(a.Type, "413", a.GetContent());
            //Host.UseSwagger();
            //Host.UseSwaggerUI(options => {
            //    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            //    options.RoutePrefix = string.Empty;
            //});
            RichTextBox aa = GetService<RichTextBox>();
            aa.IsReadOnly = true;
            aa.Background = System.Windows.Media.Brushes.Black;
            aa.TextChanged += Aa_TextChanged;
            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
                            .MinimumLevel.Override("Quartz", Serilog.Events.LogEventLevel.Information)
                            .MinimumLevel.Override("Microsoft.Extensions.Http", Serilog.Events.LogEventLevel.Information)
              // 软件名字
              // 软件名字
              .WriteTo.File("logs/HBYWCZP2024.NET.log", rollingInterval: RollingInterval.Day,
              outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] |{SourceContext}.{Method}| - {Message}{NewLine}{Exception}")
              .WriteTo.RichTextBox(GetService<RichTextBox>()).Enrich.FromLogContext()
              .CreateLogger();


            var logger = Host.Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation("Host created.");

            Host.Start();
        }
        private static void InitContext()
        {
            LinxContext DB = GetService<LinxContext>();
            // DB.Database.EnsureCreated();
            DB.Database.Migrate();
            //
            //foreach (var batteryInfo in DB.BatteryInfos.ToList())
            //{
            //    batteryInfo.Param = new();
            //}

            //DB.SaveChanges();
        }
        private void Aa_TextChanged(object sender, TextChangedEventArgs e)
        {
            var rb = (sender as RichTextBox);
            if (e.Changes.First().AddedLength < 5) return;
            var MaxLines = 100;
            //rb.Document.Blocks.LastBlock.
            // 获得rb的行数
            EndBlock(rb);
            var lines = rb.Document.Blocks.Count;
            if (lines > MaxLines)
            {
                // 获得第一行的TextPointer
                // var start = rb.Document.Blocks.FirstBlock.ContentStart;
                // 删除第一行到第MaxLines行
                rb.Document.Blocks.Remove(rb.Document.Blocks.FirstBlock);
            }
            //if (rb.)
        }
        private void EndBlock(RichTextBox richTextBox)
        {
            var paragraph = new Paragraph();
            richTextBox.Document.Blocks.Add(paragraph);
            richTextBox.CaretPosition = paragraph.ContentEnd;
            richTextBox.ScrollToEnd();
            //  richTextBox.Document.Blocks.Remove(paragraph);
            //var position = richTextBox.CaretPosition.GetLineStartPosition(-1);
            //if (position != null)
            //{
            //    richTextBox.CaretPosition = position;
            //}
        }
    }

}
