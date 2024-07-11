using System;

namespace PhishingPortal.Dto.Dashboard
{
    public class AdminDashboardDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int InActiveTenants { get; set; }

        public decimal ActiveInActivePercentage
        {
            get
            {
                if (TotalTenants == 0)
                    return 0;
                var prone = Math.Round(((decimal)ActiveTenants / TotalTenants) * 100, 2);
                return prone;
            }
        }

        public static implicit operator AdminDashboardDto(Tenant v)
        {
            throw new NotImplementedException();
        }
    }
}
