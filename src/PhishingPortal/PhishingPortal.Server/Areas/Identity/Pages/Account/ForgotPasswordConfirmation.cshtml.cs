// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PhishingPortal.Domain;

namespace PhishingPortal.Server.Areas.Identity.Pages.Account
{
    public class ForgotPasswordConfirmation : PageModel
    {
        private readonly UserManager<PhishingPortalUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordConfirmation(UserManager<PhishingPortalUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }
    }
}
