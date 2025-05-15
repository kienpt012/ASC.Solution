using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using Microsoft.WindowsAzure.Storage.Table;
using ASC.Models;
using ASC.Utilities;
namespace ASC.Models.Queries
{
    public static class Queries
    {
        public static Expression<Func<ServiceRequest, bool>> GetDashboardQuery<T>(DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            var query = (Expression<Func<ServiceRequest, bool>>)(x => true);

            // Add Requested Date Clause
            if (requestedDate.HasValue)
            {
                var requestedDateFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.RequestedDate >= requestedDate);
                query  = query.And(requestedDateFilter);
            }

            // Add Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(email))
            {
                var requestedDateFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.PartitionKey == email);
                query = query.And(requestedDateFilter);
            }

            // Add Service Engineer Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                var requestedDateFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.ServiceEngineer == serviceEngineerEmail);
                query = query.And(requestedDateFilter);
            }

            // Add Status clause if status is passed as a parameter.
            // Individual status clauses are appended with OR Condition
            var statusQueries = (Expression<Func<ServiceRequest, bool>>)(x => false);
            if (status != null)
            {
                foreach (var state in status)
                {
                    var statusFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.Status == state);
                    statusQueries = statusQueries.Or(statusFilter);
                }
                query = query.And(statusQueries);
                
            }

            return query;
        }

        public static string GetDashboardAuditQuery(string email = "")
        {
            var finalQuery = string.Empty;

            // Add Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(email))
            {
                finalQuery = TableQuery.GenerateFilterCondition("ServiceEngineer", QueryComparisons.Equal, email);
            }

            return finalQuery;
        }

        public static string GetDashboardServiceEngineersQuery(List<string> status)
        {
            var finalQuery = string.Empty;
            var statusQueries = new List<string>();

            // Add Status clause if status is passed a parameter.
            foreach (var state in status)
            {
                statusQueries.Add(TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, state));
            }
            finalQuery = string.Join($" {TableOperators.Or} ", statusQueries);

            return finalQuery;
        }

        public static string GetServiceRequestDetailsQuery(string id)
        {
            return TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id);
        }

        public static string GetServiceRequestAuditDetailsQuery(string id)
        {
            return TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id);
        }
    }
}