using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebApplication1.DTOs;


namespace WebApplication1.Services
{
    public interface IMemberStatsService
    {
        Task<MemberStatsDto> GetMemberStatsAsync(int memberId);
    }

}