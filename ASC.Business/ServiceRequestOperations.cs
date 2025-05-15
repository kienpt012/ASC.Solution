using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using ASC.Model.BaseTypes;
using ASC.Models.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ServiceRequestOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateServiceRequestAsync(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedDateAndStatus(DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            var query = Queries.GetDashboardQuery<ServiceRequest>(requestedDate, status, email, serviceEngineerEmail);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQuery(query);
            return serviceRequests.ToList();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsFormAudit(string serviceEngineerEmail = "")
        {
            var query = Queries.GetDashboardAuditQuery(serviceEngineerEmail);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByPartitionKeyAsync(query);
            return serviceRequests.Take(20).ToList();
        }

        public async Task<List<ServiceRequest>> GetActiveServiceRequests(List<string> status)
        {
            var query = Queries.GetDashboardServiceEngineersQuery(status);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByPartitionKeyAsync(query);
            return serviceRequests.ToList();
        }

        public async Task<ServiceRequest> GetServiceRequestByRowKey(string id)
        {
            var query = Queries.GetServiceRequestDetailsQuery(id);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByPartitionKeyAsync(query);
            return serviceRequests.FirstOrDefault();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestAuditByPartitionKey(string id)
        {
            var query = Queries.GetServiceRequestAuditDetailsQuery(id);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByPartitionKeyAsync(query);
            return serviceRequests.ToList();
        }

        public async Task<ServiceRequest> UpdateServiceRequestAsync(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                _unitOfWork.Repository<ServiceRequest>().Update(request);
                _unitOfWork.CommitTransaction();

                return request;
            }
        }

        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(string rowKey, string partitionKey, string status)
        {
            using (_unitOfWork)
            {
                var serviceRequest = await _unitOfWork.Repository<ServiceRequest>().FindAsync(partitionKey, rowKey);

                if (serviceRequest == null)
                    throw new NullReferenceException();

                serviceRequest.Status = status;

                _unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
                _unitOfWork.CommitTransaction();

                return serviceRequest;
            }
        }
    }
}