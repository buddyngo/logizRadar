using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Model
{
    public class UserMappingProject : BaseModel
    {
        [Required]
        [Remote(action: "CheckDuplicateMapping", controller: "UserMappingProject", AdditionalFields = nameof(ProjectID) + "," + nameof(ID))]
        public string Username { get; set; }
        [Required]
        [Remote(action: "CheckDuplicateMapping", controller: "UserMappingProject", AdditionalFields = nameof(Username) + "," + nameof(ID))]
        public string ProjectID { get; set; }
    }
}
