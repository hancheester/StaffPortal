using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StaffPortal.Service;
using StaffPortal.Service.Departments;
using StaffPortal.Service.Errors;
using StaffPortal.Service.Events;
using StaffPortal.Service.Leave;
using StaffPortal.Service.Menu;
using StaffPortal.Service.Message;
using StaffPortal.Service.Permissions;
using StaffPortal.Service.Resource;
using StaffPortal.Service.Roles;
using StaffPortal.Service.Staff;
using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using StaffPortal.Common.Settings;
using StaffPortal.Common.ViewModels;
using StaffPortal.Data;
using StaffPortal.Data.Utility;
using StaffPortal.Web.Infrastructure;
using StaffPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StaffPortal.Service.Configuration;
using System.ComponentModel;
using StaffPortal.Service.Shift;

namespace StaffPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.EnableSensitiveDataLogging()
                                  .UseSqlServer(Configuration["DbConnectionString"]),
                ServiceLifetime.Scoped);

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            });

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<AuditFilter>();
            services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, CustomUserClaimsPrincipalFactory>();

            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddTransient<IBusinessRoleService, BusinessRoleService>();
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<ILeaveTypeService, LeaveTypeService>();
            services.AddTransient<IEmployeeService, EmployeeService>();
            services.AddTransient<IWorkingDaysService, WorkingDaysService>();
            services.AddTransient<ILeaveService, LeaveService>();
            services.AddTransient<IErrorService, ErrorService>();
            services.AddTransient<IResourcesService, ResourcesService>();
            services.AddTransient<IAttendanceService, AttendanceService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IMenuService, MenuService>();
            services.AddTransient<ISettingService, SettingService>();
            services.AddTransient<IShiftService, ShiftService>();
            
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddTransient<IEventPublisher, EventPublisher>();
                        
            var assembliesToScan = Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(Assembly.Load);
            AddAutoMapperClasses(services, assembliesToScan);
            AddEventConsumerClasses(services, assembliesToScan);
            AddSettingClasses(services, assembliesToScan);

            services.AddLogging();
            services.AddSession();
            services.AddAntiforgery(opts => opts.HeaderName = "XSRF-TOKEN");
            services.AddMemoryCache();
            services.AddMvc();
            services.AddAutoMapper();

            TypeDescriptor.AddAttributes(typeof(List<string>), new TypeConverterAttribute(typeof(GenericListTypeConverter<string>)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            IDbInitializer dbInitializer,
            ILoggerFactory factory)
        {
            dbInitializer.Initialize();

            if (env.IsEnvironment("Development") || env.IsEnvironment("Staging"))
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
                factory.AddDebug(LogLevel.Information);

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                factory.AddDebug(LogLevel.Error);
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseSession();

            app.UseMvc(ConfigureRoute);
        }

        public void ConfigureRoute(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute(
                    name: "Dashboard",
                    template: "dashboard",
                    defaults: new { controller = "Home", action = "Index" });

            routeBuilder.MapRoute(
                    name: "Settings",
                    template: "settings",
                    defaults: new { controller = "System", action = "Settings" });

            routeBuilder.MapRoute(
                    name: "Departments",
                    template: "departments",
                    defaults: new { controller = "Manage", action = "Departments" });

            routeBuilder.MapRoute(
                    name: "Business Roles",
                    template: "roles",
                    defaults: new { controller = "Manage", action = "BusinessRoles" });

            routeBuilder.MapRoute(
                    name: "Employees",
                    template: "employees",
                    defaults: new { controller = "Manage", action = "Employees" });

            routeBuilder.MapRoute(
                    name: "My Leaves",
                    template: "my-leaves",
                    defaults: new { controller = "Manage", action = "MyLeaves" });

            routeBuilder.MapRoute(
                    name: "My Account",
                    template: "my-account",
                    defaults: new { controller = "Manage", action = "MyAccount" });

            routeBuilder.MapRoute(
                    name: "Pending Leave Request",
                    template: "pending-leave-requests",
                    defaults: new { controller = "Manage", action = "PendingLeaveRequest" });

            routeBuilder.MapRoute(
                    name: "Edit Staff Shifts",
                    template: "edit-shifts",
                    defaults: new { controller = "Manage", action = "EditStaffShifts" });

            routeBuilder.MapRoute(
                    name: "Login",
                    template: "login",
                    defaults: new { controller = "Account", action = "Login" });

            routeBuilder.MapRoute(
                name: "Register",
                template: "registration",
                defaults: new { controller = "Account", action = "Register" });

            routeBuilder.MapRoute(
                name: "ForgotPassword",
                template: "forgot_password",
                defaults: new { controller = "Account", action = "ForgotPassword" });

            routeBuilder.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        }

        public void AddEventConsumerClasses(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {        
            assemblies = assemblies as Assembly[] ?? assemblies.ToArray();
            var assignTypeFrom = typeof(IConsumer<>);
            var allTypes = assemblies.SelectMany(a => a.ExportedTypes).ToArray();
            var consumers = allTypes
                .Where(x => !assignTypeFrom.IsAssignableFrom(x.GetTypeInfo()) && DoesTypeImplementOpenGeneric(x, assignTypeFrom)) 
                .Where(x => x.IsClass == true)
                .Where(x => x.IsAbstract == false)
                .ToList();

            foreach (var consumer in consumers)
            {
                var foundInterfaces = consumer.FindInterfaces((type, criteria) =>
                {
                    var isMatch = type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                    return isMatch;
                }, typeof(IConsumer<>));

                foreach (var foundInterface in foundInterfaces)
                {
                    services.AddTransient(foundInterface, consumer);
                }
            }            
        }

        public void AddSettingClasses(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            assemblies = assemblies as Assembly[] ?? assemblies.ToArray();
            var assignTypeFrom = typeof(ISettings);
            var allTypes = assemblies.SelectMany(a => a.ExportedTypes).ToArray();
            var settings = allTypes
                .Where(x => assignTypeFrom.IsAssignableFrom(x))
                .Where(x => x.IsClass == true)
                .Where(x => x.IsAbstract == false)
                .ToList();

            foreach (var setting in settings)
            {                
                services.AddTransient(setting, provider => 
                {
                    var service = provider.GetService<ISettingService>();
                    return service.GetType().GetMethod("LoadSetting").MakeGenericMethod(setting).Invoke(service, new object[] { 0 });
                });                
            }
        }

        public bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                    return isMatch;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void AddAutoMapperClasses(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            assemblies = assemblies as Assembly[] ?? assemblies.ToArray();

            var allTypes = assemblies.SelectMany(a => a.ExportedTypes).ToArray();
            var profiles = allTypes
                .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                .Where(t => !t.GetTypeInfo().IsAbstract);                

            Mapper.Initialize(config =>
            {
                config.CreateMap<Department, DepartmentViewModel>()
                    .ForMember(d => d.BusinessRoles, opt => opt.Ignore());
                config.CreateMap<DepartmentViewModel, Department>();
                config.CreateMap<BusinessRole, CheckableBusinessRole>();
                config.CreateMap<CheckableBusinessRole, BusinessRole>();
                config.CreateMap<BusinessRole, SecondaryBusinessRoleAPIModel>();
                config.CreateMap<SecondaryBusinessRoleAPIModel, BusinessRole>();
                config.CreateMap<EditEmployeeInfoAPIModel, Employee>()
                    .ForMember(x => x.User, opt => opt.Ignore())
                    .ForMember(x => x.WorkingDays, opt => opt.Ignore());
                config.CreateMap<Employee, EditEmployeeInfoAPIModel>();
                config.CreateMap<CreateRequestedDateModel, RequestedDate>()
                    .ForMember(x => x.LeaveRequestId, opt => opt.Ignore())
                    .ForMember(x => x.Id, opt => opt.Ignore());
                config.CreateMap<RequestedDate, RequestedDateAPIModel>();
                config.CreateMap<PendingLeaveRequest, LeaveRequestAPIModel>();
                config.CreateMap<WorkingDay, DaysWorkingAPIModel>();
                config.CreateMap<DaysWorkingAPIModel, WorkingDay>();
                config.CreateMap<AssignmentAPIModel, Assignment>();
                config.CreateMap<TrainingModuleAPIModel, TrainingModule>()
                    .ForMember(x => x.Invitations, opt => opt.Ignore());
                config.CreateMap<TrainingModule, TrainingModuleAPIModel>();
                config.CreateMap<AnnouncementAPIModel, Announcement>();

                foreach (var profile in profiles)
                {
                    config.AddProfile(profile);
                }
            });
        }
    }
}