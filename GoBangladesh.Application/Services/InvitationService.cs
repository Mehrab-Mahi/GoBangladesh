using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Application.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IRepository<InviteUser> _repo;
        private readonly IRepository<User> _userRepo;
        private readonly IBaseRepository _baseRepo;
        private readonly IViewRenderService _viewRenderService;
        private readonly IMailService _mailService;
        public InvitationService(IRepository<InviteUser> repo, IBaseRepository baseRepo, IViewRenderService viewRenderService, IMailService mailService, IRepository<User> userRepo)
        {
            _repo = repo;
            _baseRepo = baseRepo;
            _viewRenderService = viewRenderService;
            _mailService = mailService;
            _userRepo = userRepo;
        }
        public bool Approve(string id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return false;
            }
            if (model.Status != "2")
            {
                return false;
            }
            model.Status = "3";

            _repo.Update(model);
            _repo.SaveChanges();
            UpdateUser(model);

            return true;
        }

        private void UpdateUser(InviteUser vm)
        {
            var model = _userRepo.GetConditional(_=>_.EmailAddress==vm.EmailAddress);
            model.IsApproved = true;

            _userRepo.Update(model);
            _userRepo.SaveChanges();
        }

        public List<InviteUserVm> GetAll()
        {
            var query = @"select ui.*,r.name as roleName
                        from InviteUsers ui
                        join roles r on ui.roleid = r.id";
            return _baseRepo.Query<InviteUserVm>(query);
        }

        public InviteUserVm GetByEmail(string email)
        {
            var query = $"select * from InviteUsers where EmailAddress='{email}'";
            return _baseRepo.FirstOrDefault<InviteUserVm>(query);
        }

        public bool Invite(InviteUserVm model)
        {
            var exist = IsExists(model.EmailAddress);
            if (exist)
            {
                return false;
            }
            var isSaved = SaveInvitation(model);
            if (isSaved)
            {
                SendMail(model);
            }

            return true;
        }

        private async void SendMail(InviteUserVm model)
        {
            model.PublicRegisterUrl = model.PublicRegisterUrl + $"register/invitation/?email={model.EmailAddress}";

            var result = await _viewRenderService.RenderToStringAsync("Mail", model);
            await _mailService.SendEmailAsync(result, model.EmailAddress);
        }

        private bool SaveInvitation(InviteUserVm model)
        {
            var entity = new InviteUser
            {
                EmailAddress = model.EmailAddress,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RoleId = model.RoleId,
                Status = "1"
            };
            try
            {
                _repo.Insert(entity);
                _repo.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Update(InviteUserVm inviteModel)
        {
            try
            {
                var model = _repo.GetConditional(_ => _.EmailAddress == inviteModel.EmailAddress);
                if (model != null)
                {
                    model.Status = inviteModel.Status;
                    _repo.Update(model);
                    _repo.SaveChanges();
                }

            }
            catch (Exception)
            {
            }
        }

        public bool IsExists(string email)
        {
            var query = $"select * from InviteUsers where EmailAddress='{email}'";
            var isExistInvitation = _baseRepo.FirstOrDefault<InviteUserVm>(query);
            if (isExistInvitation != null)
            {
                return true;
            }
            else
            {
                var query2 = $"select * from Users where EmailAddress='{email}'";
                var user = _baseRepo.FirstOrDefault<User>(query);
                if (user == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
