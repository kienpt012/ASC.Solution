using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.BaseTypes;
using Microsoft.WindowsAzure.Storage.Table;

namespace ASC.Models.Queries
{
    public class Queries
    {
        public static string GetDashboardQuery(DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            var finalQuery = string.Empty;
            var statusQueries = new List<string>();

            // Add Requested Date Clause
            if (requestedDate.HasValue)
            {
                finalQuery = TableQuery.GenerateFilterConditionForDate("RequestedDate", QueryComparisons.GreaterThanOrEqual, requestedDate.Value);
            }

            // Add Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(email))
            {
                finalQuery = !string.IsNullOrWhiteSpace(finalQuery)
                    ? TableQuery.CombineFilters(finalQuery, TableOperators.And, TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email))
                    : TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
            }

            // Add Service Engineer Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                finalQuery = !string.IsNullOrWhiteSpace(finalQuery)
                    ? TableQuery.CombineFilters(finalQuery, TableOperators.And, TableQuery.GenerateFilterCondition("ServiceEngineer", QueryComparisons.Equal, serviceEngineerEmail))
                    : TableQuery.GenerateFilterCondition("ServiceEngineer", QueryComparisons.Equal, serviceEngineerEmail);
            }

            // Add Status clause if status is passed as a parameter.
            // Individual status clauses are appended with OR Condition
            if (status != null)
            {
                foreach (var state in status)
                {
                    statusQueries.Add(TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, state));
                }
                var statusQuery = string.Join($" {TableOperators.Or} ", statusQueries);

                finalQuery = !string.IsNullOrWhiteSpace(finalQuery)
                    ? $"{finalQuery} {TableOperators.And} ({statusQuery})"
                    : $"({statusQuery})";
            }

            return finalQuery;
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