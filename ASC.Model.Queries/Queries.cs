using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ASC.Utilities;
using ASC.Models;

namespace ASC.Model.Queries
{
    public static class Queries
    {
        public static Expression<Func<ServiceRequest, bool>> GetDashboardQuery(DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            var query = PredicateBuilder.True<ServiceRequest>();
            
            //// Add Requested Date Clause
            if (requestedDate.HasValue)
            {
                var requestedDateFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.RequestedDate == requestedDate);
                query = query.And(requestedDateFilter);
            }

            //// Add Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(email))
            {
                var requestedDateFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.PartitionKey == email);
                query = query.And(requestedDateFilter);
            }

            // Add Service Engineer Email Clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                var requestedDateFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.ServiceEngineer == serviceEngineerEmail);
                query = query.And(requestedDateFilter);
            }

            // Add Status clause if status is passed as parameter.
            // Individual status clauses are appended with OR Condition
            var statusQueries = PredicateBuilder.False<ServiceRequest>();
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
    }
} 