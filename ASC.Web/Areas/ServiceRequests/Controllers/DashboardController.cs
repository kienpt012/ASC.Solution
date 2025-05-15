using ASC.Business.Interfaces;
using ASC.Model;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Configuration;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks; // Correct namespace for IOptions<T>

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations; // Use the interface for dependency injection
        private readonly IMasterDataOperations _masterData;
        private readonly IOptions<ApplicationSettings> _settings; // Use IOptions<T> and make it readonly

        public DashboardController(IServiceRequestOperations operations, IMasterDataOperations masterData)
        {
            _serviceRequestOperations = operations;
            _masterData = masterData;
        }

        public async Task<IActionResult> Dashboard()
        {
            var status = new List<string>
                {
                    Status.New.ToString(),
                    Status.InProgress.ToString(),
                    Status.Initiated.ToString(),
                    Status.RequestForInformation.ToString()
                };
            List<ServiceRequest> serviceRequests = new List<ServiceRequest>();
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    DateTime.UtcNow.AddDays(-7),
                    status);

            }
            else if(HttpContext.User.IsInRole(Roles.Engineer.ToString()))
            {
                serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    DateTime.UtcNow.AddYears(-7),
                    status,
                    serviceEngineerEmail: HttpContext.User.GetCurrentUserDetails().Email);
            }
            else
            {
                serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    DateTime.UtcNow.AddYears(-1),
                    email:HttpContext.User.GetCurrentUserDetails().Email);
            }
            return View(new DashboardViewModel
            {
                ServiceRequests = serviceRequests.OrderByDescending(p => p.RequestedDate).ToList()
            });  
        }
    }
}