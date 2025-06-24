using System;
using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services
{
    public class BloodBankService : IBloodBankService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Campaign> _campaignRepository;

        public BloodBankService(IRepository<User> userRepository, 
            IRepository<Campaign> campaignRepository)
        {
            _userRepository = userRepository;
            _campaignRepository = campaignRepository;
        }

        public object GetBloodBankData(BloodBankFilter filter)
        {
            var user = _userRepository
                .GetAll()
                .Where(u => u.BloodDonationStatus == "Interested");

            if (!string.IsNullOrEmpty(filter.BloodGroup))
            {
                user = FilterByBloodGroup(user, filter.BloodGroup);
            }

            if (!string.IsNullOrEmpty(filter.Upazila))
            {
                user = FilterByUpazila(user, filter.Upazila);
            }

            if (!string.IsNullOrEmpty(filter.Union))
            {
                user = FilterByUnion(user, filter.Union);
            }
            
            if (!string.IsNullOrEmpty(filter.Gender))
            {
                user = FilterByGender(user, filter.Gender);
            }

            if (filter.StartAge is null || filter.EndAge is null)
            {
                filter.StartAge = 0;
                filter.EndAge = 100;
            }

            var startDob = GetDateDifference(filter.StartAge.Value);
            var endDob = GetDateDifference(filter.EndAge.Value);
            var minimumLastDonationDate = GetMinimumLastDonationDate();

            user = FilterByDate(user, startDob, endDob, minimumLastDonationDate);

            if (filter.PageNo is null || filter.PageSize is null)
            {
                filter.PageNo = 1;
                filter.PageSize = 10;
            }

            var totalRowCount = user.Count();

            var userData = user
                .OrderBy(u => u.LastDonationTime)
                .Skip((filter.PageNo.Value - 1) * filter.PageSize.Value)
                .Take(filter.PageSize.Value)
                .Select(u => new BloodBankDonorDataVm()
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    BloodGroup = u.BloodGroup,
                    MobileNumber = u.MobileNumber,
                    Address = u.Address,
                    ImageUrl = u.ImageUrl,
                    GoBangladeshCount = u.BloodDonationCount,
                    LastDonationTime = u.LastDonationTime,
                    LastDonationDayCount = u.LastDonationTime == null ? 0 : (DateTime.Now - u.LastDonationTime.Value).Days,
                    GoBangladeshStatus = u.BloodDonationStatus,
                    PhysicalComplexity = u.PhysicalComplexity
                })
                .ToList();

            return new
            {
                data = userData,
                rowCount = totalRowCount
            };
        }

        private IQueryable<User> FilterByGender(IQueryable<User> user, string gender)
        {
            return user.Where(u => u.Gender == gender);
        }

        private IQueryable<User> FilterByDate(IQueryable<User> user, DateTime startDob, DateTime endDob, DateTime minimumLastDonationDate)
        {
            return user.Where(u => u.Dob <= startDob && u.Dob >= endDob && (u.LastDonationTime <= minimumLastDonationDate || u.LastDonationTime == null));
        }

        private IQueryable<User> FilterByUnion(IQueryable<User> user, string union)
        {
            return user.Where(u => u.Union == union);
        }

        private IQueryable<User> FilterByUpazila(IQueryable<User> user, string upazila)
        {
            return user.Where(u => u.Upazila == upazila);
        }

        private IQueryable<User> FilterByBloodGroup(IQueryable<User> user, string bloodGroup)
        {
            return user.Where(u => u.BloodGroup == bloodGroup);
        }

        public DashboardDataVm GetDashboardData()
        {
            var volunteers = _userRepository
                .GetAll()
                .Where(u => u.UserType == UserTypes.Volunteer)
                .Select(u => u.Id)
                .ToList();

            var dashboardData = new DashboardDataVm()
            {
                Volunteer = volunteers.Count,
                Donor = _userRepository.GetAll().Count(u => u.UserType == UserTypes.Donor && volunteers.Contains(u.CreatedBy)),
                RegisteredDonor = _userRepository.GetAll().Count(u => u.UserType == UserTypes.Donor),
                Campaign = _campaignRepository.GetAll().Count()
            };

            return dashboardData;
        }

        private static DateTime GetMinimumLastDonationDate()
        {
            return DateTime.Now.AddMonths(-4);
        }

        private static DateTime GetDateDifference(int ageToReduce)
        {
            var date = DateTime.Now.AddYears((0-ageToReduce));

            return date;
        }
    }
}
