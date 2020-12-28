﻿using System.Threading.Tasks;

namespace Artemis.Installer.Services.Prerequisites
{
    public interface IPrerequisite
    {
        string Title { get; }
        string Description { get; }
        string DownloadUrl { get; }

        bool IsMet();
        Task Install(string file);
    }
}
