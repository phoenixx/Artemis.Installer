﻿using System.Linq;
using Artemis.Installer.Screens;
using Artemis.Installer.Screens.Abstract;
using Artemis.Installer.Services;
using Artemis.Installer.Services.Prerequisites;
using Artemis.Installer.Stylet;
using Artemis.Installer.Utilities;
using DotNetWindowsRegistry;
using FluentValidation;
using Stylet;
using StyletIoC;

namespace Artemis.Installer
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // View related stuff
            builder.Bind<InstallStepViewModel>().ToAllImplementations();
            builder.Bind<UninstallStepViewModel>().ToAllImplementations();
            builder.Bind(typeof(IPrerequisite)).ToAllImplementations();

            // Services
            builder.Bind<IInstallationService>().To<InstallationService>().InSingletonScope();
            
            // Validation
            builder.Bind(typeof(IModelValidator<>)).To(typeof(FluentValidationAdapter<>));
            builder.Bind(typeof(IValidator<>)).ToAllImplementations();

            // Utilities
            builder.Bind<IRegistry>().To<WindowsRegistry>().InSingletonScope();
            
            base.ConfigureIoC(builder);
        }

        protected override void Configure()
        {
            Container.Get<IInstallationService>().Args = Args.ToList();
            base.Configure();
        }
    }
}