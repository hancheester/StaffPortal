namespace StaffPortal.Common
{
    public static class GlobalConstants
    {
        #region DATA SEEDING

        public const string ASPNETORLE_SUPERADMIN = "SuperAdmin";

        public const string MENUITEM_COTROLLER_ADMIN = "Admin";
        public const string MENUITEM_COTROLLER_HOME = "Home";
        public const string MENUITEM_COTROLLER_MANAGE = "Manage";
        public const string MENUITEM_COTROLLER_ACCOUNT = "Account";

        public const string MENUITEM_DESCRIPTION_GENERIC = "Standard Menu Item";
        public const string MENUITEM_ACTION_MANAGE = "Manage";
        public const string MENUITEM_ACTION_DEPARTMENTS = "Departments";
        public const string MENUITEM_ACTION_LEAVE = "Leave";
        public const string MENUITEM_ACTION_APPROVELEAVE = "ApproveLeave";
        public const string MENUITEM_ACTION_MYSHIFTS = "MyShifts";
        public const string MENUITEM_ACTION_EDITSHIFTS = "EditShifts";
        public const string MENUITEM_ACTION_EDITACCOUNT = "EditAccount";
        public const string MENUITEM_ACTION_AUDITTRAIL = "AuditTrail";
        public const string MENUITEM_ACTION_TRAININGMODULES = "Training";
        public const string MENUITEM_ACTION_ANNOUNCEMENTS = "Announcements";
        public const string MENUITEM_ACTION_CLOCKINOUT = "ClockInOut";
        public const string MENUITEM_ACTION_TIMESHEET = "EditTimesheet";
        public const string MENUITEM_ACTION_SETTINGS = "Settings";
        public const string MENUITEM_ACTION_ACCOUNTANTREPORTS = "AccountantReports";

        public const string MENUITEM_NAME_HOME = "Dashboard";
        public const string MENUITEM_NAME_PENDINGLEAVEREQUEST = "Pending Leave Request";
        public const string MENUITEM_NAME_MYSHIFTS = "My Shifts";
        public const string MENUITEM_NAME_EDITSHIFTS = "Edit Staff Shifts";
        public const string MENUITEM_NAME_EDITACCOUNT = "My Account";
        public const string MENUITEM_NAME_AUDITTRAIL = "Audit Trail";
        public const string MENUITEM_NAME_TRAININGMODULES = "Training Modules";
        public const string MENUITEM_NAME_MYTRAININGS = "My Trainings";
        public const string MENUITEM_NAME_ANNOUNCEMENTS = "Announcements";
        public const string MENUITEM_NAME_CLOCKINOUT = "Clock In/Out";
        public const string MENUITEM_NAME_TIMESHEET = "Edit Timesheet";
        public const string MENUITEM_NAME_ACCOUNTANTREPORTS = "Accountant Reports";

        public const string APPLICATIONROLE_MAIN = "SuperAdmin";
        public const string APPLICATIONROLE_USER = "User";

        public const string BUSINESSROLE_ADMIN = "Admin";
        public const string BUSINESSROLE_DESCRIPTION = "Standard Business Role";
        public const string BUSINESSROLE_MANAGER = "Manager";
        public const string BUSINESSROLE_STAFFMEMBER = "StaffMember";
        public const string DEPARTMENT_NAME = "Head Department";

        public const string PERMISSION_DESCRIPTION = "Standard Permission";
        #endregion END DATA SEEDING

        #region EMAIL TEMPLATING
        public const string EMAILTEMPLATES_FOLDERNAME = "EmailTemplates";
        public const string EMAILTEMPLATES_LEAVEREQUEST = "LeaveRequest.html";
        public const string EMAILTEMPLATES_REGISTRATION_REGISTRANT = "Registration_Registrant.html";
        public const string EMAILTEMPLATES_REGISTRATION_ADMIN = "Registration_Admin.html";
        public const string EMAILTEMPLATES_LEAVEREQUEST_REPLY = "LeaveRequest_Reply.html";
        public const string EMAILSERVICE_FOLDERNAME = "EmailService";
        public const string EMAILTEMPLATING_PARAMETERPREFIX = "@@";

        #endregion EMAIL TEMPLATING

        public const string FILENAME_GENERALSETTINGS = "generalSettings.json";
        public const string SESSIONKEYSUFFIX_TRAININGMODULE = " - trainig_module";
    }

    public struct CalendarDayStatusCode
    {
        public const string BELOW_MINIMUM = "Below-min";
        // TODO: ...complete
    }

    // Client side reflects the same enum: js/enums_consts.StaffLevelStatus
    public enum StaffLevelStatus
    {
        Critical = 0,
        Decent = 1,
        Acceptable = 2
    }

    public enum RequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }

    public enum PermissionsCode
    {
        LeaveRequestApprover = 1,
        LeaveRequestApplier = 2,
        SystemConfigurationManager = 3,
        UserMangaer = 4,
        ReportsAreaAccess = 5,
        ResourcesAreaAccess = 6,
        IntranetAreaAccess = 7,
        TimesheetApprover = 8
    }

    /*
     * 0 - NON-ACCRUABLE the leave type is non-accruable
     * 1 - ACCRUABLE AS PAY: leave type is accruable and the date requested is within a Payable Month, based on leave settings
     * 2 - ACCRUABLE AS DAY: leave type is accruable and the date requested is NOT within a Payable Month, based on leave settings
     */

    public enum AccruableAs
    {
        NonAccruable = 0,
        Pay = 1,
        Day = 2
    }

    public enum Frequency
    {
        ThreeMonths = 0,
        SixMonths = 1,
        TwelveMonths = 2
    }
}