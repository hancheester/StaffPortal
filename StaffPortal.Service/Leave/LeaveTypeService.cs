using Microsoft.AspNetCore.Hosting;
using StaffPortal.Common;
using StaffPortal.Common.Models;
using StaffPortal.Data;
using StaffPortal.Service.Errors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Service.Leave
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly IRepository<LeaveType> _leaveTypeRepository;
        private readonly IErrorService _errorService;

        public LeaveTypeService(
            IRepository<LeaveType> leaveTypeRepository,
            IErrorService errorService)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _errorService = errorService;
        }

        public LeaveType GetLeaveTypeById(int id)
        {
            return _leaveTypeRepository.Return(id);
        }

        public IList<LeaveType> GetAllLeaveTypes()
        {
            var types = _leaveTypeRepository.Table.ToList();
            
            return types;
        }

        public OperationResult<LeaveType> InsertLeaveType(LeaveType leaveType)
        {
            var result = new OperationResult<LeaveType>(leaveType);

            try
            {
                leaveType.Id = _leaveTypeRepository.Create(leaveType);                
            }
            catch(Exception ex)
            {
                result.AddOperationError("E1", "Failed to create leave type.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public void UpdateLeaveType(LeaveType leaveType)
        {
            var foundItem = _leaveTypeRepository.Return(leaveType.Id);

            if (foundItem != null)
            {
                foundItem.Name = leaveType.Name;
                foundItem.Requestable = leaveType.Requestable;
                foundItem.Payable = leaveType.Payable;
                foundItem.Accruable = leaveType.Accruable;
                foundItem.ImpactOnAllowance = leaveType.ImpactOnAllowance;

                _leaveTypeRepository.Update(foundItem);
            }
        }

        public OperationResult DeleteLeaveType(int id)
        {
            var result = new OperationResult();

            try
            {
                var foundItem = _leaveTypeRepository.Return(id);
                if (foundItem != null) _leaveTypeRepository.Delete(foundItem);
            }
            catch(Exception ex)
            {
                result.AddOperationError("E2", "Failed to delete leave type.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public IList<LeaveType> GetAllRequestable()
        {
            var types = _leaveTypeRepository.Table
                .Where(x => x.Requestable == true)
                .ToList();

            return types;
        }
    }
}