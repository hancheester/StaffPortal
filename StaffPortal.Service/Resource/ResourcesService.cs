using AutoMapper;
using StaffPortal.Service.Departments;
using StaffPortal.Service.Roles;
using StaffPortal.Service.Staff;
using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using StaffPortal.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Service.Resource
{
    public class ResourcesService : IResourcesService
    {        
        private readonly IEmployeeService _employeeService;
        private readonly IBusinessRoleService _businessRoleService;
        private readonly IDepartmentService _departmentService;
        private readonly IRepository<Invitation> _invitationRepository;
        private readonly IRepository<Announcement> _announcementRepository;
        private readonly IRepository<Recipient> _recipientRepository;
        private readonly IRepository<TrainingModule> _trainingModuleRepository;

        public ResourcesService(            
            IEmployeeService employeeService,
            IBusinessRoleService businessRoleService,
            IDepartmentService departmentService,
            IRepository<Invitation> invitationRepository,
            IRepository<Announcement> announcementRepository,
            IRepository<Recipient> recipientRepository,
            IRepository<TrainingModule> trainingModuleRepository)
        {            
            _invitationRepository = invitationRepository;
            _employeeService = employeeService;
            _businessRoleService = businessRoleService;
            _departmentService = departmentService;
            _announcementRepository = announcementRepository;
            _recipientRepository = recipientRepository;
            _trainingModuleRepository = trainingModuleRepository;
        }

        #region TRAINING MODULE
        public void Delete(TrainingModule module)
        {
            _trainingModuleRepository.Delete(module);
        }

        public IList<TrainingModule> GetAllTrainingModules()
        {
            return _trainingModuleRepository.GetAll().ToList();
        }

        public IList<TrainingModule> GetAllTrainingModules(int employeeId, int status)
        {
            var trainingModules = _trainingModuleRepository.GetAll()
                                    .Join(_invitationRepository.GetAll().Where(i => i.StatusCode == status),
                                    t => t.Id,
                                    i => i.TrainingModuleId,
                                    (t, i) => t)
                                    .ToList();

            return trainingModules;
        }


        public void Insert(TrainingModuleAPIModel model)
        {
            var module = Mapper.Map<TrainingModule>(model);
            module.DateCreated = DateTime.Now;

            _trainingModuleRepository.Create(module);

            if (model.InvitedEmployees != null && model.InvitedEmployees.Count > 0)
                foreach (var employee in model.InvitedEmployees)
                {
                    var invitation = new Invitation(employee.Id, module.Id);
                    _invitationRepository.Create(invitation);
                }
        }

        public void Insert(TrainingModule module)
        {
            _trainingModuleRepository.Create(module);
        }

        public void Update(TrainingModule module)
        {
            _trainingModuleRepository.Update(module);
        }

        public IList<TrainingModuleAPIModel> GetAllOnAPIModel()
        {
            var modules = _trainingModuleRepository.GetAll()
                            .Select(m =>
                            ToTrainingModuleAPIModel(m))
                            .ToList();

            return modules;
        }

        public TrainingModuleAPIModel Get(int moduleId)
        {
            var module = _trainingModuleRepository.Return(moduleId);

            var model = Mapper.Map<TrainingModuleAPIModel>(module);
            model.Invitations = _invitationRepository.Table.Where(x => x.TrainingModuleId == module.Id).ToList();

            model.InvitationsCount = model.Invitations.Count();
            model.AcceptedCount = model.Invitations.Where(i => i.StatusCode == (int)RequestStatus.Accepted).Count();
            model.TrainingMaterialsFileNames = AllFilesFromFolder(model.TrainingMaterialsFolderPath);

            return model;
        }

        public IList<Invitation> GetAllInvitations(int trainingModuleId)
        {
            return _invitationRepository.Table.Where(x => x.TrainingModuleId == trainingModuleId).ToList();
        }

        public void InsertInvitation(int employeeId, int moduleId)
        {
            var invitation = new Invitation(employeeId, moduleId);

            _invitationRepository.Create(invitation);
        }

        public void DeleteInvitation(int employeeId, int moduleId)
        {
            var invitation = _invitationRepository.Table
                .Where(x=> x.EmployeeId == employeeId)
                .Where(x => x.TrainingModuleId == moduleId)
                .FirstOrDefault();

            _invitationRepository.Delete(invitation);
        }

        private TrainingModuleAPIModel ToTrainingModuleAPIModel(TrainingModule m)
        {
            TrainingModuleAPIModel module = Mapper.Map<TrainingModuleAPIModel>(m);

            module.InvitationsCount = m.Invitations.Count();
            module.AcceptedCount = m.Invitations.Where(i => i.StatusCode == (int)RequestStatus.Accepted).Count();

            var invitedEmployees = m.Invitations
                                    .Join(_employeeService.GetAll(),
                                    i => i.EmployeeId,
                                    e => e.Id,
                                    (i, e) => ToEmployeeInfoAPIModel(e))
                                    .ToList();


            // TODO: get all files from TrainingMaterialsPath (did something similar in Farmco International Project)
            // TODO: consider also employees

            return module;
        }

        private EmployeeInfoAPIModel ToEmployeeInfoAPIModel(Employee e)
        {
            return new EmployeeInfoAPIModel
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Gender = e.Gender,
                ApplicationUserId = e.User.Id,
                NIN = e.NIN,
                DOB = e.DOB,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                HolidayAllowance = e.HolidayAllowance,
                Barcode = e.Barcode,
                HoursRequired = e.HoursRequired,
                PrimaryBusinessRole = _businessRoleService.GetPrimaryBusinessRoleByEmployeeId(e.Id),
                AssignedDepartments = Task.Run(() => _departmentService.GetAssignedDepartmentsByEmployeeIdAsync(e.Id)).Result
            };
        }

        #endregion END TRAINING MODULE

        #region ANNOUNCEMENTS

        public void Insert(AnnouncementAPIModel model)
        {
            var announcement = Mapper.Map<Announcement>(model);

            announcement.CreateDate = DateTime.Now;

            _announcementRepository.Create(announcement);
            foreach (var employee in model.Recipients)
            {
                var recipient = new Recipient(employee.Id, announcement.Id);

                _recipientRepository.Create(recipient);
            }
        }

        public void Update(AnnouncementAPIModel model)
        {
            var announcement = Mapper.Map<Announcement>(model);

            _announcementRepository.Update(announcement);
        }

        public Announcement GetAnnouncement(int announcementId)
        {
            return _announcementRepository.Return(announcementId);
        }

        public IList<Announcement> GetAllAnnouncements()
        {
            return _announcementRepository.GetAll();
        }

        #endregion END ANNOUNCEMENTS

        private IList<string> AllFilesFromFolder(string path)
        {
            List<string> model = null;
            if (path != null)
            {
                var paths = Directory.GetFiles(path);
                model = new List<string>();

                foreach (var filePath in paths)
                {
                    var filePathParts = filePath.Split('\\');
                    var fileName = filePathParts[filePathParts.Length - 1];

                    model.Add(fileName);
                }
            }
            return model;
        }

    }
}