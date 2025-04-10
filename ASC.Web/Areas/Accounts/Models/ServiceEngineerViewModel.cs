using Microsoft.AspNetCore.Identity;
using ASC.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace ASC.Web.Areas.Accounts.Models
{
    public class ServiceEngineerViewModel
    {
        // Khai báo nullable vì ServiceEngineers có thể null
        public List<IdentityUser>? ServiceEngineers { get; set; }

        // Thêm required vì Registration luôn cần được khởi tạo
        public ServiceEngineerRegistrationViewModel Registration { get; set; }
    }
}   