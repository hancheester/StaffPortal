using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StaffPortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Data.Utility
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IServiceProvider _serviceProvider;

        public DbInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        //This example just creates an Administrator role and one Admin users
        public async Task Initialize()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            using (var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>())
            using (var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>())
            {
                context.Database.EnsureCreated();

                #region SEED APPLICATION ROLES AND USERS
                foreach (var userData in Users)
                {
                    var role = userData.Key;
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }

                    var identityUser = userData.Value;
                    if (await userManager.FindByNameAsync(identityUser.UserName) == null)
                    {
                        await userManager.CreateAsync(identityUser, "password");
                        await userManager.AddToRoleAsync(identityUser, userData.Key);
                    }
                }

                var admin = await userManager.FindByEmailAsync(Users
                                                               .Where(u => string.Equals(u.Key, GlobalConstants.APPLICATIONROLE_MAIN))
                                                               .FirstOrDefault().Value.Email);
                var user = await userManager.FindByEmailAsync(Users
                                                              .Where(u => string.Equals(u.Key, GlobalConstants.APPLICATIONROLE_USER))
                                                              .FirstOrDefault().Value.Email);

                #endregion APPLICATION ROLES AND USERS

                #region SEED EMPLOYEE INFO

                var staff = context.Employees.Where(x => x.UserId == user.Id).FirstOrDefault();
                if (staff == null)
                {
                    staff = new Employee
                    {
                        UserId = user.Id,
                        HolidayAllowance = 28,
                        HoursRequired = 40,
                        StartDate = new DateTime(2017, 12, 1),
                        EndDate = new DateTime(2017, 12, 31),
                        DOB = new DateTime(2010, 10, 10),
                        NIN = "QQ 12 34 56 C",
                        Barcode = "123546789"
                    };
                    context.Employees.Add(staff);
                    context.SaveChanges();
                }

                var manager = context.Employees.Where(x => x.UserId == admin.Id).FirstOrDefault();
                if (manager == null)
                {
                    manager = new Employee
                    {
                        UserId = admin.Id,
                        HolidayAllowance = 28,
                        HoursRequired = 40,
                        StartDate = new DateTime(2017, 12, 1),
                        EndDate = new DateTime(2017, 12, 31),
                        DOB = new DateTime(2010, 10, 10),
                        NIN = "QQ 12 34 56 C",
                        Barcode = "123546789"
                    };

                    context.Employees.Add(manager);
                    context.SaveChanges();
                }

                #endregion SEED EMPLOYEEINFO            

                #region SEED LASTTIMESTAMP_EMPLOYEE

                if (!context.LastTimestamp_Employee_Registry.Any())
                {
                    var timestamp_registry = new LastTimestamp_Employee[]
                    {
                    new LastTimestamp_Employee{
                        EmployeeId = staff.Id,
                        LastTimestampId = 0
                    },
                    new LastTimestamp_Employee
                    {
                        EmployeeId = manager.Id,
                        LastTimestampId = 0
                    }
                    };

                    context.LastTimestamp_Employee_Registry.AddRange(timestamp_registry);
                }

                #endregion  END LASTTIMESTAMP_EMPLOYEE

                #region SEED MENU ITEMS

                if (!context.MenuItems.Any())
                {
                    context.MenuItems.AddRange(MenuItems);
                    context.SaveChanges();

                    int manageMenuItemId = context.MenuItems.First(i => string.Equals(i.Title, GlobalConstants.MENUITEM_ACTION_MANAGE)).Id;
                    int systemMenuItemId = context.MenuItems.First(i => string.Equals(i.Title, "System")).Id;

                    // SEED Sub Menu Items !!!
                    context.MenuItems.AddRange(
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = "BusinessRoles",
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = "Business Roles"
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_DEPARTMENTS,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_ACTION_DEPARTMENTS
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_LEAVE,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_MANAGE,
                        Title = GlobalConstants.MENUITEM_ACTION_LEAVE
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_LEAVE,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_NAME_PENDINGLEAVEREQUEST
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_MYSHIFTS,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_MANAGE,
                        Title = GlobalConstants.MENUITEM_NAME_MYSHIFTS
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_EDITSHIFTS,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_NAME_EDITSHIFTS
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_EDITACCOUNT,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ACCOUNT,
                        Title = GlobalConstants.MENUITEM_NAME_EDITACCOUNT
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = systemMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_AUDITTRAIL,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_NAME_AUDITTRAIL
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_TRAININGMODULES,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_NAME_TRAININGMODULES
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_ANNOUNCEMENTS,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_MANAGE,
                        Title = GlobalConstants.MENUITEM_NAME_ANNOUNCEMENTS
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_TRAININGMODULES,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_MANAGE,
                        Title = GlobalConstants.MENUITEM_NAME_MYTRAININGS
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_CLOCKINOUT,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_MANAGE,
                        Title = GlobalConstants.MENUITEM_NAME_CLOCKINOUT
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_TIMESHEET,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_NAME_TIMESHEET
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = systemMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_SETTINGS,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_NAME_TIMESHEET
                    },
                    new MenuItem
                    {
                        //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                        //ParentMenuItemId = manageMenuItemId,
                        //TargetAction = GlobalConstants.MENUITEM_ACTION_ACCOUNTANTREPORTS,
                        //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                        Title = GlobalConstants.MENUITEM_NAME_ACCOUNTANTREPORTS
                    });

                    context.SaveChanges();

                    // SEED ROLE_MENUITEMS
                    //if (!context.BusinessRole_MenuItems.Any())
                    //{
                    //    var menuItems = context.MenuItems.ToList();

                    //    foreach (var item in menuItems)
                    //    {
                    //        context.BusinessRole_MenuItems.Add(new BusinessRole_MenuItem
                    //        {
                    //            MenuItemId = item.Id,
                    //            RoleName = GlobalConstants.APPLICATIONROLE_MAIN,
                    //            Priority = 0
                    //        });
                    //        if (string.Equals(item.Title, GlobalConstants.MENUITEM_NAME_HOME)
                    //            || string.Equals(item.Title, GlobalConstants.MENUITEM_ACTION_MANAGE)
                    //            || string.Equals(item.Title, GlobalConstants.MENUITEM_ACTION_LEAVE)
                    //            //|| string.Equals(item.TargetAction, GlobalConstants.MENUITEM_ACTION_MYSHIFTS)
                    //            //|| string.Equals(item.TargetAction, GlobalConstants.MENUITEM_ACTION_EDITACCOUNT)
                    //            || string.Equals(item.Title, GlobalConstants.MENUITEM_NAME_MYTRAININGS)
                    //            //|| string.Equals(item.TargetAction, GlobalConstants.MENUITEM_ACTION_CLOCKINOUT)
                    //            )
                    //        {
                    //            context.BusinessRole_MenuItems.Add(new BusinessRole_MenuItem
                    //            {
                    //                MenuItemId = item.Id,
                    //                RoleName = GlobalConstants.APPLICATIONROLE_USER,
                    //                Priority = 0
                    //            });
                    //        }
                    //    }
                    //}
                    //context.SaveChanges();
                }

                #endregion MENU ITEMS

                #region SEED BUSINESS ROLES

                int businessRoleId_User = -1;
                int businessRoleId_Manager = -1;

                if (!context.BusinessRoles.Any())
                {
                    context.BusinessRoles.Add(adminBusinessRole);
                    context.SaveChanges();

                    adminBusinessRole = context.BusinessRoles.Where(b => string.Equals(b.Name, GlobalConstants.BUSINESSROLE_ADMIN)).FirstOrDefault();

                    context.BusinessRoles.AddRange(new BusinessRole
                    {
                        Name = GlobalConstants.BUSINESSROLE_MANAGER,
                        Description = GlobalConstants.BUSINESSROLE_MANAGER + " - " + GlobalConstants.BUSINESSROLE_DESCRIPTION,
                        ParentBusinessRoleId = adminBusinessRole.Id
                    },
                    new BusinessRole
                    {
                        Name = GlobalConstants.BUSINESSROLE_STAFFMEMBER,
                        Description = GlobalConstants.BUSINESSROLE_STAFFMEMBER + " - " + GlobalConstants.BUSINESSROLE_DESCRIPTION,
                        ParentBusinessRoleId = adminBusinessRole.Id
                    });

                    context.SaveChanges();
                    businessRoleId_User = context.BusinessRoles.Where(b => string.Equals(b.Name, GlobalConstants.BUSINESSROLE_STAFFMEMBER)).FirstOrDefault().Id;
                    businessRoleId_Manager = context.BusinessRoles.Where(b => string.Equals(b.Name, GlobalConstants.BUSINESSROLE_MANAGER)).FirstOrDefault().Id;
                }

                #endregion END BUSINESS BUSINESSROLES

                #region SEED PERMISSION 

                if (!context.Permissions.Any())
                {
                    context.Permissions.AddRange(Permissions);
                }

                #endregion END SEED PERMISSIONS

                #region SEED BUSINESSROLE_PERMISSIONS

                if (!context.BusinessRole_Permissions.Any())
                {
                    Seed_BusinessRole_Permissions(context, adminBusinessRole);
                    context.SaveChanges();
                }

                #endregion END SEED BUSINESSROLE_PERMISSIONS

                #region SEED EMPLOYEE_BUSINESSROLES

                if (!context.Employee_BusinessRoles.Any())
                {
                    Seed_ApplicationUser_BusinessRoles(context, staff.Id, manager.Id);
                    context.SaveChanges();
                }

                #endregion SEED APPLICATIONUSER_BUSINESSROLES

                #region SEED DEPARTMENTS

                if (!context.Departments.Any())
                {
                    context.Departments.Add(department);
                    context.SaveChanges();
                }
                else
                    department = context.Departments.FirstOrDefault(d => string.Equals(d.Name, "Head Department"));

                #endregion END SEED DEPARTMENTS 

                #region SEED DAYSWORKING

                if (!context.WorkingDays.Any())
                {
                    var user_daysWorking = PrepareDaysWorking(department.Id, staff.Id);
                    var admin_daysWorking = PrepareDaysWorking(department.Id, manager.Id);

                    context.WorkingDays.AddRange(user_daysWorking);
                    context.WorkingDays.AddRange(admin_daysWorking);
                    context.SaveChanges();
                }

                #endregion SEED DAYSWORKING

                var departmentId = context.Departments.ToList().FirstOrDefault(d => string.Equals(d.Name, GlobalConstants.DEPARTMENT_NAME)).Id;

                #region SEED OPENING HOURS

                if (!context.OpeningHours.Any())
                {
                    foreach (var oh in OpeningHours)
                    {
                        oh.DepartmentId = departmentId;
                        context.OpeningHours.Add(oh);
                    };
                    context.SaveChanges();
                }

                #endregion END SEED OPNEING HOURS

                #region SEED BUSINESSROLE_DEPARTMENT

                if (!context.BusinessRole_Departments.Any())
                {
                    context.BusinessRole_Departments.Add(new BusinessRole_Department
                    {
                        DepartmentId = departmentId,
                        BusinessRoleId = businessRoleId_Manager,
                        MinimumRequired = 2
                    });
                }

                #endregion SEED BUSINESSROLE_DEPARTMENT

                #region SEED LEAVE TYPES

                if (!context.LeaveTypes.Any())
                {
                    context.LeaveTypes.AddRange(LeaveTypes);
                }

                #endregion END SEED LEAVE TYPES
            }
        }

        private void Seed_ApplicationUser_BusinessRoles(ApplicationDbContext context, int userId, int managerId)
        {
            var businessRoleId_StaffMember = context.BusinessRoles.FirstOrDefault(b => string.Equals(b.Name, GlobalConstants.BUSINESSROLE_STAFFMEMBER)).Id;
            var businessRoleId_Manager = context.BusinessRoles.FirstOrDefault(b => string.Equals(b.Name, GlobalConstants.BUSINESSROLE_MANAGER)).Id;

            var applicationUser_BusinessRoles = new List<Employee_BusinessRole>
            {
                new Employee_BusinessRole { EmployeeId = managerId, BusinessRoleId = businessRoleId_Manager, IsPrimary = true },
                new Employee_BusinessRole { EmployeeId = userId, BusinessRoleId = businessRoleId_StaffMember, IsPrimary = true }
            };

            context.Employee_BusinessRoles.AddRange(applicationUser_BusinessRoles);
        }

        private IList<WorkingDay> PrepareDaysWorking(int departmentId, int employeeId)
        {
            IList<WorkingDay> daysWorking = new List<WorkingDay>()
            {
                new WorkingDay
                {
                    Day = "Monday",
                    IsAssigned = true,
                    StartTime = OPENING_TIME,
                    EndTime = CLOSING_TIME,
                    DepartmentId = departmentId,
                    EmployeeId = employeeId
                },
                new WorkingDay
                {
                    Day = "Tuesday",
                    IsAssigned = true,
                    StartTime = OPENING_TIME,
                    EndTime = CLOSING_TIME,
                    DepartmentId = departmentId,
                    EmployeeId = employeeId
                },
                new WorkingDay
                {
                    Day = "Wednesday",
                    IsAssigned = true,
                    StartTime = OPENING_TIME,
                    EndTime = CLOSING_TIME,
                    DepartmentId = departmentId,
                    EmployeeId = employeeId
                },
                new WorkingDay
                {
                    Day = "Thursday",
                    IsAssigned = true,
                    StartTime = OPENING_TIME,
                    EndTime = CLOSING_TIME,
                    DepartmentId = departmentId,
                    EmployeeId = employeeId
                },
                new WorkingDay
                {
                    Day = "Friday",
                    IsAssigned = true,
                    StartTime = OPENING_TIME,
                    EndTime = CLOSING_TIME,
                    DepartmentId = departmentId,
                    EmployeeId = employeeId
                },
                new WorkingDay
                {
                    Day = "Saturday",
                    IsAssigned = false,
                    StartTime = DateTime.Parse("00:00:00").TimeOfDay,
                    EndTime = DateTime.Parse("00:00:00").TimeOfDay,
                    DepartmentId = -1,
                    EmployeeId = employeeId
                },
                new WorkingDay
                {
                    Day = "Sunday",
                    IsAssigned = false,
                    StartTime = DateTime.Parse("00:00:00").TimeOfDay,
                    EndTime = DateTime.Parse("00:00:00").TimeOfDay,
                    DepartmentId = -1,
                    EmployeeId = employeeId
                }
            };

            return daysWorking;
        }

        private void Seed_BusinessRole_Permissions(ApplicationDbContext context, BusinessRole businessRole)
        {
            var businessRole_Permissions = new List<BusinessRole_Permission>();
            foreach (var p in Permissions)
            {
                businessRole_Permissions.Add(new BusinessRole_Permission { PermissionId = p.Id, BusinessRoleId = businessRole.Id });
            }

            context.BusinessRole_Permissions.AddRange(businessRole_Permissions);
        }

        // USERS AND ROLES DATA
        private static Dictionary<string, IdentityUser> users;
        public static Dictionary<string, IdentityUser> Users
        {
            get
            {
                if (users == null)
                {
                    users = new Dictionary<string, IdentityUser>();

                    users.Add(GlobalConstants.APPLICATIONROLE_MAIN, new IdentityUser
                    {
                        UserName = GlobalConstants.APPLICATIONROLE_MAIN,
                        Email = GlobalConstants.APPLICATIONROLE_MAIN.ToLower() + "@staffportal.sp",
                        EmailConfirmed = true,
                        PhoneNumber = "07709042506"
                    });

                    users.Add(GlobalConstants.APPLICATIONROLE_USER, new IdentityUser
                    {
                        UserName = GlobalConstants.APPLICATIONROLE_USER,
                        Email = GlobalConstants.APPLICATIONROLE_USER.ToLower() + "@staffportal.sp",
                        EmailConfirmed = true,
                        PhoneNumber = "07709042507"
                    });

                }
                return users;
            }
        }

        // MENUITEMS  DATA
        private static List<MenuItem> menuItems;
        public static List<MenuItem> MenuItems
        {
            get
            {
                if (menuItems == null)
                {
                    menuItems = new List<MenuItem>();

                    menuItems.AddRange(new List<MenuItem> {
                        new MenuItem
                        {
                            //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                            //ParentMenuItemId = 0,
                            //TargetAction = "Index",
                            //TargetController = "Home",
                            Title = GlobalConstants.MENUITEM_NAME_HOME
                        },
                        new MenuItem
                        {
                            //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                            //ParentMenuItemId = 0,
                            //TargetAction = GlobalConstants.MENUITEM_ACTION_MANAGE,
                            //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                            Title = GlobalConstants.MENUITEM_ACTION_MANAGE
                        },
                        new MenuItem
                        {
                            //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                            //ParentMenuItemId = 0,
                            //TargetAction = "Settings",
                            //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                            Title = "System"
                        },
                        new MenuItem
                        {
                            //Description = GlobalConstants.MENUITEM_DESCRIPTION_GENERIC,
                            //ParentMenuItemId = 0,
                            //TargetAction = "Users",
                            //TargetController = GlobalConstants.MENUITEM_COTROLLER_ADMIN,
                            Title = "Users"
                       }
                    });
                }

                return menuItems;
            }
        }

        private static BusinessRole adminBusinessRole = new BusinessRole
        {
            Name = GlobalConstants.BUSINESSROLE_ADMIN,
            Description = GlobalConstants.BUSINESSROLE_ADMIN + " - " + GlobalConstants.BUSINESSROLE_DESCRIPTION,
            ParentBusinessRoleId = 0
        };

        // PERMISSIONS
        private static List<Permission> Permissions = new List<Permission> {
                new Permission { Name = "Approve / reject leave requests", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Apply leave request", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Manage system configuration", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Manage users", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Access reports area", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Access resources area", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Access intranet area", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Approve timesheet", Description = GlobalConstants.PERMISSION_DESCRIPTION },
                new Permission { Name = "Access Accountant Area", Description = GlobalConstants.PERMISSION_DESCRIPTION }                
                };

        private static Department department = new Department
        {
            Name = "Head Department",
            MinimumRequired = 10
        };

        // LEAVE TYPE
        private static List<LeaveType> LeaveTypes = new List<LeaveType> {
                new LeaveType { Name = "Vacation", Requestable = true, Payable = true, Accruable = false, ImpactOnAllowance = true },
                new LeaveType { Name = "Sick Leave", Requestable = true, Payable = true, Accruable = false, ImpactOnAllowance = false },
                new LeaveType { Name = "Unpaid Leave", Requestable = true, Payable = false, Accruable = false, ImpactOnAllowance = false },
                new LeaveType { Name = "Time Off In Lieu", Requestable = true, Payable = true, Accruable = true, ImpactOnAllowance = true },
                new LeaveType { Name = "Working", Requestable = false, Payable = true, Accruable = false, ImpactOnAllowance = false }
                };

        private static TimeSpan OPENING_TIME = DateTime.Parse("08:30:00").TimeOfDay;
        private static TimeSpan CLOSING_TIME = DateTime.Parse("17:30:00").TimeOfDay;
        private static IList<OpeningHour> OpeningHours = new List<OpeningHour>()
        {
            new OpeningHour
            {
                Day = "Monday",
                IsOpen = true,
                OpeningTime = OPENING_TIME,
                ClosingTime = CLOSING_TIME
            },
            new OpeningHour
            {
                Day = "Tuesday",
                IsOpen = true,
                OpeningTime = OPENING_TIME,
                ClosingTime = CLOSING_TIME
            },
            new OpeningHour
            {
                Day = "Wednesday",
                IsOpen = true,
                OpeningTime = OPENING_TIME,
                ClosingTime = CLOSING_TIME
            },
            new OpeningHour
            {
                Day = "Thursday",
                IsOpen = true,
                OpeningTime = OPENING_TIME,
                ClosingTime = CLOSING_TIME
            },
            new OpeningHour
            {
                Day = "Friday",
                IsOpen = true,
                OpeningTime = OPENING_TIME,
                ClosingTime = CLOSING_TIME
            },
            new OpeningHour
            {
                Day = "Saturday",
                IsOpen = false,
                OpeningTime = OPENING_TIME,
                ClosingTime = CLOSING_TIME
            },
            new OpeningHour
            {
                Day = "Sunday",
                IsOpen = false,
                OpeningTime = OPENING_TIME,
                ClosingTime = CLOSING_TIME
            }
        };
    }
}