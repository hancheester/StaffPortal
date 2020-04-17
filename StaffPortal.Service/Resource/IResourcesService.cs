using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using System.Collections.Generic;

namespace StaffPortal.Service.Resource
{
    public interface IResourcesService
    {
        #region TRAINING MODULES
        IList<TrainingModule> GetAllTrainingModules();

        IList<TrainingModule> GetAllTrainingModules(int employeeId, int status);

        IList<TrainingModuleAPIModel> GetAllOnAPIModel();

        IList<Invitation> GetAllInvitations(int trainingModuleId);

        TrainingModuleAPIModel Get(int moduleId);

        void Update(TrainingModule module);

        void Insert(TrainingModule module);

        void Insert(TrainingModuleAPIModel model);

        void Delete(TrainingModule module);

        void InsertInvitation(int employeeId, int moduleId);

        void DeleteInvitation(int employeeId, int moduleId);
        #endregion TRAINING MODULES


        #region ANNOUNCEMENTS
        void Insert(AnnouncementAPIModel model);

        void Update(AnnouncementAPIModel model);

        Announcement GetAnnouncement(int announcementId);

        IList<Announcement> GetAllAnnouncements();

        #endregion END ANNOUNCEMENTS 
    }
}