using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Model
{
    public abstract class BaseModel
    {
        public BaseModel()
        {
            ID = Guid.NewGuid().ToString();
            CreatedBy = "SYSTEM";
            CreatedDateTime = DateTime.Now;
            UpdatedBy = "SYSTEM";
            UpdatedDateTime = new DateTime(CreatedDateTime.Ticks);
            IsActive = true;
        }

        public void SetCreator(string creator)
        {
            CreatedBy = creator;
            CreatedDateTime = DateTime.Now;
            UpdatedBy = creator;
            UpdatedDateTime = new DateTime(CreatedDateTime.Ticks);
        }

        public void SetUpdater (string updater)
        {
            UpdatedBy = updater;
            UpdatedDateTime = DateTime.Now;
        }

        public void UpdateActiveStatus (bool isActive, string updater)
        {
            IsActive = isActive;
            SetUpdater(updater);
        }

        public string ID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public bool IsActive { get; set; }
    }
}
